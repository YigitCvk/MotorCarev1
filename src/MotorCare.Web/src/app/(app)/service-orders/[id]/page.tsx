'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ArrowLeft,
  Printer,
  Plus,
  Trash2,
  ChevronDown,
} from 'lucide-react';
import Link from 'next/link';
import { toast } from 'sonner';
import apiClient from '@/core/api/client';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { QRLinkCard } from '@/components/ui/qr-link-card';
import { friendlyError } from '@/core/api/errors';
import { money, dateText, dateTimeText, todayInputValue } from '@/shared/utils/format';
import { publicServiceRecordUrl } from '@/shared/utils/public-links';
import type { PagedResult } from '@/shared/types/api.types';

// ─── DTOs ────────────────────────────────────────────────────────────────────

interface ServiceOperationItemDto {
  id: string;
  description: string;
  price: number;
  quantity: number;
  unitPrice: number;
  discount: number;
  lineTotal: number;
  notes: string | null;
  serviceCatalogItemId: string | null;
}

interface ServicePartItemDto {
  id: string;
  partName: string;
  partNumber: string | null;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  discount: number;
  lineTotal: number;
  notes: string | null;
  inventoryItemId: string | null;
}

interface ServiceConsumableItemDto {
  id: string;
  category: string;
  brand: string;
  productName: string;
  subCategory: string | null;
  specification: string | null;
  notes: string | null;
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

interface ServiceCatalogOptionDto {
  id: string;
  name: string;
  categoryText?: string | null;
  defaultDurationMinutes?: number;
  price: number;
  defaultPrice?: number;
  currency?: string;
  isActive: boolean;
}

interface InventoryOptionDto {
  id: string;
  name: string;
  sku: string | null;
  barcode: string | null;
  category: string | null;
  brand: string | null;
  unit: string;
  unitPrice: number;
  stockQuantity: number;
  isActive: boolean;
}

interface ServiceOrderDto {
  id: string;
  orderNo: string;
  publicSlug: string | null;
  vehicleId: string;
  customerId: string;
  customerName: string | null;
  vehiclePlate: string | null;
  vehicleDisplay: string | null;
  status: string;
  openedAt: string;
  updatedAt: string | null;
  closedAt: string | null;
  vehicleKm: number;
  complaint: string | null;
  workDescription: string | null;
  internalNote: string | null;
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

// ─── Helpers ─────────────────────────────────────────────────────────────────

const STATUS_OPTIONS: Array<{ value: string; label: string }> = [
  { value: 'Open', label: 'Açık' },
  { value: 'InProgress', label: 'Devam Ediyor' },
  { value: 'WaitingForParts', label: 'Parça Bekleniyor' },
  { value: 'Completed', label: 'Tamamlandı' },
  { value: 'Cancelled', label: 'İptal' },
];

const PAYMENT_METHODS: Array<{ value: string; label: string }> = [
  { value: 'Cash', label: 'Nakit' },
  { value: 'CreditCard', label: 'Kredi Kartı' },
  { value: 'BankTransfer', label: 'Havale/EFT' },
  { value: 'Check', label: 'Çek' },
];

function paymentMethodLabel(method: string): string {
  return PAYMENT_METHODS.find((m) => m.value === method)?.label ?? method;
}

function StatusBadge({ status }: { status: string }): React.ReactElement {
  switch (status) {
    case 'Open':
      return <span className="badge badge-blue">Açık</span>;
    case 'InProgress':
      return <span className="badge badge-yellow">Devam Ediyor</span>;
    case 'WaitingForParts':
      return (
        <span
          className="badge"
          style={{ backgroundColor: '#fff7ed', color: '#ea580c', borderColor: '#fed7aa' }}
        >
          Parça Bekleniyor
        </span>
      );
    case 'Completed':
      return <span className="badge badge-green">Tamamlandı</span>;
    case 'Cancelled':
      return <span className="badge badge-red">İptal</span>;
    default:
      return <span className="badge badge-gray">{status}</span>;
  }
}

type TabKey = 'operations' | 'parts' | 'consumables' | 'payments';

// ─── Add-form state shapes ────────────────────────────────────────────────────

interface OperationForm {
  serviceCatalogItemId: string;
  description: string;
  quantity: string;
  unitPrice: string;
  discount: string;
  notes: string;
}

interface PartForm {
  inventoryItemId: string;
  partName: string;
  partNumber: string;
  unitPrice: string;
  quantity: string;
  discount: string;
  notes: string;
}

interface ConsumableForm {
  category: string;
  productName: string;
  brand: string;
  unitPrice: string;
  quantity: string;
}

interface PaymentForm {
  amount: string;
  method: string;
  paymentDate: string;
}

const defaultOperationForm = (): OperationForm => ({
  serviceCatalogItemId: '',
  description: '',
  quantity: '1',
  unitPrice: '',
  discount: '0',
  notes: '',
});

const defaultPartForm = (): PartForm => ({
  inventoryItemId: '',
  partName: '',
  partNumber: '',
  unitPrice: '',
  quantity: '1',
  discount: '0',
  notes: '',
});

const defaultConsumableForm = (): ConsumableForm => ({
  category: '',
  productName: '',
  brand: '',
  unitPrice: '',
  quantity: '1',
});

const defaultPaymentForm = (): PaymentForm => ({
  amount: '',
  method: 'Cash',
  paymentDate: todayInputValue(),
});

// ─── Main component ───────────────────────────────────────────────────────────

export default function ServiceOrderDetailPage(): React.ReactElement {
  const params = useParams();
  const id = params.id as string;
  const router = useRouter();
  const qc = useQueryClient();

  const [activeTab, setActiveTab] = useState<TabKey>('operations');

  // Status update
  const [statusValue, setStatusValue] = useState<string>('');
  const [statusNote, setStatusNote] = useState<string>('');
  const [showStatusForm, setShowStatusForm] = useState<boolean>(false);

  // Discount
  const [showDiscountForm, setShowDiscountForm] = useState<boolean>(false);
  const [discountValue, setDiscountValue] = useState<string>('');

  // Add-item forms visibility
  const [showAddOperation, setShowAddOperation] = useState<boolean>(false);
  const [showAddPart, setShowAddPart] = useState<boolean>(false);
  const [showAddConsumable, setShowAddConsumable] = useState<boolean>(false);
  const [showAddPayment, setShowAddPayment] = useState<boolean>(false);

  // Form states
  const [opForm, setOpForm] = useState<OperationForm>(defaultOperationForm());
  const [partForm, setPartForm] = useState<PartForm>(defaultPartForm());
  const [consumableForm, setConsumableForm] = useState<ConsumableForm>(defaultConsumableForm());
  const [paymentForm, setPaymentForm] = useState<PaymentForm>(defaultPaymentForm());

  // Inline errors
  const [opError, setOpError] = useState<string>('');
  const [partError, setPartError] = useState<string>('');
  const [consumableError, setConsumableError] = useState<string>('');
  const [paymentError, setPaymentError] = useState<string>('');
  const [statusError, setStatusError] = useState<string>('');
  const [discountError, setDiscountError] = useState<string>('');

  // ── Query ──────────────────────────────────────────────────────────────────

  const { data, isLoading, error, refetch } = useQuery<ServiceOrderDto>({
    queryKey: ['service-order', id],
    queryFn: async () => {
      const { data } = await apiClient.get<ServiceOrderDto>(`/api/service-orders/${id}`);
      return data;
    },
  });

  const { data: serviceCatalogOptions } = useQuery<PagedResult<ServiceCatalogOptionDto>>({
    queryKey: ['service-catalog-options'],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<ServiceCatalogOptionDto>>('/api/services', {
        params: { pageSize: 100, isActive: true },
      });
      return data;
    },
    staleTime: 60_000,
  });

