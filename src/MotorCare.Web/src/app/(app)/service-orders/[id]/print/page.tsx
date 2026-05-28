'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { Printer } from 'lucide-react';
import apiClient from '@/core/api/client';
import { QRLinkCard } from '@/components/ui/qr-link-card';
import { money, dateText, dateTimeText } from '@/shared/utils/format';
import { publicServiceRecordUrl } from '@/shared/utils/public-links';

// ─── DTOs (inline — print page is standalone) ────────────────────────────────

interface ServiceOperationItemDto {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  lineTotal: number;
  notes: string | null;
}

interface ServicePartItemDto {
  id: string;
  partName: string;
  partNumber: string | null;
  unitPrice: number;
  quantity: number;
  discount: number;
  lineTotal: number;
  notes: string | null;
}

interface ServiceConsumableItemDto {
  id: string;
  category: string;
  brand: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

interface ServicePaymentDto {
  id: string;
  amount: number;
  method: string;
  paymentDate: string;
}

interface ServiceOrderDto {
  id: string;
  orderNo: string;
  publicSlug: string | null;
  customerName: string | null;
  vehiclePlate: string | null;
  vehicleDisplay: string | null;
  status: string;
  openedAt: string;
  closedAt: string | null;
  vehicleKm: number;
  complaint: string | null;
  workDescription: string | null;
  laborTotal: number;
  partsTotal: number;
  consumablesTotal: number;
  discountTotal: number;
  grandTotal: number;
  paidTotal: number;
  remainingTotal: number;
  operations: ServiceOperationItemDto[];
  parts: ServicePartItemDto[];
  consumables: ServiceConsumableItemDto[];
  payments: ServicePaymentDto[];
}

const STATUS_LABELS: Record<string, string> = {
  Open: 'Açık',
  InProgress: 'Devam Ediyor',
  WaitingForParts: 'Parça Bekleniyor',
  Completed: 'Tamamlandı',
  Cancelled: 'İptal',
};

const PAYMENT_METHOD_LABELS: Record<string, string> = {
  Cash: 'Nakit',
  CreditCard: 'Kredi Kartı',
  BankTransfer: 'Havale/EFT',
  Check: 'Çek',
};

export default function ServiceOrderPrintPage(): React.ReactElement {
  const params = useParams();
  const id = params.id as string;

  const [order, setOrder] = useState<ServiceOrderDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [loadError, setLoadError] = useState<string>('');

  useEffect(() => {
    async function load(): Promise<void> {
      try {
        const { data } = await apiClient.get<ServiceOrderDto>(`/api/service-orders/${id}`);
        setOrder(data);
      } catch {
        setLoadError('Servis kaydı yüklenemedi.');
      } finally {
        setLoading(false);
      }
    }
    void load();
  }, [id]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen text-slate-500 text-sm">
        Yükleniyor...
      </div>
    );
  }

  if (loadError || !order) {
    return (
      <div className="flex items-center justify-center min-h-screen text-red-600 text-sm">
        {loadError || 'Kayıt bulunamadı.'}
      </div>
    );
  }
  const publicUrl = order.publicSlug ? publicServiceRecordUrl(order.publicSlug) : null;

