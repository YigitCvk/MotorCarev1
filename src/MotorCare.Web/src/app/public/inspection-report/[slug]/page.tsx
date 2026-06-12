'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { ClipboardList, Building2, AlertCircle, Loader2, CheckCircle2, AlertTriangle, XCircle, MinusCircle, Car, User, Printer } from 'lucide-react';
import apiClient from '@/core/api/client';
import { CopyButton } from '@/components/ui/copy-button';
import { dateText } from '@/shared/utils/format';
import { publicInspectionReportUrl } from '@/shared/utils/public-links';
import { VehicleDiagram } from '@/features/inspections/components';
import { buildDamageZones, resolveVehicleDiagramKind } from '@/features/inspections/utils/diagram';

// ─── PII masking ──────────────────────────────────────────────────────────────

// ─── DTOs ────────────────────────────────────────────────────────────────────

interface InspectionItem {
  category: string;
  categoryText: string;
  name: string;
  result: string;
  resultText: string;
  notes: string | null;
  sortOrder: number;
}

interface PublicInspectionReportDto {
  inspectionNo: string;
  businessName: string;
  businessPhone: string | null;
  customerName: string;
  plate: string;
  brand: string | null;
  model: string | null;
  year: number | null;
  mileage: number | null;
  packageType: string;
  packageTypeText: string;
  status: string;
  statusText: string;
  createdAt: string;
  completedAt: string | null;
  generalNotes: string | null;
  items: InspectionItem[];
  vehicleType?: string | null;
  criticalFindingCount: number;
  resultSummary: string;
  verificationText: string;
  testRideNotes: string | null;
  cosmeticNotes: string | null;
}

interface PublicInspectionReportApiDto {
  inspectionNo: string;
  date: string;
  vehiclePlate: string;
  vehicleBrand: string | null;
  vehicleModel: string | null;
  vehicleYear: number | null;
  vehicleMileage: number | null;
  packageType: string;
  status: string;
  isCompleted: boolean;
  criticalFindingCount: number;
  resultSummary: string;
  generalNotes: string | null;
  testRideNotes: string | null;
  cosmeticNotes: string | null;
  items: Array<{
    category: string;
    name: string;
    result: string;
    notes: string | null;
    sortOrder: number;
  }>;
  businessName: string | null;
  verificationText: string;
}

// ─── Result badge ─────────────────────────────────────────────────────────────

type ResultVariant = {
  cls: string;
  icon: React.ReactNode;
};

function resultVariant(result: string): ResultVariant {
  switch (result) {
    case 'Good':
      return {
        cls: 'bg-green-100 text-green-800',
        icon: <CheckCircle2 size={12} className="shrink-0" />,
      };
    case 'Medium':
      return {
        cls: 'bg-yellow-100 text-yellow-800',
        icon: <AlertTriangle size={12} className="shrink-0" />,
      };
    case 'Bad':
      return {
        cls: 'bg-red-100 text-red-800',
        icon: <XCircle size={12} className="shrink-0" />,
      };
    case 'Damaged':
    case 'Scratched':
    case 'Missing':
      return {
        cls: 'bg-orange-100 text-orange-800',
        icon: <AlertTriangle size={12} className="shrink-0" />,
      };
    case 'Changed':
      return {
        cls: 'bg-orange-100 text-orange-800',
        icon: <AlertCircle size={12} className="shrink-0" />,
      };
    case 'Original':
    case 'Exists':
    case 'Painted':
      return {
        cls: 'bg-blue-100 text-blue-800',
        icon: <CheckCircle2 size={12} className="shrink-0" />,
      };
    case 'NotChecked':
    default:
      return {
        cls: 'bg-slate-100 text-slate-500',
        icon: <MinusCircle size={12} className="shrink-0" />,
      };
  }
}

function ResultBadge({ result, resultText }: { result: string; resultText: string }) {
  const { cls, icon } = resultVariant(result);
  return (
    <span
      className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${cls}`}
    >
      {icon}
      {resultText}
    </span>
  );
}

// ─── Status badge ─────────────────────────────────────────────────────────────

const STATUS_CLASSES: Record<string, string> = {
  Pending: 'bg-slate-100 text-slate-700',
  InProgress: 'bg-yellow-100 text-yellow-800',
  Completed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-700',
};

function StatusBadge({ status, statusText }: { status: string; statusText: string }) {
  const cls = STATUS_CLASSES[status] ?? 'bg-slate-100 text-slate-700';
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${cls}`}>
      {statusText}
    </span>
  );
}

// ─── Section header ───────────────────────────────────────────────────────────

function SectionHeader({ icon, title }: { icon: React.ReactNode; title: string }) {
  return (
    <div className="flex items-center gap-2 mb-3 mt-2">
      <span className="text-slate-400">{icon}</span>
      <h2 className="text-sm font-semibold text-slate-800">{title}</h2>
    </div>
  );
}

// ─── Mobile-friendly key-value list ──────────────────────────────────────────

function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-center gap-0.5 sm:gap-3 py-1.5 border-b border-slate-100 last:border-0">
      <dt className="text-xs text-slate-400 sm:w-32 shrink-0">{label}</dt>
      <dd className="text-sm font-medium text-slate-800">{value}</dd>
    </div>
  );
}