  const { data: inventoryOptions } = useQuery<PagedResult<InventoryOptionDto>>({
    queryKey: ['inventory-options'],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<InventoryOptionDto>>('/api/inventory', {
        params: { pageSize: 100, isActive: true },
      });
      return data;
    },
    staleTime: 60_000,
  });

  useEffect(() => {
    if (data) {
      setStatusValue(data.status);
      setDiscountValue(String(data.discountTotal ?? 0));
    }
  }, [data]);

  function invalidate(): void {
    void qc.invalidateQueries({ queryKey: ['service-order', id] });
    void qc.invalidateQueries({ queryKey: ['service-orders'] });
    void qc.invalidateQueries({ queryKey: ['inventory-options'] });
  }

  // ── Mutations ──────────────────────────────────────────────────────────────

  function handleCatalogSelection(serviceCatalogItemId: string): void {
    const selected = serviceCatalogOptions?.items.find((item) => item.id === serviceCatalogItemId);

    setOpForm((previous) => ({
      ...previous,
      serviceCatalogItemId,
      description: selected ? selected.name : previous.description,
      unitPrice: selected ? String(selected.price ?? selected.defaultPrice ?? 0) : previous.unitPrice,
    }));
  }

  function handleInventorySelection(inventoryItemId: string): void {
    const selected = inventoryOptions?.items.find((item) => item.id === inventoryItemId);

    setPartForm((previous) => ({
      ...previous,
      inventoryItemId,
      partName: selected ? selected.name : previous.partName,
      partNumber: selected ? selected.sku ?? selected.barcode ?? '' : previous.partNumber,
      unitPrice: selected ? String(selected.unitPrice ?? 0) : previous.unitPrice,
    }));
  }

  const updateStatusMutation = useMutation({
    mutationFn: async ({ status, note }: { status: string; note: string | null }) => {
      await apiClient.put(`/api/service-orders/${id}/status`, { status, note });
    },
    onSuccess: () => {
      invalidate();
      setShowStatusForm(false);
      setStatusNote('');
      setStatusError('');
      toast.success('Durum güncellendi');
    },
    onError: (err) => {
      const msg = friendlyError(err, 'Durum güncellenemedi.');
      setStatusError(msg);
      toast.error(msg);
    },
  });

  const setDiscountMutation = useMutation({
    mutationFn: async (discount: number) => {
      await apiClient.patch(`/api/service-orders/${id}/discount`, { discount });
    },
    onSuccess: () => {
      invalidate();
      setShowDiscountForm(false);
      setDiscountError('');
    },
    onError: (err) => setDiscountError(friendlyError(err, 'İndirim kaydedilemedi.')),
  });

