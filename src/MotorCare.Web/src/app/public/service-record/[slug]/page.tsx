'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Wrench, Package, Droplets, FileText, Building2, AlertCircle, Loader2, Printer, Car, User } from 'lucide-react';
import apiClient from '@/core/api/client';
import { money, dateText } from '@/shared/utils/format';

// ─── PII masking ──────────────────────────────────────────────────────────────

function maskName(fullName: string): string {
  if (!fullName) return '';

  // Email address: mask middle of local part
  if (fullName.includes('@')) {
    const [local, domain] = fullName.split('@');
    if (!domain) return fullName;
    const masked = local.length <= 1 ? local : `${local[0]}***`;
    return `${masked}@${domain}`;
  }

  const parts = fullName.trim().split(/\s+/);

  // Single word: show first 3 chars + ***
  if (parts.length === 1) {
    const name = parts[0];
    return name.length <= 3 ? name : `${name.slice(0, 3)}***`;
  }

  // Multi-word: show first name fully, mask last name to initial + "."
  const firstName = parts[0];
  const lastInitial = parts[parts.length - 1][0];
  return `${firstName} ${lastInitial}.`;
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

interface OperationItem {
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface PartItem {
  partName: string;
  partNumber: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface ConsumableItem {
  category: string;
  productName: string;
  brand: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface PublicServiceRecordDto {
  orderNo: string;
  businessName: string;
  businessPhone: string | null;
  businessAddress: string | null;
  vehiclePlate: string;
  vehicleDisplay: string | null;
  vehicleKm: number;
  customerName: string;
  status: string;
  statusText: string;
  openedAt: string;
  closedAt: string | null;
  complaint: string | null;
  workDescription: string | null;
  operations: OperationItem[];
  parts: PartItem[];
  consumables: ConsumableItem[];
  laborTotal: number;
  partsTotal: number;
  consumablesTotal: number;
  grandTotal: number;
}

// ─── Status badge ─────────────────────────────────────────────────────────────

const STATUS_CLASSES: Record<string, string> = {
  Open: 'bg-blue-100 text-blue-800',
  InProgress: 'bg-yellow-100 text-yellow-800',
  Completed: 'bg-green-100 text-green-800',
  Delivered: 'bg-slate-100 text-slate-700',
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

// ─── Mobile-friendly line item card ──────────────────────────────────────────

function LineItemCard({
  title,
  subtitle,
  qty,
  unitPrice,
  lineTotal,
}: {
  title: string;
  subtitle?: string | null;
  qty: number;
  unitPrice: number;
  lineTotal: number;
}) {
  return (
    <div className="flex items-start justify-between gap-3 py-3 border-b border-slate-100 last:border-0">
      <div className="min-w-0 flex-1">
        <p className="text-sm text-slate-800 break-words">{title}</p>
        {subtitle && <p className="text-xs text-slate-400 font-mono mt-0.5">{subtitle}</p>}
        <p className="text-xs text-slate-500 mt-0.5">
          {qty} × {money(unitPrice)}
        </p>
      </div>
      <p className="text-sm font-semibold text-slate-900 shrink-0">{money(lineTotal)}</p>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function PublicServiceRecordPage() {
  const params = useParams();
  const slug = params?.slug as string;

  const { data, isLoading, error } = useQuery<PublicServiceRecordDto>({
    queryKey: ['public-service-record', slug],
    queryFn: async () => {
      const { data } = await apiClient.get<PublicServiceRecordDto>(
        `/api/public/service-record/${encodeURIComponent(slug)}`
      );
      return data;
    },
    enabled: Boolean(slug),
    retry: 1,
  });

  // ── Loading ──
  if (isLoading) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3 text-slate-400">
          {/* BakımSuite brand */}
          <div className="mb-2 text-center">
            <span className="text-slate-700 font-bold text-lg tracking-tight">BakımSuite</span>
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
            <span className="text-slate-800 font-bold text-lg tracking-tight">BakımSuite</span>
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

  const hasOperations = (data.operations?.length ?? 0) > 0;
  const hasParts = (data.parts?.length ?? 0) > 0;
  const hasConsumables = (data.consumables?.length ?? 0) > 0;

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
              <span className="text-white font-bold text-lg tracking-tight">BakımSuite</span>
              <span className="text-brand-200 text-xs print:text-slate-300">Servis Kaydı</span>
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

          {/* Order number + plate */}
          <div className="px-4 py-4">
            <div className="flex items-start justify-between mb-4 gap-2">
              <div>
                <p className="text-xs text-slate-400 mb-0.5">Plaka</p>
                <p className="text-2xl font-extrabold text-slate-900 tracking-widest leading-none">
                  {data.vehiclePlate}
                </p>
                {data.vehicleDisplay && (
                  <p className="text-xs text-slate-500 mt-1">{data.vehicleDisplay}</p>
                )}
              </div>
              <div className="text-right">
                <p className="text-xs text-slate-400 mb-0.5">Sipariş No</p>
                <p className="font-mono text-sm font-semibold text-slate-800">{data.orderNo}</p>
              </div>
            </div>

            {/* Print button */}
            <div className="flex justify-end mb-1 no-print">
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
            {data.vehicleDisplay && <InfoRow label="Araç" value={data.vehicleDisplay} />}
            <InfoRow label="Plaka" value={
              <span className="font-mono font-bold tracking-widest">{data.vehiclePlate}</span>
            } />
            <InfoRow label="Kilometre" value={
              data.vehicleKm != null
                ? `${data.vehicleKm.toLocaleString('tr-TR')} km`
                : '-'
            } />
          </dl>
        </div>

        {/* ── 3. CUSTOMER INFO ── */}
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
          <SectionHeader icon={<User size={15} />} title="Müşteri Bilgileri" />
          <dl>
            <InfoRow label="Müşteri" value={maskName(data.customerName)} />
            <InfoRow label="Açılış Tarihi" value={dateText(data.openedAt)} />
            {data.closedAt && (
              <InfoRow label="Kapanış Tarihi" value={dateText(data.closedAt)} />
            )}
          </dl>
        </div>

        {/* ── 4. SERVICE SUMMARY ── */}

        {/* Complaint */}
        {data.complaint && (
          <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
            <h2 className="text-sm font-semibold text-slate-600 mb-2">Müşteri Şikayeti</h2>
            <p className="text-sm text-slate-700 whitespace-pre-line leading-relaxed">{data.complaint}</p>
          </div>
        )}

        {/* Operations */}
        {hasOperations && (
          <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
            <SectionHeader icon={<Wrench size={15} />} title="İşçilik" />
            <div>
              {data.operations.map((op, i) => (
                <LineItemCard
                  key={i}
                  title={op.description}
                  qty={op.quantity}
                  unitPrice={op.unitPrice}
                  lineTotal={op.lineTotal}
                />
              ))}
            </div>
          </div>
        )}

        {/* Parts */}
        {hasParts && (
          <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
            <SectionHeader icon={<Package size={15} />} title="Yedek Parçalar" />
            <div>
              {data.parts.map((p, i) => (
                <LineItemCard
                  key={i}
                  title={p.partName}
                  subtitle={p.partNumber}
                  qty={p.quantity}
                  unitPrice={p.unitPrice}
                  lineTotal={p.lineTotal}
                />
              ))}
            </div>
          </div>
        )}

        {/* Consumables */}
        {hasConsumables && (
          <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
            <SectionHeader icon={<Droplets size={15} />} title="Sarf Malzemeleri" />
            <div>
              {data.consumables.map((c, i) => (
                <LineItemCard
                  key={i}
                  title={c.productName}
                  subtitle={`${c.brand} · ${c.category}`}
                  qty={c.quantity}
                  unitPrice={c.unitPrice}
                  lineTotal={c.lineTotal}
                />
              ))}
            </div>
          </div>
        )}

        {/* Cost summary */}
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
          <div className="max-w-xs ml-auto space-y-1.5 text-sm">
            {hasOperations && (
              <div className="flex justify-between text-slate-600">
                <span>İşçilik</span>
                <span>{money(data.laborTotal)}</span>
              </div>
            )}
            {hasParts && (
              <div className="flex justify-between text-slate-600">
                <span>Parçalar</span>
                <span>{money(data.partsTotal)}</span>
              </div>
            )}
            {hasConsumables && (
              <div className="flex justify-between text-slate-600">
                <span>Sarf Mal.</span>
                <span>{money(data.consumablesTotal)}</span>
              </div>
            )}
            <div className="flex justify-between font-bold text-slate-900 text-base border-t border-slate-200 pt-2 mt-1">
              <span>Toplam</span>
              <span>{money(data.grandTotal)}</span>
            </div>
          </div>
        </div>

        {/* Work description */}
        {data.workDescription && (
          <div className="bg-white rounded-2xl shadow-sm border border-slate-200 px-4 py-4 mb-3">
            <SectionHeader icon={<FileText size={15} />} title="Yapılan İşler" />
            <p className="text-sm text-slate-700 whitespace-pre-line leading-relaxed">{data.workDescription}</p>
          </div>
        )}

        {/* ── 5. FOOTER ── */}
        <div className="text-center mt-6 mb-4 space-y-1">
          <p className="text-xs text-slate-400">
            Bu kayıt{' '}
            <span className="font-medium text-slate-600">{data.businessName}</span>{' '}
            işletmesi tarafından paylaşılmıştır.
          </p>
          <p className="text-xs text-slate-300">
            Güçlendiren:{' '}
            <span className="font-semibold text-slate-500">BakımSuite</span>
          </p>
        </div>
      </div>
    </div>
  );
}