// ─── Group items by category ──────────────────────────────────────────────────

function groupByCategory(items: InspectionItem[]): Map<string, InspectionItem[]> {
  const map = new Map<string, InspectionItem[]>();
  const sorted = [...items].sort((a, b) => a.sortOrder - b.sortOrder);
  for (const item of sorted) {
    const key = item.categoryText ?? item.category;
    if (!map.has(key)) map.set(key, []);
    map.get(key)!.push(item);
  }
  return map;
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function PublicInspectionReportPage() {
  const params = useParams();
  const slug = params?.slug as string;

  const { data, isLoading, error } = useQuery<PublicInspectionReportDto>({
    queryKey: ['public-inspection-report', slug],
    queryFn: async () => {
      const { data } = await apiClient.get<PublicInspectionReportApiDto>(
        `/api/public/inspection-report/${encodeURIComponent(slug)}`
      );
      return {
        inspectionNo: data.inspectionNo,
        businessName: data.businessName ?? 'GarajPass',
        businessPhone: null,
        customerName: 'Paylaşılmıyor',
        plate: data.vehiclePlate,
        brand: data.vehicleBrand,
        model: data.vehicleModel,
        year: data.vehicleYear,
        mileage: data.vehicleMileage,
        packageType: data.packageType,
        packageTypeText: data.packageType,
        status: data.status,
        statusText: data.status,
        createdAt: data.date,
        completedAt: data.isCompleted ? data.date : null,
        generalNotes: data.generalNotes,
        items: data.items.map((item) => ({
          ...item,
          categoryText: item.category,
          resultText: item.result,
        })),
        vehicleType: null,
        criticalFindingCount: data.criticalFindingCount,
        resultSummary: data.resultSummary,
        verificationText: data.verificationText,
        testRideNotes: data.testRideNotes,
        cosmeticNotes: data.cosmeticNotes,
      };
    },
    enabled: Boolean(slug),
    retry: 1,
  });

  // ── Loading ──
  if (isLoading) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3 text-slate-400">
          {/* GarajPass brand */}
          <div className="mb-2 text-center">
            <span className="text-slate-700 font-bold text-lg tracking-tight">GarajPass</span>
          </div>
          <Loader2 size={28} className="animate-spin" />
          <p className="text-sm">Yükleniyor...</p>
        </div>
      </div>
    );
  }

  // ── Error / not found ──
  if (error || !data) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 p-8 max-w-sm w-full text-center">
          {/* Brand */}
          <div className="mb-5">
            <span className="text-slate-800 font-bold text-lg tracking-tight">GarajPass</span>
          </div>
          <AlertCircle size={36} className="text-slate-300 mx-auto mb-4" />
          <h1 className="text-lg font-semibold text-slate-800 mb-2">Kayıt bulunamadı</h1>
          <p className="text-sm text-slate-500">
            Bu paylaşım bağlantısı geçersiz veya artık kullanılamıyor.
          </p>
        </div>
      </div>
    );
  }

  const grouped = groupByCategory(data.items ?? []);
  const vehicleLabel = [data.brand, data.model].filter(Boolean).join(' ');
  const publicUrl = publicInspectionReportUrl(slug);
  const damageZones = buildDamageZones(data.items ?? []);
  const diagramKind = resolveVehicleDiagramKind(data.vehicleType);

  return (
    <div className="min-h-screen bg-slate-50 py-6 px-3 sm:px-4 print:bg-white print:py-0">
      {/* Print + layout styles */}
      <style>{`
        @media print {
          .no-print { display: none !important; }
          body { background: white; }
          .print-container { max-width: 100%; padding: 0; }
        }
      `}</style>

      <div className="max-w-2xl mx-auto print-container">

        {/* ── 1. HEADER ── */}
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden mb-3">
          {/* Brand bar */}
          <div className="bg-brand-600 px-4 py-3 flex items-center justify-between print:bg-slate-800">
            <div className="flex items-center gap-2">
              <span className="text-white font-bold text-lg tracking-tight">GarajPass</span>
              <span className="text-brand-200 text-xs print:text-slate-300">Expertiz Raporu</span>
            </div>
            <StatusBadge status={data.status} statusText={data.statusText} />
          </div>

          {/* Business info */}
          <div className="px-4 py-3 bg-slate-50 border-b border-slate-200">
            <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-slate-600">
              <span className="flex items-center gap-1 font-semibold text-slate-800">
                <Building2 size={12} className="text-slate-400 shrink-0" />
                {data.businessName}
              </span>
            </div>
          </div>

          {/* Report number + plate */}
          <div className="px-4 py-4">
            <div className="flex items-start justify-between mb-4 gap-2">
              <div>
                <p className="text-xs text-slate-400 mb-0.5">Plaka</p>
                <p className="text-2xl font-extrabold text-slate-900 tracking-widest leading-none">
                  {data.plate}
                </p>
                {vehicleLabel && (
                  <p className="text-xs text-slate-500 mt-1">
                    {vehicleLabel}
                    {data.year ? ` · ${data.year}` : ''}
                  </p>
                )}
              </div>
              <div className="text-right">
                <p className="text-xs text-slate-400 mb-0.5">Rapor No</p>
                <p className="font-mono text-sm font-semibold text-slate-800">{data.inspectionNo}</p>
              </div>
            </div>

            {/* Print button */}
            <div className="flex flex-wrap justify-end gap-2 mb-1 no-print">
              <CopyButton value={publicUrl} label="Linki Kopyala" />
              <button
                onClick={() => window.print()}
                className="no-print inline-flex items-center gap-1.5 text-xs text-slate-500 hover:text-slate-800 border border-slate-200 hover:border-slate-300 rounded-lg px-3 py-1.5 transition-colors"
              >
                <Printer size={13} />
                Yazdır / PDF Kaydet
              </button>
            </div>
          </div>
        </div>

        {/* ── 2. VEHICLE INFO ── */}
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
          <SectionHeader icon={<Car size={15} />} title="Araç Bilgileri" />
          <dl>
            <InfoRow label="Plaka" value={
              <span className="font-mono font-bold tracking-widest">{data.plate}</span>
            } />
            {vehicleLabel && (
              <InfoRow label="Araç" value={`${vehicleLabel}${data.year ? ` (${data.year})` : ''}`} />
            )}
            {data.mileage != null && (
              <InfoRow label="Kilometre" value={`${data.mileage.toLocaleString('tr-TR')} km`} />
            )}
            <InfoRow label="Paket" value={data.packageTypeText} />
          </dl>
        </div>

        {/* ── 3. REPORT INFO ── */}
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
          <SectionHeader icon={<User size={15} />} title="Rapor Bilgileri" />
          <dl>
            <InfoRow label="Gizlilik" value="Müşteri bilgileri bu public raporda paylaşılmaz." />
            <InfoRow label="Rapor Tarihi" value={dateText(data.createdAt)} />
            {data.completedAt && (
              <InfoRow label="Tamamlanma" value={dateText(data.completedAt)} />
            )}
            <InfoRow label="Sonuç" value={data.resultSummary} />
            <InfoRow label="Kritik Bulgu" value={String(data.criticalFindingCount)} />
          </dl>
        </div>

        {damageZones.length > 0 && (
          <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
            <SectionHeader
              icon={<Car size={15} />}
              title={diagramKind === 'motorcycle' ? 'Motosiklet Diyagramı' : 'Araç Diyagramı'}
            />
            <VehicleDiagram zones={damageZones} vehicleType={diagramKind} />
          </div>
        )}

        {/* ── 4. INSPECTION ITEMS BY CATEGORY ── */}
        {grouped.size > 0 && (
          <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
            <SectionHeader icon={<ClipboardList size={15} />} title="Kontrol Sonuçları" />

            <div className="space-y-5">
              {Array.from(grouped.entries()).map(([categoryText, items]) => (
                <div key={categoryText}>
                  {/* Category header */}
                  <div className="bg-slate-50 -mx-1 px-2 py-1.5 rounded-lg mb-2">
                    <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                      {categoryText}
                    </h3>
                  </div>

                  {/* Items as definition list for mobile */}
                  <dl className="space-y-0">
                    {items.map((item, i) => (
                      <div
                        key={i}
                        className="flex flex-col sm:flex-row sm:items-start gap-1 sm:gap-3 py-2.5 border-b border-slate-100 last:border-0"
                      >
                        <dt className="text-sm text-slate-700 sm:flex-1">{item.name}</dt>
                        <dd className="flex items-start gap-2 sm:flex-col sm:items-end sm:gap-1">
                          <ResultBadge result={item.result} resultText={item.resultText} />
                          {item.notes && (
                            <span className="text-xs text-slate-500 italic">{item.notes}</span>
                          )}
                        </dd>
                      </div>
                    ))}
                  </dl>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* General notes */}
        {data.generalNotes && (
          <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
            <h2 className="text-sm font-semibold text-slate-600 mb-2">Genel Notlar</h2>
            <p className="text-sm text-slate-700 whitespace-pre-line leading-relaxed">
              {data.generalNotes}
            </p>
          </div>
        )}

        {/* Package and verification */}
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-xs text-slate-400 mb-0.5">Ekspertiz Paketi</p>
              <p className="text-sm font-medium text-slate-700">{data.packageTypeText}</p>
            </div>
            <div className="sm:max-w-xs sm:text-right">
              <p className="text-xs text-slate-400 mb-0.5">Doğrulama</p>
              <p className="text-xs font-medium text-slate-700">{data.verificationText}</p>
            </div>
          </div>
        </div>

        {/* ── 5. FOOTER ── */}
        <div className="text-center mt-6 mb-4 space-y-1">
          <p className="text-xs text-slate-400">
            Bu rapor{' '}
            <span className="font-medium text-slate-600">{data.businessName}</span>{' '}
            işletmesi tarafından paylaşılmıştır.
          </p>
          <p className="text-xs text-slate-300">
            Güçlendiren:{' '}
            <span className="font-semibold text-slate-500">GarajPass</span>
          </p>
        </div>
      </div>
    </div>
  );
}