  const addOperationMutation = useMutation({
    mutationFn: async (body: {
      description: string;
      quantity: number;
      unitPrice: number;
      discount: number;
      notes: string | null;
      serviceCatalogItemId: string | null;
    }) => {
      await apiClient.post(`/api/service-orders/${id}/operations`, body);
    },
    onSuccess: () => {
      invalidate();
      setOpForm(defaultOperationForm());
      setShowAddOperation(false);
      setOpError('');
      toast.success('İşlem eklendi');
    },
    onError: (err) => {
      const msg = friendlyError(err, 'İşlem eklenemedi.');
      setOpError(msg);
      toast.error(msg);
    },
  });

  const removeOperationMutation = useMutation({
    mutationFn: async (operationId: string) => {
      await apiClient.delete(`/api/service-orders/${id}/operations/${operationId}`);
    },
    onSuccess: () => invalidate(),
  });

  const addPartMutation = useMutation({
    mutationFn: async (body: {
      partName: string;
      partNumber: string | null;
      unitPrice: number;
      quantity: number;
      inventoryItemId: string | null;
      discount: number;
      notes: string | null;
    }) => {
      await apiClient.post(`/api/service-orders/${id}/parts`, body);
    },
    onSuccess: () => {
      invalidate();
      setPartForm(defaultPartForm());
      setShowAddPart(false);
      setPartError('');
      toast.success('Parça eklendi');
    },
    onError: (err) => {
      const msg = friendlyError(err, 'Parça eklenemedi.');
      setPartError(msg);
      toast.error(msg);
    },
  });

  const removePartMutation = useMutation({
    mutationFn: async (partId: string) => {
      await apiClient.delete(`/api/service-orders/${id}/parts/${partId}`);
    },
    onSuccess: () => invalidate(),
  });

  const addConsumableMutation = useMutation({
    mutationFn: async (body: {
      category: string;
      productName: string;
      unitPrice: number;
      quantity: number;
      brand: string | null;
      subCategory: string | null;
      specification: string | null;
      notes: string | null;
    }) => {
      await apiClient.post(`/api/service-orders/${id}/consumables`, body);
    },
    onSuccess: () => {
      invalidate();
      setConsumableForm(defaultConsumableForm());
      setShowAddConsumable(false);
      setConsumableError('');
      toast.success('Sarf malzeme eklendi');
    },
    onError: (err) => {
      const msg = friendlyError(err, 'Sarf malzeme eklenemedi.');
      setConsumableError(msg);
      toast.error(msg);
    },
  });

  const removeConsumableMutation = useMutation({
    mutationFn: async (consumableId: string) => {
      await apiClient.delete(`/api/service-orders/${id}/consumables/${consumableId}`);
    },
    onSuccess: () => invalidate(),
  });

  const addPaymentMutation = useMutation({
    mutationFn: async (body: { amount: number; method: string; paymentDate: string }) => {
      await apiClient.post(`/api/service-orders/${id}/payments`, body);
    },
    onSuccess: () => {
      invalidate();
      setPaymentForm(defaultPaymentForm());
      setShowAddPayment(false);
      setPaymentError('');
      toast.success('Ödeme eklendi');
    },
    onError: (err) => {
      const msg = friendlyError(err, 'Ödeme eklenemedi.');
      setPaymentError(msg);
      toast.error(msg);
    },
  });

  // ── Submit handlers ────────────────────────────────────────────────────────

  function handleAddOperation(e: React.FormEvent): void {
    e.preventDefault();
    setOpError('');
    if (!opForm.description.trim()) { setOpError('Açıklama zorunludur.'); return; }
    const qty = Number(opForm.quantity);
    const price = Number(opForm.unitPrice);
    const discount = Number(opForm.discount) || 0;
    if (isNaN(qty) || qty <= 0) { setOpError('Geçerli bir miktar girin.'); return; }
    if (isNaN(price) || price < 0) { setOpError('Geçerli bir birim fiyat girin.'); return; }
    if (discount < 0) { setOpError('İndirim negatif olamaz.'); return; }
    if (discount > qty * price) { setOpError('İndirim satır toplamından büyük olamaz.'); return; }
    addOperationMutation.mutate({
      description: opForm.description.trim(),
      quantity: qty,
      unitPrice: price,
      discount,
      notes: opForm.notes.trim() || null,
      serviceCatalogItemId: opForm.serviceCatalogItemId || null,
    });
  }

  function handleAddPart(e: React.FormEvent): void {
    e.preventDefault();
    setPartError('');
    if (!partForm.partName.trim()) { setPartError('Parça adı zorunludur.'); return; }
    const qty = Number(partForm.quantity);
    const price = Number(partForm.unitPrice);
    const discount = Number(partForm.discount) || 0;
    if (isNaN(qty) || qty <= 0 || !Number.isInteger(qty)) { setPartError('Geçerli bir tam sayı miktar girin.'); return; }
    if (isNaN(price) || price <= 0) { setPartError('Geçerli bir birim fiyat girin.'); return; }
    if (discount < 0) { setPartError('İndirim negatif olamaz.'); return; }
    if (discount > qty * price) { setPartError('İndirim satır toplamından büyük olamaz.'); return; }
    addPartMutation.mutate({
      partName: partForm.partName.trim(),
      partNumber: partForm.partNumber.trim() || null,
      unitPrice: price,
      quantity: qty,
      inventoryItemId: partForm.inventoryItemId || null,
      discount,
      notes: partForm.notes.trim() || null,
    });
  }

