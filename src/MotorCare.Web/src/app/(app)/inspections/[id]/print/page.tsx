'use client';

import { use } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Printer } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { money, dateText } from '@/shared/utils/format';

// ---- DTOs ----------------------------------------------------------------

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
}

// ---- Constants -----------------------------------------------------------

const RESULT_PRINT_COLOR: Record<string, string> = {
  Good: '#15803d',
  Bad: '#b91c1c',
  Medium: '#b45309',
  Damaged: '#b91c1c',
  Scratched: '#c2410c',
  Missing: '#b91c1c',
  NotChecked: '#64748b',
};

const CATEGORY_ORDER = [
  'MechanicalAndRunningGear',
  'BodyAndFairing',
  'ObdAndElectrical',
  'TestRide',
  'General',
];

function resultColor(result: string): string {
  return RESULT_PRINT_COLOR[result] ?? '#475569';
}

// ---- Page ----------------------------------------------------------------

export default function InspectionPrintPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);

  const { data, isLoading, error, refetch } = useQuery<MotorcycleInspectionDto>({
    queryKey: ['inspection-print', id],
    queryFn: async () => {
      const { data } = await apiClient.get<MotorcycleInspectionDto>(`/api/inspections/${id}`);
      return data;
    },
  });

  if (isLoading) return <PageLoading />;
  if (error || !data) return <ErrorState message="Ekspertiz bulunamadı." onRetry={() => void refetch()} />;

  // Group & sort items
  const grouped = data.items.reduce<Record<string, MotorcycleInspectionItemDto[]>>((acc, item) => {
    const key = item.categoryText;
    if (!acc[key]) acc[key] = [];
    acc[key].push(item);
    return acc;
  }, {});

  const sortedCategories = Object.entries(grouped).sort(([, aItems], [, bItems]) => {
    const aOrder = CATEGORY_ORDER.indexOf(aItems[0].category);
    const bOrder = CATEGORY_ORDER.indexOf(bItems[0].category);
    return (aOrder === -1 ? 99 : aOrder) - (bOrder === -1 ? 99 : bOrder);
  });

  return (
    <>
      {/* Print styles */}
      <style>{`
        @media print {
          .no-print { display: none !important; }
          body { background: white !important; }
          .print-page { padding: 0 !important; margin: 0 !important; }
        }
        @page {
          margin: 1.5cm;
        }
      `}</style>

      {/* Print button — hidden on print */}
      <div className="no-print fixed top-4 right-4 z-50 flex gap-2">
        <button
          onClick={() => window.print()}
          className="btn-primary"
        >
          <Printer size={16} />
          Yazdır / PDF
        </button>
        <button
          onClick={() => window.history.back()}
          className="btn"
        >
          Geri
        </button>
      </div>

      {/* Print page content */}
      <div className="print-page max-w-4xl mx-auto px-6 py-8 bg-white text-slate-900">

        {/* Business header */}
        <div className="flex items-center justify-between border-b-2 border-slate-800 pb-4 mb-6">
          <div>
            <h1 className="text-2xl font-bold text-slate-900">MotorCare</h1>
            <p className="text-sm text-slate-500">Motorsiklet Ekspertiz Raporu</p>
          </div>
          <div className="text-right">
            <p className="text-xl font-bold tracking-widest text-slate-800">{data.inspectionNo}</p>
            <p className="text-sm text-slate-500">{dateText(data.createdAt)}</p>
            <p className="text-sm font-medium text-slate-700">{data.statusText}</p>
          </div>
        </div>

        {/* Customer & Vehicle info */}
        <div className="grid grid-cols-2 gap-6 mb-6">
          <div>
            <h2 className="text-xs font-semibold uppercase tracking-widest text-slate-400 mb-2">
              Müşteri Bilgileri
            </h2>
            <table style={{ borderCollapse: 'collapse', width: '100%' }}>
              <tbody>
                {(
                  [
                    { label: 'Ad Soyad', value: data.customerName },
                    { label: 'Telefon', value: data.phone },
                  ] as Array<{ label: string; value: string | null }>
                )
                  .filter((r) => Boolean(r.value))
                  .map((r) => (
                    <tr key={r.label}>
                      <td
                        style={{
                          fontSize: '11px',
                          color: '#94a3b8',
                          paddingRight: '12px',
                          paddingBottom: '4px',
                          whiteSpace: 'nowrap',
                          verticalAlign: 'top',
                        }}
                      >
                        {r.label}
                      </td>
                      <td style={{ fontSize: '13px', color: '#1e293b', paddingBottom: '4px' }}>
                        {r.value}
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>

          <div>
            <h2 className="text-xs font-semibold uppercase tracking-widest text-slate-400 mb-2">
              Araç Bilgileri
            </h2>
            <table style={{ borderCollapse: 'collapse', width: '100%' }}>
              <tbody>
                {(
                  [
                    { label: 'Plaka', value: data.plate },
                    { label: 'Marka / Model', value: [data.brand, data.model].filter(Boolean).join(' ') || null },
                    { label: 'Yıl', value: data.year ? String(data.year) : null },
                    { label: 'Kilometre', value: data.mileage ? `${data.mileage.toLocaleString('tr-TR')} km` : null },
                    { label: 'Şasi No', value: data.chassisNumber },
                    { label: 'Motor No', value: data.engineNumber },
                    { label: '5664 Sorgu', value: data.query5664 },
                    { label: 'KM Sorgu', value: data.mileageQuery },
                  ] as Array<{ label: string; value: string | null }>
                )
                  .filter((r) => Boolean(r.value))
                  .map((r) => (
                    <tr key={r.label}>
                      <td
                        style={{
                          fontSize: '11px',
                          color: '#94a3b8',
                          paddingRight: '12px',
                          paddingBottom: '4px',
                          whiteSpace: 'nowrap',
                          verticalAlign: 'top',
                        }}
                      >
                        {r.label}
                      </td>
                      <td
                        style={{
                          fontSize: '13px',
                          color: '#1e293b',
                          paddingBottom: '4px',
                          fontFamily: r.label === 'Plaka' ? 'monospace' : undefined,
                          fontWeight: r.label === 'Plaka' ? 700 : undefined,
                        }}
                      >
                        {r.value}
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Inspection items by category */}
        {sortedCategories.map(([categoryText, items]) => (
          <div key={categoryText} className="mb-5">
            <h2
              style={{
                fontSize: '12px',
                fontWeight: 700,
                textTransform: 'uppercase',
                letterSpacing: '0.08em',
                color: '#475569',
                borderBottom: '1px solid #e2e8f0',
                paddingBottom: '4px',
                marginBottom: '6px',
              }}
            >
              {categoryText}
            </h2>
            <table style={{ borderCollapse: 'collapse', width: '100%' }}>
              <thead>
                <tr style={{ backgroundColor: '#f8fafc' }}>
                  <th
                    style={{
                      textAlign: 'left',
                      fontSize: '11px',
                      fontWeight: 600,
                      color: '#64748b',
                      padding: '5px 8px',
                      borderBottom: '1px solid #e2e8f0',
                    }}
                  >
                    Kontrol Kalemi
                  </th>
                  <th
                    style={{
                      textAlign: 'left',
                      fontSize: '11px',
                      fontWeight: 600,
                      color: '#64748b',
                      padding: '5px 8px',
                      borderBottom: '1px solid #e2e8f0',
                      width: '140px',
                    }}
                  >
                    Sonuç
                  </th>
                  <th
                    style={{
                      textAlign: 'left',
                      fontSize: '11px',
                      fontWeight: 600,
                      color: '#64748b',
                      padding: '5px 8px',
                      borderBottom: '1px solid #e2e8f0',
                    }}
                  >
                    Notlar
                  </th>
                </tr>
              </thead>
              <tbody>
                {items
                  .slice()
                  .sort((a, b) => a.sortOrder - b.sortOrder)
                  .map((item, idx) => (
                    <tr
                      key={item.id}
                      style={{ backgroundColor: idx % 2 === 0 ? '#ffffff' : '#f8fafc' }}
                    >
                      <td
                        style={{
                          fontSize: '12px',
                          color: '#1e293b',
                          padding: '5px 8px',
                          borderBottom: '1px solid #f1f5f9',
                        }}
                      >
                        {item.name}
                      </td>
                      <td
                        style={{
                          fontSize: '12px',
                          fontWeight: 600,
                          color: resultColor(item.result),
                          padding: '5px 8px',
                          borderBottom: '1px solid #f1f5f9',
                        }}
                      >
                        {item.resultText}
                      </td>
                      <td
                        style={{
                          fontSize: '12px',
                          color: '#64748b',
                          padding: '5px 8px',
                          borderBottom: '1px solid #f1f5f9',
                        }}
                      >
                        {item.notes ?? ''}
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
        ))}

        {/* Notes section */}
        {(data.generalNotes || data.testRideNotes || data.cosmeticNotes) && (
          <div className="mb-5 grid grid-cols-1 gap-3">
            {data.generalNotes && (
              <div>
                <p
                  style={{
                    fontSize: '11px',
                    fontWeight: 700,
                    textTransform: 'uppercase',
                    letterSpacing: '0.08em',
                    color: '#64748b',
                    marginBottom: '2px',
                  }}
                >
                  Genel Notlar
                </p>
                <p style={{ fontSize: '13px', color: '#334155', whiteSpace: 'pre-line' }}>
                  {data.generalNotes}
                </p>
              </div>
            )}
            {data.testRideNotes && (
              <div>
                <p
                  style={{
                    fontSize: '11px',
                    fontWeight: 700,
                    textTransform: 'uppercase',
                    letterSpacing: '0.08em',
                    color: '#64748b',
                    marginBottom: '2px',
                  }}
                >
                  Test Sürüşü Notları
                </p>
                <p style={{ fontSize: '13px', color: '#334155', whiteSpace: 'pre-line' }}>
                  {data.testRideNotes}
                </p>
              </div>
            )}
            {data.cosmeticNotes && (
              <div>
                <p
                  style={{
                    fontSize: '11px',
                    fontWeight: 700,
                    textTransform: 'uppercase',
                    letterSpacing: '0.08em',
                    color: '#64748b',
                    marginBottom: '2px',
                  }}
                >
                  Kozmetik Notlar
                </p>
                <p style={{ fontSize: '13px', color: '#334155', whiteSpace: 'pre-line' }}>
                  {data.cosmeticNotes}
                </p>
              </div>
            )}
          </div>
        )}

        {/* Totals */}
        <div
          style={{
            borderTop: '2px solid #1e293b',
            paddingTop: '12px',
            display: 'flex',
            justifyContent: 'flex-end',
          }}
        >
          <table style={{ borderCollapse: 'collapse', minWidth: '240px' }}>
            <tbody>
              <tr>
                <td
                  style={{ fontSize: '13px', color: '#64748b', padding: '4px 0', paddingRight: '24px' }}
                >
                  {data.packageTypeText}
                </td>
                <td
                  style={{
                    fontSize: '13px',
                    color: '#1e293b',
                    padding: '4px 0',
                    textAlign: 'right',
                  }}
                >
                  {money(data.packagePrice)}
                </td>
              </tr>
              <tr>
                <td
                  style={{
                    fontSize: '16px',
                    fontWeight: 700,
                    color: '#0f172a',
                    padding: '6px 0',
                    paddingRight: '24px',
                    borderTop: '1px solid #e2e8f0',
                  }}
                >
                  Toplam
                </td>
                <td
                  style={{
                    fontSize: '16px',
                    fontWeight: 700,
                    color: '#0f172a',
                    padding: '6px 0',
                    textAlign: 'right',
                    borderTop: '1px solid #e2e8f0',
                  }}
                >
                  {money(data.packagePrice)}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        {/* Footer */}
        <div
          style={{
            marginTop: '40px',
            borderTop: '1px solid #e2e8f0',
            paddingTop: '12px',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'flex-end',
          }}
        >
          <div>
            <p style={{ fontSize: '11px', color: '#94a3b8' }}>
              Ekspertiz No: {data.inspectionNo}
            </p>
            <p style={{ fontSize: '11px', color: '#94a3b8' }}>
              Tarih: {dateText(data.createdAt)}
            </p>
            {data.completedAt && (
              <p style={{ fontSize: '11px', color: '#94a3b8' }}>
                Tamamlanma: {dateText(data.completedAt)}
              </p>
            )}
          </div>
          <div style={{ textAlign: 'right' }}>
            <div
              style={{
                width: '160px',
                borderTop: '1px solid #334155',
                paddingTop: '4px',
                fontSize: '11px',
                color: '#64748b',
                textAlign: 'center',
              }}
            >
              Yetkili İmza
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