  return (
    <>
      {/* Print styles */}
      <style>{`
        @media print {
          .no-print { display: none !important; }
          body { background: white !important; }
          @page { margin: 1.5cm; }
        }
        body { font-family: sans-serif; }
      `}</style>

      {/* Print actions */}
      <div className="no-print mx-auto flex max-w-3xl justify-end p-4">
        <button
          onClick={() => window.print()}
          className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow hover:bg-blue-700 transition-colors"
        >
          <Printer size={16} />
          Yazdır
        </button>
      </div>

      {/* Print content */}
      <div className="max-w-3xl mx-auto p-8 bg-white text-slate-900">

        {/* Header */}
        <div className="flex items-start justify-between border-b-2 border-slate-900 pb-4 mb-6">
          <div>
            <h1 className="text-2xl font-bold tracking-tight">SERVİS EMRİ</h1>
            <p className="text-lg font-mono font-semibold mt-1">{order.orderNo}</p>
          </div>
          <div className="text-right text-sm text-slate-600">
            <p>
              <span className="font-semibold">Durum:</span>{' '}
              {STATUS_LABELS[order.status] ?? order.status}
            </p>
            <p>
              <span className="font-semibold">Açılış:</span> {dateTimeText(order.openedAt)}
            </p>
            {order.closedAt && (
              <p>
                <span className="font-semibold">Kapanış:</span> {dateText(order.closedAt)}
              </p>
            )}
          </div>
        </div>

        <div className="mb-6">
          <QRLinkCard
            href={publicUrl}
            title="Servis paylaşım QR kodu"
            description="Müşteri bu QR ile servis kaydının public sayfasına ulaşabilir."
            compact
          />
        </div>

        {/* Customer + Vehicle info */}
        <div className="grid grid-cols-2 gap-6 mb-6">
          <div>
            <h2 className="text-xs uppercase tracking-wider text-slate-400 mb-2">Müşteri</h2>
            <p className="font-semibold text-base">{order.customerName ?? '-'}</p>
          </div>
          <div>
            <h2 className="text-xs uppercase tracking-wider text-slate-400 mb-2">Araç</h2>
            <p className="font-semibold text-base">{order.vehiclePlate ?? '-'}</p>
            {order.vehicleDisplay && (
              <p className="text-sm text-slate-500">{order.vehicleDisplay}</p>
            )}
            <p className="text-sm text-slate-500">
              KM: {order.vehicleKm.toLocaleString('tr-TR')}
            </p>
          </div>
        </div>

        {/* Complaint */}
        {order.complaint && (
          <div className="mb-6 rounded border border-slate-200 bg-slate-50 px-4 py-3">
            <p className="text-xs uppercase tracking-wider text-slate-400 mb-1">Müşteri Şikayeti</p>
            <p className="text-sm text-slate-700 italic">&quot;{order.complaint}&quot;</p>
          </div>
        )}

        {/* Work description */}
        {order.workDescription && (
          <div className="mb-6 rounded border border-slate-200 bg-slate-50 px-4 py-3">
            <p className="text-xs uppercase tracking-wider text-slate-400 mb-1">Yapılan İşler</p>
            <p className="text-sm text-slate-700">{order.workDescription}</p>
          </div>
        )}

        {/* Operations table */}
        {order.operations.length > 0 && (
          <div className="mb-6">
            <h2 className="text-sm font-semibold text-slate-700 mb-2 border-b border-slate-200 pb-1">
              İşlemler
            </h2>
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-slate-50">
                  <th className="text-left py-2 px-2 border border-slate-200 font-medium">Açıklama</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-16">Miktar</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-28">Birim Fiyat</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-24">İndirim</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-28">Toplam</th>
                </tr>
              </thead>
              <tbody>
                {order.operations.map((op) => (
                  <tr key={op.id}>
                    <td className="py-1.5 px-2 border border-slate-200">
                      {op.description}
                      {op.notes && <span className="text-xs text-slate-400 ml-1">({op.notes})</span>}
                    </td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right">{op.quantity}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right">{money(op.unitPrice)}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right">{money(op.discount)}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right font-medium">{money(op.lineTotal)}</td>
                  </tr>
                ))}
                <tr className="bg-slate-50 font-semibold">
                  <td colSpan={4} className="py-1.5 px-2 border border-slate-200 text-right text-xs text-slate-500">
                    İşçilik Toplamı
                  </td>
                  <td className="py-1.5 px-2 border border-slate-200 text-right">{money(order.laborTotal)}</td>
                </tr>
              </tbody>
            </table>
          </div>
        )}

        {/* Parts table */}
        {order.parts.length > 0 && (
          <div className="mb-6">
            <h2 className="text-sm font-semibold text-slate-700 mb-2 border-b border-slate-200 pb-1">
              Parçalar
            </h2>
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-slate-50">
                  <th className="text-left py-2 px-2 border border-slate-200 font-medium">Parça</th>
                  <th className="text-left py-2 px-2 border border-slate-200 font-medium w-28">Parça No</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-16">Miktar</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-28">Birim Fiyat</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-24">İndirim</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-28">Toplam</th>
                </tr>
              </thead>
              <tbody>
                {order.parts.map((part) => (
                  <tr key={part.id}>
                    <td className="py-1.5 px-2 border border-slate-200">
                      {part.partName}
                      {part.notes && <span className="text-xs text-slate-400 ml-1">({part.notes})</span>}
                    </td>
                    <td className="py-1.5 px-2 border border-slate-200 text-slate-500">
                      {part.partNumber ?? '-'}
                    </td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right">{part.quantity}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right">{money(part.unitPrice)}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right">{money(part.discount)}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right font-medium">{money(part.lineTotal)}</td>
                  </tr>
                ))}
                <tr className="bg-slate-50 font-semibold">
                  <td colSpan={5} className="py-1.5 px-2 border border-slate-200 text-right text-xs text-slate-500">
                    Parça Toplamı
                  </td>
                  <td className="py-1.5 px-2 border border-slate-200 text-right">{money(order.partsTotal)}</td>
                </tr>
              </tbody>
            </table>
          </div>
        )}

        {/* Consumables table */}
        {order.consumables.length > 0 && (
          <div className="mb-6">
            <h2 className="text-sm font-semibold text-slate-700 mb-2 border-b border-slate-200 pb-1">
              Sarf Malzeme
            </h2>
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-slate-50">
                  <th className="text-left py-2 px-2 border border-slate-200 font-medium">Ürün</th>
                  <th className="text-left py-2 px-2 border border-slate-200 font-medium w-28">Marka</th>
                  <th className="text-left py-2 px-2 border border-slate-200 font-medium w-24">Kategori</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-16">Miktar</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-28">Birim Fiyat</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium w-28">Toplam</th>
                </tr>
              </thead>
              <tbody>
                {order.consumables.map((c) => (
                  <tr key={c.id}>
                    <td className="py-1.5 px-2 border border-slate-200">{c.productName}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-slate-500">{c.brand || '-'}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-slate-500">{c.category}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right">{c.quantity}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right">{money(c.unitPrice)}</td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right font-medium">{money(c.lineTotal)}</td>
                  </tr>
                ))}
                <tr className="bg-slate-50 font-semibold">
                  <td colSpan={5} className="py-1.5 px-2 border border-slate-200 text-right text-xs text-slate-500">
                    Sarf Malzeme Toplamı
                  </td>
                  <td className="py-1.5 px-2 border border-slate-200 text-right">
                    {money(order.consumablesTotal)}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        )}

        {/* Totals summary */}
        <div className="flex justify-end mb-6">
          <div className="w-64 border border-slate-200 rounded text-sm">
            <div className="flex justify-between px-3 py-1.5 border-b border-slate-100">
              <span className="text-slate-500">İşçilik</span>
              <span>{money(order.laborTotal)}</span>
            </div>
            <div className="flex justify-between px-3 py-1.5 border-b border-slate-100">
              <span className="text-slate-500">Parçalar</span>
              <span>{money(order.partsTotal)}</span>
            </div>
            <div className="flex justify-between px-3 py-1.5 border-b border-slate-100">
              <span className="text-slate-500">Sarf Malzeme</span>
              <span>{money(order.consumablesTotal)}</span>
            </div>
            {order.discountTotal > 0 && (
              <div className="flex justify-between px-3 py-1.5 border-b border-slate-100 text-orange-600">
                <span>İndirim</span>
                <span>-{money(order.discountTotal)}</span>
              </div>
            )}
            <div className="flex justify-between px-3 py-2 bg-slate-50 font-bold text-base">
              <span>Genel Toplam</span>
              <span>{money(order.grandTotal)}</span>
            </div>
          </div>
        </div>

        {/* Payments */}
        {order.payments.length > 0 && (
          <div className="mb-6">
            <h2 className="text-sm font-semibold text-slate-700 mb-2 border-b border-slate-200 pb-1">
              Ödemeler
            </h2>
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-slate-50">
                  <th className="text-left py-2 px-2 border border-slate-200 font-medium">Tarih</th>
                  <th className="text-left py-2 px-2 border border-slate-200 font-medium">Yöntem</th>
                  <th className="text-right py-2 px-2 border border-slate-200 font-medium">Tutar</th>
                </tr>
              </thead>
              <tbody>
                {order.payments.map((pmt) => (
                  <tr key={pmt.id}>
                    <td className="py-1.5 px-2 border border-slate-200">{dateText(pmt.paymentDate)}</td>
                    <td className="py-1.5 px-2 border border-slate-200">
                      {PAYMENT_METHOD_LABELS[pmt.method] ?? pmt.method}
                    </td>
                    <td className="py-1.5 px-2 border border-slate-200 text-right font-medium text-green-700">
                      {money(pmt.amount)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="flex justify-end mt-2">
              <div className="w-64 border border-slate-200 rounded text-sm">
                <div className="flex justify-between px-3 py-1.5 border-b border-slate-100">
                  <span className="text-slate-500">Ödenen</span>
                  <span className="text-green-600 font-medium">{money(order.paidTotal)}</span>
                </div>
                <div className="flex justify-between px-3 py-2 font-bold">
                  <span>Kalan Borç</span>
                  <span className={order.remainingTotal > 0 ? 'text-red-600' : 'text-green-600'}>
                    {money(order.remainingTotal)}
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Footer */}
        <div className="border-t border-slate-200 pt-4 mt-8 text-xs text-slate-400 text-center">
          <p>Bu belge {dateTimeText(new Date().toISOString())} tarihinde oluşturulmuştur.</p>
        </div>
      </div>
    </>
  );
}