  function handleAddConsumable(e: React.FormEvent): void {
    e.preventDefault();
    setConsumableError('');
    if (!consumableForm.category.trim()) { setConsumableError('Kategori zorunludur.'); return; }
    if (!consumableForm.productName.trim()) { setConsumableError('Ürün adı zorunludur.'); return; }
    const qty = Number(consumableForm.quantity);
    const price = Number(consumableForm.unitPrice);
    if (isNaN(qty) || qty <= 0 || !Number.isInteger(qty)) { setConsumableError('Geçerli bir tam sayı miktar girin.'); return; }
    if (isNaN(price) || price < 0) { setConsumableError('Geçerli bir birim fiyat girin.'); return; }
    addConsumableMutation.mutate({
      category: consumableForm.category.trim(),
      productName: consumableForm.productName.trim(),
      unitPrice: price,
      quantity: qty,
      brand: consumableForm.brand.trim() || null,
      subCategory: null,
      specification: null,
      notes: null,
    });
  }

  function handleAddPayment(e: React.FormEvent): void {
    e.preventDefault();
    setPaymentError('');
    const amount = Number(paymentForm.amount);
    if (isNaN(amount) || amount <= 0) { setPaymentError('Geçerli bir tutar girin.'); return; }
    if (!paymentForm.paymentDate) { setPaymentError('Ödeme tarihi zorunludur.'); return; }
    addPaymentMutation.mutate({
      amount,
      method: paymentForm.method,
      paymentDate: paymentForm.paymentDate,
    });
  }

  // ── Render ─────────────────────────────────────────────────────────────────

  if (isLoading) return <PageLoading />;
  if (error || !data) {
    return <ErrorState message="Servis kaydı bulunamadı." onRetry={() => void refetch()} />;
  }

  const TABS: Array<{ key: TabKey; label: string; count: number }> = [
    { key: 'operations', label: 'İşlemler', count: data.operations.length },
    { key: 'parts', label: 'Parçalar', count: data.parts.length },
    { key: 'consumables', label: 'Sarf Malzeme', count: data.consumables.length },
    { key: 'payments', label: 'Ödemeler', count: data.payments.length },
  ];
  const publicUrl = data.publicSlug ? publicServiceRecordUrl(data.publicSlug) : null;

  return (
    <div>
      {/* Back nav */}
      <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <button onClick={() => router.push('/service-orders')} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Servis Kayıtları
        </button>
        <div className="flex flex-wrap items-center gap-2">
          <Link href={`/service-orders/${id}/print`} target="_blank" className="btn-secondary text-sm">
            <Printer size={14} />
            Yazdır
          </Link>
        </div>
      </div>

      <div className="mb-4">
        <QRLinkCard
          href={publicUrl}
          title="Servis paylaşım QR kodu"
          description="Müşteri servis kaydını bu QR veya bağlantı ile görüntüleyebilir."
        />
      </div>

      {/* Header */}
      <div className="card p-5 mb-4">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <div className="flex items-center gap-3 mb-2">
              <h1 className="text-xl font-bold text-slate-900 font-mono">{data.orderNo}</h1>
              <StatusBadge status={data.status} />
            </div>
            <div className="flex flex-wrap gap-x-6 gap-y-1 text-sm text-slate-600">
              <span>
                <span className="text-slate-400">Müşteri:</span>{' '}
                <span className="font-medium text-slate-800">{data.customerName ?? '-'}</span>
              </span>
              <span>
                <span className="text-slate-400">Araç:</span>{' '}
                <span className="font-medium text-slate-800">
                  {data.vehiclePlate ?? '-'}
                  {data.vehicleDisplay ? ` · ${data.vehicleDisplay}` : ''}
                </span>
              </span>
              <span>
                <span className="text-slate-400">KM:</span>{' '}
                <span className="font-medium text-slate-800">
                  {data.vehicleKm.toLocaleString('tr-TR')}
                </span>
              </span>
              <span>
                <span className="text-slate-400">Açılış:</span>{' '}
                <span className="font-medium text-slate-800">{dateTimeText(data.openedAt)}</span>
              </span>
              {data.closedAt && (
                <span>
                  <span className="text-slate-400">Kapanış:</span>{' '}
                  <span className="font-medium text-slate-800">{dateText(data.closedAt)}</span>
                </span>
              )}
            </div>
            {data.complaint && (
              <p className="mt-2 text-sm text-slate-500 italic">&quot;{data.complaint}&quot;</p>
            )}
          </div>

          {/* Status update trigger */}
          <button
            onClick={() => setShowStatusForm((v) => !v)}
            className="btn-secondary text-sm shrink-0"
          >
            Durum Güncelle
            <ChevronDown size={14} />
          </button>
        </div>

        {/* Status update form (inline dropdown) */}
        {showStatusForm && (
          <div className="mt-4 border-t border-slate-100 pt-4">
            {statusError && (
              <p className="error-text mb-2">{statusError}</p>
            )}
            <div className="flex flex-wrap gap-3 items-end">
              <div className="form-group mb-0">
                <label className="label">Yeni Durum</label>
                <select
                  className="input w-48"
                  value={statusValue}
                  onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setStatusValue(e.target.value)}
                >
                  {STATUS_OPTIONS.map((opt) => (
                    <option key={opt.value} value={opt.value}>{opt.label}</option>
                  ))}
                </select>
              </div>
              <div className="form-group mb-0 flex-1 min-w-[200px]">
                <label className="label">Not (isteğe bağlı)</label>
                <input
                  className="input"
                  placeholder="Durum notu..."
                  value={statusNote}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) => setStatusNote(e.target.value)}
                />
              </div>
              <button
                onClick={() =>
                  updateStatusMutation.mutate({ status: statusValue, note: statusNote.trim() || null })
                }
                disabled={updateStatusMutation.isPending}
                className="btn-primary"
              >
                {updateStatusMutation.isPending ? 'Kaydediliyor...' : 'Güncelle'}
              </button>
              <button onClick={() => setShowStatusForm(false)} className="btn-ghost">
                İptal
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-7 gap-3 mb-4">
        {[
          { label: 'İşçilik', value: data.laborTotal },
          { label: 'Parçalar', value: data.partsTotal },
          { label: 'Sarf Malzeme', value: data.consumablesTotal },
          { label: 'İndirim', value: -data.discountTotal, negative: true },
          { label: 'Toplam', value: data.grandTotal, highlight: true },
          { label: 'Ödenen', value: data.paidTotal, green: true },
          { label: 'Kalan', value: data.remainingTotal, red: data.remainingTotal > 0 },
        ].map((item) => (
          <div
            key={item.label}
            className={[
              'card p-3 text-center',
              item.highlight ? 'border-blue-200 bg-blue-50' : '',
            ].join(' ')}
          >
            <p className="text-xs text-slate-400 mb-1">{item.label}</p>
            <p
              className={[
                'text-sm font-bold',
                item.highlight ? 'text-blue-700' : '',
                item.green ? 'text-green-600' : '',
                item.red ? 'text-red-600' : '',
                item.negative ? 'text-orange-600' : '',
                !item.highlight && !item.green && !item.red && !item.negative ? 'text-slate-900' : '',
              ].join(' ')}
            >
              {item.negative ? `-${money(data.discountTotal)}` : money(item.value as number)}
            </p>
          </div>
        ))}
      </div>

