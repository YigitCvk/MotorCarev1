'use client';

import { use, useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Printer, CheckCircle, XCircle, ClipboardList, Pencil, Globe, GlobeLock, ExternalLink } from 'lucide-react';
import apiClient from '@/core/api/client';
import { useAuth } from '@/core/auth/auth.context';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { QRLinkCard } from '@/components/ui/qr-link-card';
import { money, dateText } from '@/shared/utils/format';
import { publicInspectionReportUrl } from '@/shared/utils/public-links';
import { canManageInspection } from '@/shared/constants/permissions';
import { friendlyError } from '@/core/api/errors';
import { MotorcycleInspectionDiagram } from '@/features/inspections/components';
import type { DamageZone } from '@/features/inspections/components';
import { buildDamageZones as buildInspectionDamageZones } from '@/features/inspections/utils/diagram';
import {
  inspectionCategoryFromApi,
  inspectionResultFromApi,
  inspectionResultToApi,
  inspectionPackageTypeFromApi,
  inspectionStatusFromApi,
} from '@/features/inspections/api-enums';

// ---- DTOs ----------------------------------------------------------------

interface InspectionPublicAccessDto {
  slug: string;
  isActive: boolean;
  lastAccessedAtUtc: string | null;
  accessCount: number;
}

interface MotorcycleInspectionItemDto {
  id: string;
  category: string;
  categoryText: string;
  name: string;
  result: string;
  resultText: string;
  notes: string | null;
  sortOrder: number;
}

interface MotorcycleInspectionDto {
  id: string;
  inspectionNo: string;
  customerId: string | null;
  vehicleId: string | null;
  customerName: string;
  phone: string;
  plate: string;
  brand: string | null;
  model: string | null;
  year: number | null;
  mileage: number | null;
  chassisNumber: string | null;
  engineNumber: string | null;
  query5664: string | null;
  mileageQuery: string | null;
  packageType: string;
  packageTypeText: string;
  status: string;
  statusText: string;
  packagePrice: number;
  generalNotes: string | null;
  testRideNotes: string | null;
  cosmeticNotes: string | null;
  createdAt: string;
  updatedAt: string | null;
  completedAt: string | null;
  items: MotorcycleInspectionItemDto[];
  vehicleType?: string | null;
  motorcycleType?: string | null;
}

// ---- Constants -----------------------------------------------------------

const STATUS_BADGE: Record<string, string> = {
  Draft: 'badge-gray',
  InProgress: 'badge-yellow',
  Completed: 'badge-green',
  Cancelled: 'badge-red',
};

const RESULT_OPTIONS: Array<{ value: string; label: string }> = [
  { value: 'NotChecked', label: 'Kontrol Edilmedi' },
  { value: 'Good', label: 'İyi' },
  { value: 'Medium', label: 'Orta' },
  { value: 'Bad', label: 'Kötü' },
  { value: 'NotAvailable', label: 'Yok (Uygulanamaz)' },
  { value: 'Exists', label: 'Var' },
  { value: 'NotExists', label: 'Yok' },
  { value: 'Damaged', label: 'Hasarlı' },
  { value: 'Painted', label: 'Boyalı' },
  { value: 'Original', label: 'Orijinal' },
  { value: 'Changed', label: 'Değişen' },
  { value: 'Scratched', label: 'Çizik' },
  { value: 'Missing', label: 'Eksik' },
];

const RESULT_COLOR: Record<string, string> = {
  Good: 'text-green-700 bg-green-50',
  Bad: 'text-red-700 bg-red-50',
  Medium: 'text-yellow-700 bg-yellow-50',
  Damaged: 'text-red-700 bg-red-50',
  Scratched: 'text-orange-700 bg-orange-50',
  Missing: 'text-red-700 bg-red-50',
};

function resultColorClass(result: string): string {
  return RESULT_COLOR[result] ?? 'text-slate-500 bg-slate-50';
}

// Results that count as "issues" for the damage diagram
const ISSUE_RESULTS = new Set(['Bad', 'Damaged', 'Scratched', 'Missing', 'Fail', 'Issue', 'Başarısız', 'Sorunlu']);

function buildDamageZones(items: MotorcycleInspectionItemDto[]): DamageZone[] {
  const sharedZones = buildInspectionDamageZones(items, 'motorcycle');
  if (sharedZones.length > 0) return sharedZones;

  const categoryMap = new Map<string, { category: string; hasIssue: boolean }>();
  for (const item of items) {
    const key = item.categoryText || item.name || item.category || 'Genel';
    const existing = categoryMap.get(key);
    const isIssue = ISSUE_RESULTS.has(item.result) || ISSUE_RESULTS.has(item.resultText ?? '');
    if (!existing) {
      categoryMap.set(key, { category: item.category || key, hasIssue: isIssue });
    } else if (isIssue) {
      existing.hasIssue = true;
    }
  }
  return Array.from(categoryMap.entries()).map(([label, val]) => ({
    id: val.category,
    label,
    hasIssue: val.hasIssue,
  }));
}

// ---- Row component -------------------------------------------------------

interface ItemRowProps {
  item: MotorcycleInspectionItemDto;
  inspectionId: string;
  editable: boolean;
}

function ItemRow({ item, inspectionId, editable }: ItemRowProps) {
  const qc = useQueryClient();
  const [localResult, setLocalResult] = useState(item.result);
  const [localNotes, setLocalNotes] = useState(item.notes ?? '');
  const [saving, setSaving] = useState(false);

  // Keep local state in sync if item data re-fetches
  useEffect(() => {
    setLocalResult(item.result);
    setLocalNotes(item.notes ?? '');
  }, [item.result, item.notes]);

  const saveItem = useCallback(
    async (result: string, notes: string) => {
      if (!editable) return;
      setSaving(true);
      try {
        await apiClient.put(`/api/inspections/${inspectionId}/items/${item.id}`, {
          result: inspectionResultToApi(result),
          notes: notes || null,
        });
        void qc.invalidateQueries({ queryKey: ['inspection', inspectionId] });
      } finally {
        setSaving(false);
      }
    },
    [editable, inspectionId, item.id, qc]
  );

  function handleResultChange(e: React.ChangeEvent<HTMLSelectElement>) {
    const val = e.target.value;
    setLocalResult(val);
    void saveItem(val, localNotes);
  }

  function handleNotesBlur() {
    if (localNotes !== (item.notes ?? '')) {
      void saveItem(localResult, localNotes);
    }
  }

  return (
    <tr>
      <td className="text-sm text-slate-800">{item.name}</td>
      <td>
        {editable ? (
          <div className="flex items-center gap-1">
            <select
              className={`input py-1 text-xs font-medium ${resultColorClass(localResult)}`}
              value={localResult}
              onChange={handleResultChange}
              disabled={saving}
            >
              {RESULT_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
            {saving && <span className="text-xs text-slate-400">...</span>}
          </div>
        ) : (
          <span className={`px-2 py-0.5 rounded text-xs font-medium ${resultColorClass(item.result)}`}>
            {item.resultText}
          </span>
        )}
      </td>
      <td>
        {editable ? (
          <input
            type="text"
            className="input py-1 text-sm"
            value={localNotes}
            onChange={(e) => setLocalNotes(e.target.value)}
            onBlur={handleNotesBlur}
            placeholder="Not ekle..."
          />
        ) : (
          <span className="text-sm text-slate-500">{item.notes ?? '-'}</span>
        )}
      </td>
    </tr>
  );
}

// ---- Main page -----------------------------------------------------------

export default function InspectionDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const router = useRouter();
  const qc = useQueryClient();
  const { user } = useAuth();
  const [actionError, setActionError] = useState('');

  const { data, isLoading, error, refetch } = useQuery<MotorcycleInspectionDto>({
    queryKey: ['inspection', id],
    queryFn: async () => {
      const { data } = await apiClient.get<MotorcycleInspectionDto>(`/api/inspections/${id}`);
      return {
        ...data,
        packageType: inspectionPackageTypeFromApi(data.packageType),
        status: inspectionStatusFromApi(data.status),
        items: data.items.map((item) => ({
          ...item,
          category: inspectionCategoryFromApi(item.category),
          result: inspectionResultFromApi(item.result),
        })),
      };
    },
  });

  const {
    data: publicAccess,
    error: publicAccessError,
    refetch: refetchPublicAccess,
  } = useQuery<InspectionPublicAccessDto | null>({
    queryKey: ['inspection', id, 'public-access'],
    queryFn: async () => {
      try {
        const { data: d } = await apiClient.get<InspectionPublicAccessDto>(`/api/inspections/${id}/public-access`);
        return d;
      } catch (err: unknown) {
        const status = (err as { response?: { status?: number } })?.response?.status;
        if (status === 404) return null;
        throw err;
      }
    },
    staleTime: 60_000,
    throwOnError: false,
  });

  const enablePublicAccessMutation = useMutation({
    mutationFn: async () => {
      await apiClient.post(`/api/inspections/${id}/public-access`);
      await apiClient.put(`/api/inspections/${id}/public-access/enable`);
    },
    onSuccess: () => {
      void refetchPublicAccess();
      void qc.invalidateQueries({ queryKey: ['inspection', id] });
      setActionError('');
    },
    onError: (err: unknown) => setActionError(friendlyError(err, 'Erişim etkinleştirilemedi.')),
  });

  const disablePublicAccessMutation = useMutation({
    mutationFn: async () => {
      await apiClient.put(`/api/inspections/${id}/public-access/disable`);
    },
    onSuccess: () => {
      void refetchPublicAccess();
      void qc.invalidateQueries({ queryKey: ['inspection', id] });
      setActionError('');
    },
    onError: (err: unknown) => setActionError(friendlyError(err, 'Erişim devre dışı bırakılamadı.')),
  });

  const completeMutation = useMutation({
    mutationFn: async () => {
      await apiClient.put(`/api/inspections/${id}/complete`);
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['inspection', id] });
      void qc.invalidateQueries({ queryKey: ['inspections'] });
      setActionError('');
    },
    onError: (err: unknown) => {
      setActionError(friendlyError(err, 'Ekspertiz tamamlanamadı.'));
    },
  });

  const cancelMutation = useMutation({
    mutationFn: async () => {
      await apiClient.put(`/api/inspections/${id}/cancel`);
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['inspection', id] });
      void qc.invalidateQueries({ queryKey: ['inspections'] });
      setActionError('');
    },
    onError: (err: unknown) => {
      setActionError(friendlyError(err, 'Ekspertiz iptal edilemedi.'));
    },
  });

  if (isLoading) return <PageLoading />;
  if (error || !data) return <ErrorState message="Ekspertiz bulunamadı." onRetry={() => void refetch()} />;

  const canWriteInspection = canManageInspection(user?.role);
  const hasEditableStatus = data.status === 'Draft' || data.status === 'InProgress';
  const editable = canWriteInspection && hasEditableStatus;
  const canComplete = canWriteInspection && hasEditableStatus;
  const canCancel = canWriteInspection && hasEditableStatus;

  const publicSlug = publicAccess?.slug?.trim() || null;
  const publicUrl = publicSlug ? publicInspectionReportUrl(publicSlug) : null;
  const publicAccessErrorMessage = publicAccessError
    ? friendlyError(publicAccessError, 'Paylaşım bilgileri yüklenemedi.')
    : '';
  // Build damage zones for the diagram
  const damageZones = buildDamageZones(data.items);
  const checkedItemCount = data.items.filter((item) => item.result !== 'NotChecked').length;
  const problemZoneCount = damageZones.filter((zone) => zone.hasIssue).length;

  // Group items by category
  const grouped = data.items.reduce<Record<string, MotorcycleInspectionItemDto[]>>((acc, item) => {
    const key = item.categoryText || item.name || item.category || 'Genel';
    if (!acc[key]) acc[key] = [];
    acc[key].push(item);
    return acc;
  }, {});

  const categoryOrder = [
    'MechanicalAndRunningGear',
    'BodyAndFairing',
    'ObdAndElectrical',
    'TestRide',
    'General',
  ];

  const sortedCategories = Object.entries(grouped).sort(([, aItems], [, bItems]) => {
    const aOrder = categoryOrder.indexOf(aItems[0].category);
    const bOrder = categoryOrder.indexOf(bItems[0].category);
    return (aOrder === -1 ? 99 : aOrder) - (bOrder === -1 ? 99 : bOrder);
  });

  return (
    <div>
      {/* Top nav */}
      <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <button onClick={() => router.push('/inspections')} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Ekspertizler
        </button>
        <div className="flex flex-wrap items-center gap-2">
          {editable && (
            <Link href={`/inspections/${id}/edit`} className="btn text-sm">
              <Pencil size={14} />
              Düzenle
            </Link>
          )}
          <button
            onClick={() => router.push(`/inspections/${id}/print`)}
            className="btn text-sm"
          >
            <Printer size={14} />
            Yazdır
          </button>
        </div>
      </div>

      {/* Sharing panel */}
      <div className="mb-4 card p-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          {publicAccess?.isActive
            ? <Globe size={16} className="text-green-600" />
            : <GlobeLock size={16} className="text-slate-400" />}
          <div>
            <p className="text-sm font-medium text-slate-900">
              {publicAccessError
                ? 'Paylaşım durumu alınamadı'
                : publicAccess?.isActive
                  ? 'Genel Erişim Açık'
                  : 'Genel Erişim Kapalı'}
            </p>
            {publicAccess?.isActive && publicUrl && (
              <a href={publicUrl} target="_blank" rel="noreferrer" className="flex items-center gap-1 text-xs text-brand-600 hover:underline">
                {publicUrl} <ExternalLink size={10} />
              </a>
            )}
          </div>
        </div>
        <div className="flex items-center gap-2">
          {publicAccess?.isActive && (
            <span className="text-xs text-slate-400">{publicAccess.accessCount} görüntülenme</span>
          )}
          {canWriteInspection && !publicAccessError && (publicAccess?.isActive ? (
            <button
              onClick={() => disablePublicAccessMutation.mutate()}
              disabled={disablePublicAccessMutation.isPending}
              className="btn text-sm text-rose-600 border-rose-200 hover:bg-rose-50"
            >
              <GlobeLock size={13} />
              {disablePublicAccessMutation.isPending ? '...' : 'Kapat'}
            </button>
          ) : (
            <button
              onClick={() => enablePublicAccessMutation.mutate()}
              disabled={enablePublicAccessMutation.isPending}
              className="btn-primary text-sm"
            >
              <Globe size={13} />
              {enablePublicAccessMutation.isPending ? '...' : 'Paylaşımı Aç'}
            </button>
          ))}
        </div>
      </div>

      {publicAccessErrorMessage && (
        <div className="alert-error mb-4">{publicAccessErrorMessage}</div>
      )}

      {!publicAccessError && !publicUrl && (
        <div className="mb-4 text-sm text-slate-500">
          Paylaşım bağlantısı henüz oluşturulmamış.
        </div>
      )}

      {publicAccess?.isActive && (
        <div className="mb-4">
          <QRLinkCard
            href={publicUrl}
            title="Ekspertiz paylaşım QR kodu"
            description="Müşteri ekspertiz raporunu bu QR veya bağlantı ile görüntüleyebilir."
            emptyText="Paylaşım bağlantısı henüz oluşturulmamış."
          />
        </div>
      )}

      {/* Header */}
      <div className="flex flex-wrap items-start justify-between gap-4 mb-6">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <ClipboardList size={20} className="text-slate-400" />
            <h1 className="text-2xl font-bold text-slate-900">{data.inspectionNo}</h1>
            <span className={STATUS_BADGE[data.status] ?? 'badge-gray'}>{data.statusText}</span>
          </div>
          <p className="text-slate-500 text-sm">
            {data.customerName} &bull; <span className="font-mono">{data.plate}</span>
          </p>
        </div>
        <div className="flex gap-2">
          {canComplete && (
            <button
              className="btn-primary text-sm"
              disabled={completeMutation.isPending}
              onClick={() => completeMutation.mutate()}
            >
              <CheckCircle size={14} />
              {completeMutation.isPending ? 'Tamamlanıyor...' : 'Tamamla'}
            </button>
          )}
          {canCancel && (
            <button
              className="btn-danger text-sm"
              disabled={cancelMutation.isPending}
              onClick={() => cancelMutation.mutate()}
            >
              <XCircle size={14} />
              {cancelMutation.isPending ? 'İptal Ediliyor...' : 'İptal Et'}
            </button>
          )}
        </div>
      </div>

      {actionError && <div className="alert-error mb-4">{actionError}</div>}

      <div className="grid grid-cols-2 gap-3 mb-6 lg:grid-cols-4">
        {[
          { label: 'Kontrol kalemi', value: data.items.length },
          { label: 'İşlenen kalem', value: checkedItemCount },
          { label: 'Sorunlu bölge', value: problemZoneCount },
          { label: 'Paket', value: data.packageTypeText },
        ].map((item) => (
          <div key={item.label} className="rounded-lg border border-slate-200 bg-white p-3">
            <p className="text-xs text-slate-400">{item.label}</p>
            <p className="mt-1 text-lg font-semibold text-slate-900">{item.value}</p>
          </div>
        ))}
      </div>

      {/* Info cards */}
      <div className="grid sm:grid-cols-2 gap-4 mb-6">
        {/* Customer & vehicle */}
        <div className="card p-5">
          <h2 className="text-sm font-semibold text-slate-500 uppercase tracking-wide mb-3">
            Müşteri &amp; Araç
          </h2>
          <dl className="space-y-2">
            {(
              [
                { label: 'Müşteri', value: data.customerName },
                { label: 'Telefon', value: data.phone },
                { label: 'Plaka', value: data.plate, mono: true },
                { label: 'Marka / Model', value: [data.brand, data.model].filter(Boolean).join(' ') || null },
                { label: 'Yıl', value: data.year ? String(data.year) : null },
                { label: 'Kilometre', value: data.mileage ? `${data.mileage.toLocaleString('tr-TR')} km` : null },
                { label: 'Şasi No', value: data.chassisNumber },
                { label: 'Motor No', value: data.engineNumber },
                { label: '5664 Sorgu', value: data.query5664 },
                { label: 'KM Sorgu', value: data.mileageQuery },
              ] as Array<{ label: string; value: string | null; mono?: boolean }>
            )
              .filter((r) => Boolean(r.value))
              .map((r) => (
                <div key={r.label} className="flex gap-2">
                  <dt className="text-xs text-slate-400 w-28 shrink-0">{r.label}</dt>
                  <dd className={`text-sm text-slate-800 ${r.mono ? 'font-mono' : ''}`}>{r.value}</dd>
                </div>
              ))}
          </dl>
        </div>

        {/* Package & dates */}
        <div className="card p-5">
          <h2 className="text-sm font-semibold text-slate-500 uppercase tracking-wide mb-3">
            Paket &amp; Tarihler
          </h2>
          <dl className="space-y-2">
            {(
              [
                { label: 'Paket', value: data.packageTypeText },
                { label: 'Ücret', value: money(data.packagePrice) },
                { label: 'Oluşturulma', value: dateText(data.createdAt) },
                { label: 'Güncelleme', value: dateText(data.updatedAt) },
                { label: 'Tamamlanma', value: dateText(data.completedAt) },
              ] as Array<{ label: string; value: string }>
            )
              .filter((r) => r.value && r.value !== '-')
              .map((r) => (
                <div key={r.label} className="flex gap-2">
                  <dt className="text-xs text-slate-400 w-28 shrink-0">{r.label}</dt>
                  <dd className="text-sm text-slate-800">{r.value}</dd>
                </div>
              ))}
          </dl>

          {/* Notes */}
          {(data.generalNotes || data.testRideNotes || data.cosmeticNotes) && (
            <div className="mt-4 pt-4 border-t border-slate-100 space-y-2">
              {data.generalNotes && (
                <div>
                  <p className="text-xs text-slate-400">Genel Notlar</p>
                  <p className="text-sm text-slate-700 whitespace-pre-line">{data.generalNotes}</p>
                </div>
              )}
              {data.testRideNotes && (
                <div>
                  <p className="text-xs text-slate-400">Test Sürüşü</p>
                  <p className="text-sm text-slate-700 whitespace-pre-line">{data.testRideNotes}</p>
                </div>
              )}
              {data.cosmeticNotes && (
                <div>
                  <p className="text-xs text-slate-400">Kozmetik</p>
                  <p className="text-sm text-slate-700 whitespace-pre-line">{data.cosmeticNotes}</p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Vehicle damage diagram */}
      {damageZones.length > 0 && (
        <div className="card p-5 mb-6">
          <MotorcycleInspectionDiagram
            motorcycleType={data.motorcycleType}
            zones={damageZones}
          />
        </div>
      )}

      {/* Inspection items */}
      {sortedCategories.length > 0 && (
        <div className="space-y-5">
          {sortedCategories.map(([categoryText, items]) => (
            <div key={categoryText} className="card p-0 overflow-hidden">
              <div className="px-4 py-3 bg-slate-50 border-b border-slate-100">
                <h3 className="text-sm font-semibold text-slate-700">{categoryText}</h3>
              </div>
              <div className="table-container">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Kontrol Kalemi</th>
                      <th style={{ width: '200px' }}>Sonuç</th>
                      <th>Notlar</th>
                    </tr>
                  </thead>
                  <tbody>
                    {items
                      .slice()
                      .sort((a, b) => a.sortOrder - b.sortOrder)
                      .map((item) => (
                        <ItemRow
                          key={item.id}
                          item={item}
                          inspectionId={id}
                          editable={editable}
                        />
                      ))}
                  </tbody>
                </table>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Price summary */}
      <div className="mt-6 flex justify-end">
        <div className="card p-4 min-w-[240px]">
          <div className="flex justify-between items-center text-sm text-slate-600 mb-1">
            <span>{data.packageTypeText}</span>
          </div>
          <div className="flex justify-between items-center font-semibold text-lg text-slate-900">
            <span>Toplam</span>
            <span>{money(data.packagePrice)}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