      {/* Discount control */}
      <div className="flex items-center gap-3 mb-4">
        <button onClick={() => setShowDiscountForm((v) => !v)} className="btn-secondary text-sm">
          İndirim Belirle
        </button>
        {showDiscountForm && (
          <div className="flex items-center gap-2">
            <input
              type="number"
              className="input w-36"
              placeholder="İndirim tutarı"
              min={0}
              value={discountValue}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) => setDiscountValue(e.target.value)}
            />
            <button
              onClick={() => {
                const v = Number(discountValue);
                if (isNaN(v) || v < 0) { setDiscountError('Geçerli bir değer girin.'); return; }
                setDiscountMutation.mutate(v);
              }}
              disabled={setDiscountMutation.isPending}
              className="btn-primary text-sm"
            >
              {setDiscountMutation.isPending ? '...' : 'Uygula'}
            </button>
            <button onClick={() => setShowDiscountForm(false)} className="btn-ghost text-sm">İptal</button>
            {discountError && <p className="error-text">{discountError}</p>}
          </div>
        )}
      </div>

      {/* Tabs */}
      <div className="flex gap-1 border-b border-slate-200 mb-4">
        {TABS.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={[
              'px-4 py-2.5 text-sm font-medium transition-colors border-b-2 -mb-px',
              activeTab === tab.key
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-slate-500 hover:text-slate-700',
            ].join(' ')}
          >
            {tab.label}
            {tab.count > 0 && (
              <span className="ml-1.5 rounded-full bg-slate-100 px-1.5 py-0.5 text-xs text-slate-600">
                {tab.count}
              </span>
            )}
          </button>
        ))}
      </div>

      {/* ── Tab: Operations ── */}
      {activeTab === 'operations' && (
        <div>
          <div className="flex justify-between items-center mb-3">
            <h2 className="text-sm font-semibold text-slate-700">İşlemler</h2>
            <button
              onClick={() => setShowAddOperation((v) => !v)}
              className="btn-secondary text-sm"
            >
              <Plus size={14} />
              Ekle
            </button>
          </div>

          {showAddOperation && (
            <div className="card p-4 mb-4">
              {opError && <p className="error-text mb-2">{opError}</p>}
              <form onSubmit={handleAddOperation} className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                <div className="form-group col-span-2 sm:col-span-4">
                  <label className="label">Katalog Hizmeti</label>
                  <select
                    className="input"
                    value={opForm.serviceCatalogItemId}
                    onChange={(e) => handleCatalogSelection(e.target.value)}
                  >
                    <option value="">Manuel işlem</option>
                    {serviceCatalogOptions?.items.map((item) => (
                      <option key={item.id} value={item.id}>
                        {item.name} - {money(item.price ?? item.defaultPrice ?? 0)}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group col-span-2 sm:col-span-4">
                  <label className="label">Açıklama *</label>
                  <input
                    className="input"
                    value={opForm.description}
                    onChange={(e) => setOpForm((p) => ({ ...p, description: e.target.value }))}
                    placeholder="İşlem açıklaması..."
                  />
                </div>
                <div className="form-group">
                  <label className="label">Miktar</label>
                  <input
                    type="number"
                    className="input"
                    min={1}
                    value={opForm.quantity}
                    onChange={(e) => setOpForm((p) => ({ ...p, quantity: e.target.value }))}
                  />
                </div>
                <div className="form-group">
                  <label className="label">Birim Fiyat (₺)</label>
                  <input
                    type="number"
                    className="input"
                    min={0}
                    step="0.01"
                    value={opForm.unitPrice}
                    onChange={(e) => setOpForm((p) => ({ ...p, unitPrice: e.target.value }))}
                  />
                </div>
                <div className="form-group">
                  <label className="label">İndirim (₺)</label>
                  <input
                    type="number"
                    className="input"
                    min={0}
                    step="0.01"
                    value={opForm.discount}
                    onChange={(e) => setOpForm((p) => ({ ...p, discount: e.target.value }))}
                  />
                </div>
                <div className="form-group">
                  <label className="label">Not</label>
                  <input
                    className="input"
                    value={opForm.notes}
                    onChange={(e) => setOpForm((p) => ({ ...p, notes: e.target.value }))}
                    placeholder="İsteğe bağlı..."
                  />
                </div>
                <div className="col-span-2 sm:col-span-4 flex gap-2">
                  <button type="submit" disabled={addOperationMutation.isPending} className="btn-primary text-sm">
                    {addOperationMutation.isPending ? 'Ekleniyor...' : 'Ekle'}
                  </button>
                  <button type="button" onClick={() => setShowAddOperation(false)} className="btn-ghost text-sm">
                    İptal
                  </button>
                </div>
              </form>
            </div>
          )}

          {data.operations.length === 0 ? (
            <div className="card p-6 text-center text-sm text-slate-400">Henüz işlem eklenmedi.</div>
          ) : (
            <div className="card p-0 overflow-hidden">
              <table className="table">
                <thead>
                  <tr>
                    <th>Açıklama</th>
                    <th className="text-right">Miktar</th>
                    <th className="text-right">Birim Fiyat</th>
                    <th className="text-right">İndirim</th>
                    <th className="text-right">Toplam</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {data.operations.map((op) => (
                    <tr key={op.id}>
                      <td>
                        <span className="font-medium text-slate-900">{op.description}</span>
                        {op.notes && <p className="text-xs text-slate-400">{op.notes}</p>}
                      </td>
                      <td className="text-right text-slate-600">{op.quantity}</td>
                      <td className="text-right text-slate-600">{money(op.unitPrice)}</td>
                      <td className="text-right text-slate-500">{money(op.discount)}</td>
                      <td className="text-right font-semibold text-slate-900">{money(op.lineTotal)}</td>
                      <td>
                        <button
                          onClick={() => removeOperationMutation.mutate(op.id)}
                          disabled={removeOperationMutation.isPending}
                          className="btn-ghost text-red-500 hover:text-red-700 p-1"
                          title="Sil"
                        >
                          <Trash2 size={14} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* ── Tab: Parts ── */}
      {activeTab === 'parts' && (
        <div>
          <div className="flex justify-between items-center mb-3">
            <h2 className="text-sm font-semibold text-slate-700">Parçalar</h2>
            <button
              onClick={() => setShowAddPart((v) => !v)}
              className="btn-secondary text-sm"
            >
              <Plus size={14} />
              Ekle
            </button>
          </div>

          {showAddPart && (
            <div className="card p-4 mb-4">
              {partError && <p className="error-text mb-2">{partError}</p>}
              <form onSubmit={handleAddPart} className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                <div className="form-group col-span-2 sm:col-span-3">
                  <label className="label">Stok Kalemi</label>
                  <select
                    className="input"
                    value={partForm.inventoryItemId}
                    onChange={(e) => handleInventorySelection(e.target.value)}
                  >
                    <option value="">Manuel parça</option>
                    {inventoryOptions?.items.map((item) => (
                      <option key={item.id} value={item.id}>
                        {item.name} - {money(item.unitPrice)} - Stok: {item.stockQuantity} {item.unit}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group col-span-2 sm:col-span-3">
                  <label className="label">Parça Adı *</label>
                  <input
                    className="input"
                    value={partForm.partName}
                    onChange={(e) => setPartForm((p) => ({ ...p, partName: e.target.value }))}
                    placeholder="ör. Fren diski"
                  />
                </div>
                <div className="form-group">
                  <label className="label">Parça No</label>
                  <input
                    className="input"
                    value={partForm.partNumber}
                    onChange={(e) => setPartForm((p) => ({ ...p, partNumber: e.target.value }))}
                    placeholder="İsteğe bağlı"
                  />
                </div>
                <div className="form-group">
                  <label className="label">Miktar</label>
                  <input
                    type="number"
                    className="input"
                    min={1}
                    step={1}
                    value={partForm.quantity}
                    onChange={(e) => setPartForm((p) => ({ ...p, quantity: e.target.value }))}
                  />
                </div>
                <div className="form-group">
                  <label className="label">Birim Fiyat (₺)</label>
                  <input
                    type="number"
                    className="input"
                    min={0}
                    step="0.01"
                    value={partForm.unitPrice}
                    onChange={(e) => setPartForm((p) => ({ ...p, unitPrice: e.target.value }))}
                  />
                </div>
                <div className="form-group">
                  <label className="label">İndirim (₺)</label>
                  <input
                    type="number"
                    className="input"
                    min={0}
                    step="0.01"
                    value={partForm.discount}
                    onChange={(e) => setPartForm((p) => ({ ...p, discount: e.target.value }))}
                  />
                </div>
                <div className="form-group col-span-2">
                  <label className="label">Not</label>
                  <input
                    className="input"
                    value={partForm.notes}
                    onChange={(e) => setPartForm((p) => ({ ...p, notes: e.target.value }))}
                    placeholder="İsteğe bağlı..."
                  />
                </div>
                <div className="col-span-2 sm:col-span-3 flex gap-2">
                  <button type="submit" disabled={addPartMutation.isPending} className="btn-primary text-sm">
                    {addPartMutation.isPending ? 'Ekleniyor...' : 'Ekle'}
                  </button>
                  <button type="button" onClick={() => setShowAddPart(false)} className="btn-ghost text-sm">
                    İptal
                  </button>
                </div>
              </form>
            </div>
          )}

          {data.parts.length === 0 ? (
            <div className="card p-6 text-center text-sm text-slate-400">Henüz parça eklenmedi.</div>
          ) : (
            <div className="card p-0 overflow-hidden">
              <table className="table">
                <thead>
                  <tr>
                    <th>Parça</th>
                    <th>Parça No</th>
                    <th className="text-right">Miktar</th>
                    <th className="text-right">Birim Fiyat</th>
                    <th className="text-right">İndirim</th>
                    <th className="text-right">Toplam</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {data.parts.map((part) => (
                    <tr key={part.id}>
                      <td>
                        <span className="font-medium text-slate-900">{part.partName}</span>
                        {part.notes && <p className="text-xs text-slate-400">{part.notes}</p>}
                      </td>
                      <td className="text-slate-500 text-sm">{part.partNumber ?? '-'}</td>
                      <td className="text-right text-slate-600">{part.quantity}</td>
                      <td className="text-right text-slate-600">{money(part.unitPrice)}</td>
                      <td className="text-right text-slate-500">{money(part.discount)}</td>
                      <td className="text-right font-semibold text-slate-900">{money(part.lineTotal)}</td>
                      <td>
                        <button
                          onClick={() => removePartMutation.mutate(part.id)}
                          disabled={removePartMutation.isPending}
                          className="btn-ghost text-red-500 hover:text-red-700 p-1"
                          title="Sil"
                        >
                          <Trash2 size={14} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* ── Tab: Consumables ── */}
      {activeTab === 'consumables' && (
        <div>
          <div className="flex justify-between items-center mb-3">
            <h2 className="text-sm font-semibold text-slate-700">Sarf Malzeme</h2>
            <button
              onClick={() => setShowAddConsumable((v) => !v)}
              className="btn-secondary text-sm"
            >
              <Plus size={14} />
              Ekle
            </button>
          </div>

          {showAddConsumable && (
            <div className="card p-4 mb-4">
              {consumableError && <p className="error-text mb-2">{consumableError}</p>}
              <form onSubmit={handleAddConsumable} className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                <div className="form-group">
                  <label className="label">Kategori *</label>
                  <input
                    className="input"
                    value={consumableForm.category}
                    onChange={(e) => setConsumableForm((p) => ({ ...p, category: e.target.value }))}
                    placeholder="ör. Yağ"
                  />
                </div>
                <div className="form-group">
                  <label className="label">Ürün Adı *</label>
                  <input
                    className="input"
                    value={consumableForm.productName}
                    onChange={(e) => setConsumableForm((p) => ({ ...p, productName: e.target.value }))}
                    placeholder="ör. Motor Yağı 5W-40"
                  />
                </div>
                <div className="form-group">
                  <label className="label">Marka</label>
                  <input
                    className="input"
                    value={consumableForm.brand}
                    onChange={(e) => setConsumableForm((p) => ({ ...p, brand: e.target.value }))}
                    placeholder="ör. Castrol"
                  />
                </div>
                <div className="form-group">
                  <label className="label">Miktar</label>
                  <input
                    type="number"
                    className="input"
                    min={1}
                    step={1}
                    value={consumableForm.quantity}
                    onChange={(e) => setConsumableForm((p) => ({ ...p, quantity: e.target.value }))}
                  />
                </div>
                <div className="form-group">
                  <label className="label">Birim Fiyat (₺)</label>
                  <input
                    type="number"
                    className="input"
                    min={0}
                    step="0.01"
                    value={consumableForm.unitPrice}
                    onChange={(e) => setConsumableForm((p) => ({ ...p, unitPrice: e.target.value }))}
                  />
                </div>
                <div className="col-span-2 sm:col-span-3 flex gap-2">
                  <button type="submit" disabled={addConsumableMutation.isPending} className="btn-primary text-sm">
                    {addConsumableMutation.isPending ? 'Ekleniyor...' : 'Ekle'}
                  </button>
                  <button type="button" onClick={() => setShowAddConsumable(false)} className="btn-ghost text-sm">
                    İptal
                  </button>
                </div>
              </form>
            </div>
          )}

          {data.consumables.length === 0 ? (
            <div className="card p-6 text-center text-sm text-slate-400">Henüz sarf malzeme eklenmedi.</div>
          ) : (
            <div className="card p-0 overflow-hidden">
              <table className="table">
                <thead>
                  <tr>
                    <th>Ürün</th>
                    <th>Marka</th>
                    <th className="text-right">Miktar</th>
                    <th className="text-right">Birim Fiyat</th>
                    <th className="text-right">Toplam</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {data.consumables.map((c) => (
                    <tr key={c.id}>
                      <td>
                        <span className="font-medium text-slate-900">{c.productName}</span>
                        <p className="text-xs text-slate-400">{c.category}</p>
                      </td>
                      <td className="text-slate-500 text-sm">{c.brand || '-'}</td>
                      <td className="text-right text-slate-600">{c.quantity}</td>
                      <td className="text-right text-slate-600">{money(c.unitPrice)}</td>
                      <td className="text-right font-semibold text-slate-900">{money(c.lineTotal)}</td>
                      <td>
                        <button
                          onClick={() => removeConsumableMutation.mutate(c.id)}
                          disabled={removeConsumableMutation.isPending}
                          className="btn-ghost text-red-500 hover:text-red-700 p-1"
                          title="Sil"
                        >
                          <Trash2 size={14} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* ── Tab: Payments ── */}
      {activeTab === 'payments' && (
        <div>
          <div className="flex justify-between items-center mb-3">
            <h2 className="text-sm font-semibold text-slate-700">Ödemeler</h2>
            <button
              onClick={() => setShowAddPayment((v) => !v)}
              className="btn-secondary text-sm"
            >
              <Plus size={14} />
              Ekle
            </button>
          </div>

          {showAddPayment && (
            <div className="card p-4 mb-4">
              {paymentError && <p className="error-text mb-2">{paymentError}</p>}
              <form onSubmit={handleAddPayment} className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                <div className="form-group">
                  <label className="label">Tutar (₺) *</label>
                  <input
                    type="number"
                    className="input"
                    min={0.01}
                    step="0.01"
                    value={paymentForm.amount}
                    onChange={(e) => setPaymentForm((p) => ({ ...p, amount: e.target.value }))}
                    placeholder="0,00"
                  />
                </div>
                <div className="form-group">
                  <label className="label">Yöntem</label>
                  <select
                    className="input"
                    value={paymentForm.method}
                    onChange={(e) => setPaymentForm((p) => ({ ...p, method: e.target.value }))}
                  >
                    {PAYMENT_METHODS.map((m) => (
                      <option key={m.value} value={m.value}>{m.label}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="label">Tarih *</label>
                  <input
                    type="date"
                    className="input"
                    value={paymentForm.paymentDate}
                    onChange={(e) => setPaymentForm((p) => ({ ...p, paymentDate: e.target.value }))}
                  />
                </div>
                <div className="col-span-2 sm:col-span-3 flex gap-2">
                  <button type="submit" disabled={addPaymentMutation.isPending} className="btn-primary text-sm">
                    {addPaymentMutation.isPending ? 'Ekleniyor...' : 'Ödeme Ekle'}
                  </button>
                  <button type="button" onClick={() => setShowAddPayment(false)} className="btn-ghost text-sm">
                    İptal
                  </button>
                </div>
              </form>
            </div>
          )}

          {data.payments.length === 0 ? (
            <div className="card p-6 text-center text-sm text-slate-400">Henüz ödeme kaydı yok.</div>
          ) : (
            <div className="card p-0 overflow-hidden">
              <table className="table">
                <thead>
                  <tr>
                    <th>Tarih</th>
                    <th>Yöntem</th>
                    <th className="text-right">Tutar</th>
                  </tr>
                </thead>
                <tbody>
                  {data.payments.map((pmt) => (
                    <tr key={pmt.id}>
                      <td className="text-slate-600">{dateText(pmt.paymentDate)}</td>
                      <td className="text-slate-700">{paymentMethodLabel(pmt.method)}</td>
                      <td className="text-right font-semibold text-green-700">{money(pmt.amount)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Payment summary */}
          {data.payments.length > 0 && (
            <div className="mt-3 flex flex-col items-end gap-1 text-sm">
              <div className="flex gap-6">
                <span className="text-slate-500">Toplam Borç:</span>
                <span className="font-semibold text-slate-900 w-24 text-right">{money(data.grandTotal)}</span>
              </div>
              <div className="flex gap-6">
                <span className="text-slate-500">Ödenen:</span>
                <span className="font-semibold text-green-600 w-24 text-right">{money(data.paidTotal)}</span>
              </div>
              <div className="flex gap-6 border-t border-slate-200 pt-1">
                <span className="text-slate-500">Kalan:</span>
                <span
                  className={`font-bold w-24 text-right ${data.remainingTotal > 0 ? 'text-red-600' : 'text-green-600'}`}
                >
                  {money(data.remainingTotal)}
                </span>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
