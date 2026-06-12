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
  Clock,
  Activity,
  Paperclip,
  Share2,
  Globe,
  GlobeLock,
  Upload,
  FileText,
  Wrench,
  Package,
  CreditCard as CreditCardIcon,
  ExternalLink,
} from 'lucide-react';
import Link from 'next/link';
import { toast } from 'sonner';
import apiClient from '@/core/api/client';
import { useAuth } from '@/core/auth/auth.context';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { QRLinkCard } from '@/components/ui/qr-link-card';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { friendlyError } from '@/core/api/errors';
import { canAddPayment, canEditServiceOrder } from '@/shared/constants/permissions';
import { downloadFile as downloadAuthenticatedFile } from '@/shared/utils/download';
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

interface StatusHistoryItem {
  id: string;
  fromStatus: string | null;
  fromStatusText: string | null;
  toStatus: string;
  toStatusText: string;
  note: string | null;
  changedByUserName: string | null;
  createdAt: string;
}

interface ActivityFeedItem {
  id: string;
  activityType: string;
  activityTypeText: string;
  title: string;
  description: string;
  actorName: string;
  createdAt: string;
  amount: number | null;
  currency: string;
  iconName: string;
  severity: string;
}

interface AttachmentDto {
  id: string;
  originalFileName: string;
  fileUrl: string;
  contentType: string;
  fileSize: number;
  attachmentType: string;
  attachmentTypeText: string;
  description: string | null;
  uploadedByUserName: string | null;
  createdAt: string;
  isImage: boolean;
  isPdf: boolean;
}

interface PublicAccessDto {
  slug: string;
  isActive: boolean;
  createdAtUtc: string;
  lastAccessedAtUtc: string | null;
  accessCount: number;
}

interface AttachmentForm {
  attachmentType: string;
  description: string;
  file: File | null;
}

type DeleteTarget =
  | { type: 'operation'; id: string; label: string }
  | { type: 'part'; id: string; label: string }
  | { type: 'consumable'; id: string; label: string }
  | { type: 'attachment'; id: string; label: string };

const ATTACHMENT_TYPES = [
  { value: 'BeforeServicePhoto', label: 'İşlem Öncesi Fotoğraf' },
  { value: 'AfterServicePhoto', label: 'İşlem Sonrası Fotoğraf' },
  { value: 'DamagePhoto', label: 'Hasar Fotoğrafı' },
  { value: 'PartInvoice', label: 'Parça Faturası' },
  { value: 'ServiceDocument', label: 'Servis Evrakı' },
  { value: 'Other', label: 'Diğer' },
];

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

type TabKey = 'operations' | 'parts' | 'consumables' | 'payments' | 'history' | 'activity' | 'attachments' | 'sharing';

function activityIcon(iconName: string, severity: string): React.ReactElement {
  const cls = severity === 'success' ? 'text-green-600'
    : severity === 'warning' ? 'text-amber-500'
    : severity === 'danger' ? 'text-red-500'
    : 'text-blue-500';
  switch (iconName) {
    case 'wrench': return <Wrench size={14} className={cls} />;
    case 'package': return <Package size={14} className={cls} />;
    case 'credit-card': return <CreditCardIcon size={14} className={cls} />;
    case 'file': return <FileText size={14} className={cls} />;
    default: return <Clock size={14} className={cls} />;
  }
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

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

interface ConsumableCatalogItem {
  category: string;
  subCategory: string | null;
  brand: string;
  productName: string;
  specification: string | null;
  notes: string | null;
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

const defaultAttachmentForm = (): AttachmentForm => ({
  attachmentType: 'Other',
  description: '',
  file: null,
});

// ─── Main component ───────────────────────────────────────────────────────────

export default function ServiceOrderDetailPage(): React.ReactElement {
  const params = useParams();
  const id = params.id as string;
  const router = useRouter();
  const qc = useQueryClient();
  const { user } = useAuth();
  const canWriteServiceOrder = canEditServiceOrder(user?.role);
  const canRecordPayment = canAddPayment(user?.role);

  const [activeTab, setActiveTab] = useState<TabKey>('operations');
  const [deleteTarget, setDeleteTarget] = useState<DeleteTarget | null>(null);
  const [downloadingAttachmentId, setDownloadingAttachmentId] = useState<string | null>(null);

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

  // Consumable catalog search
  const [catalogSearchQuery, setCatalogSearchQuery] = useState<string>('');
  const [showCatalogSuggestions, setShowCatalogSuggestions] = useState<boolean>(false);

  // Add-attachment form
  const [showAddAttachment, setShowAddAttachment] = useState<boolean>(false);
  const [attachmentForm, setAttachmentForm] = useState<AttachmentForm>(defaultAttachmentForm());

  // Inline errors
  const [opError, setOpError] = useState<string>('');
  const [partError, setPartError] = useState<string>('');
  const [consumableError, setConsumableError] = useState<string>('');
  const [paymentError, setPaymentError] = useState<string>('');
  const [statusError, setStatusError] = useState<string>('');
  const [discountError, setDiscountError] = useState<string>('');
  const [attachmentError, setAttachmentError] = useState<string>('');

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
    enabled: canWriteServiceOrder,
    staleTime: 60_000,
  });

  const { data: catalogSuggestions } = useQuery<ConsumableCatalogItem[]>({
    queryKey: ['consumable-catalog', catalogSearchQuery],
    queryFn: async () => {
      if (!catalogSearchQuery.trim()) return [];
      const { data: d } = await apiClient.get<ConsumableCatalogItem[]>('/api/service-orders/consumable-catalog/search', {
        params: { query: catalogSearchQuery, maxResults: 8 },
      });
      return d;
    },
    enabled: catalogSearchQuery.trim().length >= 2 && showCatalogSuggestions,
    staleTime: 30_000,
  });

  const { data: statusHistory, isLoading: isHistoryLoading } = useQuery<StatusHistoryItem[]>({
    queryKey: ['service-order', id, 'status-history'],
    queryFn: async () => {
      const { data: d } = await apiClient.get<StatusHistoryItem[]>(`/api/service-orders/${id}/status-history`);
      return d;
    },
    enabled: activeTab === 'history',
    staleTime: 30_000,
  });

  const { data: activityFeed, isLoading: isActivityLoading } = useQuery<ActivityFeedItem[]>({
    queryKey: ['service-order', id, 'activity'],
    queryFn: async () => {
      const { data: d } = await apiClient.get<ActivityFeedItem[]>(`/api/service-orders/${id}/activity-feed`);
      return d;
    },
    enabled: activeTab === 'activity',
    staleTime: 30_000,
  });

  const { data: attachments, isLoading: isAttachmentsLoading } = useQuery<AttachmentDto[]>({
    queryKey: ['service-order', id, 'attachments'],
    queryFn: async () => {
      const { data: d } = await apiClient.get<AttachmentDto[]>(`/api/service-orders/${id}/attachments`);
      return d;
    },
    enabled: activeTab === 'attachments',
    staleTime: 30_000,
  });

  const { data: publicAccess, refetch: refetchPublicAccess } = useQuery<PublicAccessDto | null>({
    queryKey: ['service-order', id, 'public-access'],
    queryFn: async () => {
      try {
        const { data: d } = await apiClient.get<PublicAccessDto>(`/api/service-orders/${id}/public-access`);
        return d;
      } catch {
        return null;
      }
    },
    staleTime: 60_000,
    throwOnError: false,
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
    onSuccess: () => {
      invalidate();
      setDeleteTarget(null);
      toast.success('İşlem silindi');
    },
    onError: (err: unknown) => toast.error(friendlyError(err, 'İşlem silinemedi.')),
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
    onSuccess: () => {
      invalidate();
      setDeleteTarget(null);
      toast.success('Parça silindi');
    },
    onError: (err: unknown) => toast.error(friendlyError(err, 'Parça silinemedi.')),
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
    onSuccess: () => {
      invalidate();
      setDeleteTarget(null);
      toast.success('Sarf malzeme silindi');
    },
    onError: (err: unknown) => toast.error(friendlyError(err, 'Sarf malzeme silinemedi.')),
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

  const uploadAttachmentMutation = useMutation({
    mutationFn: async ({ file, attachmentType, description }: { file: File; attachmentType: string; description: string }) => {
      const formData = new FormData();
      formData.append('File', file);
      formData.append('AttachmentType', attachmentType);
      if (description) formData.append('Description', description);
      const { data: d } = await apiClient.post<AttachmentDto>(
        `/api/service-orders/${id}/attachments`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
      );
      return d;
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['service-order', id, 'attachments'] });
      setAttachmentForm(defaultAttachmentForm());
      setShowAddAttachment(false);
      setAttachmentError('');
      toast.success('Dosya yüklendi');
    },
    onError: (err: unknown) => {
      const msg = friendlyError(err, 'Dosya yüklenemedi.');
      setAttachmentError(msg);
      toast.error(msg);
    },
  });

  const deleteAttachmentMutation = useMutation({
    mutationFn: async (attachmentId: string) => {
      await apiClient.delete(`/api/service-orders/${id}/attachments/${attachmentId}`);
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['service-order', id, 'attachments'] });
      setDeleteTarget(null);
      toast.success('Dosya silindi');
    },
    onError: (err: unknown) => toast.error(friendlyError(err, 'Dosya silinemedi.')),
  });

  const enablePublicAccessMutation = useMutation({
    mutationFn: async () => {
      await apiClient.post(`/api/service-orders/${id}/public-access`);
      await apiClient.put<PublicAccessDto>(`/api/service-orders/${id}/public-access/enable`);
    },
    onSuccess: () => {
      void refetchPublicAccess();
      invalidate();
      toast.success('Genel erişim etkinleştirildi');
    },
    onError: (err: unknown) => toast.error(friendlyError(err, 'Erişim etkinleştirilemedi.')),
  });

  const disablePublicAccessMutation = useMutation({
    mutationFn: async () => {
      await apiClient.put(`/api/service-orders/${id}/public-access/disable`);
    },
    onSuccess: () => {
      void refetchPublicAccess();
      invalidate();
      toast.success('Genel erişim devre dışı bırakıldı');
    },
    onError: (err: unknown) => toast.error(friendlyError(err, 'Erişim devre dışı bırakılamadı.')),
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

  async function handleAttachmentDownload(attachment: AttachmentDto): Promise<void> {
    setDownloadingAttachmentId(attachment.id);
    try {
      await downloadAuthenticatedFile(attachment.fileUrl, {
        fallbackFileName: attachment.originalFileName,
      });
    } catch (err: unknown) {
      toast.error(friendlyError(err, 'Dosya indirilemedi.'));
    } finally {
      setDownloadingAttachmentId(null);
    }
  }

  function handleConfirmDelete(): void {
    if (!deleteTarget) return;

    switch (deleteTarget.type) {
      case 'operation':
        removeOperationMutation.mutate(deleteTarget.id);
        break;
      case 'part':
        removePartMutation.mutate(deleteTarget.id);
        break;
      case 'consumable':
        removeConsumableMutation.mutate(deleteTarget.id);
        break;
      case 'attachment':
        deleteAttachmentMutation.mutate(deleteTarget.id);
        break;
    }
  }

  // ── Render ─────────────────────────────────────────────────────────────────

  if (isLoading) return <PageLoading />;
  if (error || !data) {
    return <ErrorState message="Servis kaydı bulunamadı." onRetry={() => void refetch()} />;
  }

  const TABS: Array<{ key: TabKey; label: string; count?: number; icon?: React.ReactNode }> = [
    { key: 'operations', label: 'İşlemler', count: data.operations.length },
    { key: 'parts', label: 'Parçalar', count: data.parts.length },
    { key: 'consumables', label: 'Sarf Malzeme', count: data.consumables.length },
    { key: 'payments', label: 'Ödemeler', count: data.payments.length },
    { key: 'history', label: 'Durum Geçmişi', icon: <Clock size={13} /> },
    { key: 'activity', label: 'Aktivite', icon: <Activity size={13} /> },
    { key: 'attachments', label: 'Ekler', icon: <Paperclip size={13} /> },
    { key: 'sharing', label: 'Paylaşım', icon: <Share2 size={13} /> },
  ];
  const deletePending =
    removeOperationMutation.isPending ||
    removePartMutation.isPending ||
    removeConsumableMutation.isPending ||
    deleteAttachmentMutation.isPending;
  const publicUrl = publicAccess?.slug ? publicServiceRecordUrl(publicAccess.slug) : null;

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

      {publicAccess?.isActive && publicUrl && (
        <div className="mb-4">
          <QRLinkCard
            href={publicUrl}
            title="Servis paylaşım QR kodu"
            description="Müşteri servis kaydını bu QR veya bağlantı ile görüntüleyebilir."
          />
        </div>
      )}

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
          {canWriteServiceOrder && (
            <button
              onClick={() => setShowStatusForm((v) => !v)}
              className="btn-secondary text-sm shrink-0"
            >
              Durum Güncelle
              <ChevronDown size={14} />
            </button>
          )}
        </div>

        {/* Status update form (inline dropdown) */}
        {canWriteServiceOrder && showStatusForm && (
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
      {canWriteServiceOrder && (
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
      )}

      {/* Tabs */}
      <div className="flex gap-1 border-b border-slate-200 mb-4 overflow-x-auto scrollbar-none">
        {TABS.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={[
              'flex items-center gap-1.5 px-3 py-2.5 text-sm font-medium transition-colors border-b-2 -mb-px whitespace-nowrap',
              activeTab === tab.key
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-slate-500 hover:text-slate-700',
            ].join(' ')}
          >
            {tab.icon}
            {tab.label}
            {(tab.count ?? 0) > 0 && (
              <span className="rounded-full bg-slate-100 px-1.5 py-0.5 text-xs text-slate-600">
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
            {canWriteServiceOrder && (
              <button
                onClick={() => setShowAddOperation((v) => !v)}
                className="btn-secondary text-sm"
              >
                <Plus size={14} />
                Ekle
              </button>
            )}
          </div>

          {canWriteServiceOrder && showAddOperation && (
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
            <div className="card p-0 overflow-x-auto">
              <table className="table min-w-[720px]">
                <thead>
                  <tr>
                    <th>Açıklama</th>
                    <th className="text-right">Miktar</th>
                    <th className="text-right">Birim Fiyat</th>
                    <th className="text-right">İndirim</th>
                    <th className="text-right">Toplam</th>
                    {canWriteServiceOrder && <th></th>}
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
                      {canWriteServiceOrder && (
                        <td>
                          <button
                            onClick={() => setDeleteTarget({ type: 'operation', id: op.id, label: op.description })}
                            disabled={deletePending}
                            className="btn-ghost text-red-500 hover:text-red-700 p-1"
                            title="Sil"
                          >
                            <Trash2 size={14} />
                          </button>
                        </td>
                      )}
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
            {canWriteServiceOrder && (
              <button
                onClick={() => setShowAddPart((v) => !v)}
                className="btn-secondary text-sm"
              >
                <Plus size={14} />
                Ekle
              </button>
            )}
          </div>

          {canWriteServiceOrder && showAddPart && (
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
            <div className="card p-0 overflow-x-auto">
              <table className="table min-w-[760px]">
                <thead>
                  <tr>
                    <th>Parça</th>
                    <th>Parça No</th>
                    <th className="text-right">Miktar</th>
                    <th className="text-right">Birim Fiyat</th>
                    <th className="text-right">İndirim</th>
                    <th className="text-right">Toplam</th>
                    {canWriteServiceOrder && <th></th>}
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
                      {canWriteServiceOrder && (
                        <td>
                          <button
                            onClick={() => setDeleteTarget({ type: 'part', id: part.id, label: part.partName })}
                            disabled={deletePending}
                            className="btn-ghost text-red-500 hover:text-red-700 p-1"
                            title="Sil"
                          >
                            <Trash2 size={14} />
                          </button>
                        </td>
                      )}
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
            {canWriteServiceOrder && (
              <button
                onClick={() => setShowAddConsumable((v) => !v)}
                className="btn-secondary text-sm"
              >
                <Plus size={14} />
                Ekle
              </button>
            )}
          </div>

          {canWriteServiceOrder && showAddConsumable && (
            <div className="card p-4 mb-4">
              {consumableError && <p className="error-text mb-2">{consumableError}</p>}
              <form onSubmit={handleAddConsumable} className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                {/* Catalog search autocomplete */}
                <div className="form-group col-span-2 sm:col-span-3 relative">
                  <label className="label">Katalogdan Ara</label>
                  <input
                    className="input"
                    placeholder="Ürün adı veya kategori yazın..."
                    value={catalogSearchQuery}
                    onChange={(e) => {
                      setCatalogSearchQuery(e.target.value);
                      setShowCatalogSuggestions(true);
                    }}
                    onFocus={() => setShowCatalogSuggestions(true)}
                    onBlur={() => setTimeout(() => setShowCatalogSuggestions(false), 150)}
                    autoComplete="off"
                  />
                  {showCatalogSuggestions && catalogSuggestions && catalogSuggestions.length > 0 && (
                    <ul className="absolute z-20 left-0 right-0 top-full mt-1 bg-white border border-slate-200 rounded-lg shadow-lg max-h-48 overflow-y-auto">
                      {catalogSuggestions.map((item, idx) => (
                        <li key={idx}>
                          <button
                            type="button"
                            className="w-full text-left px-3 py-2 text-sm hover:bg-slate-50 flex items-start gap-3"
                            onMouseDown={() => {
                              setConsumableForm((p) => ({
                                ...p,
                                category: item.category,
                                productName: item.productName,
                                brand: item.brand,
                              }));
                              setCatalogSearchQuery(`${item.brand} ${item.productName}`);
                              setShowCatalogSuggestions(false);
                            }}
                          >
                            <div>
                              <span className="font-medium text-slate-900">{item.brand} {item.productName}</span>
                              <span className="ml-2 text-xs text-slate-400">{item.category}</span>
                              {item.specification && (
                                <span className="ml-1 text-xs text-slate-400">· {item.specification}</span>
                              )}
                            </div>
                          </button>
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
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
                  <button type="button" onClick={() => { setShowAddConsumable(false); setCatalogSearchQuery(''); }} className="btn-ghost text-sm">
                    İptal
                  </button>
                </div>
              </form>
            </div>
          )}

          {data.consumables.length === 0 ? (
            <div className="card p-6 text-center text-sm text-slate-400">Henüz sarf malzeme eklenmedi.</div>
          ) : (
            <div className="card p-0 overflow-x-auto">
              <table className="table min-w-[680px]">
                <thead>
                  <tr>
                    <th>Ürün</th>
                    <th>Marka</th>
                    <th className="text-right">Miktar</th>
                    <th className="text-right">Birim Fiyat</th>
                    <th className="text-right">Toplam</th>
                    {canWriteServiceOrder && <th></th>}
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
                      {canWriteServiceOrder && (
                        <td>
                          <button
                            onClick={() => setDeleteTarget({ type: 'consumable', id: c.id, label: c.productName })}
                            disabled={deletePending}
                            className="btn-ghost text-red-500 hover:text-red-700 p-1"
                            title="Sil"
                          >
                            <Trash2 size={14} />
                          </button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* ── Tab: Status History ── */}
      {activeTab === 'history' && (
        <div>
          <h2 className="text-sm font-semibold text-slate-700 mb-3">Durum Geçmişi</h2>
          {isHistoryLoading ? (
            <div className="card p-6 text-center text-sm text-slate-400">Yükleniyor...</div>
          ) : !statusHistory || statusHistory.length === 0 ? (
            <div className="card p-6 text-center text-sm text-slate-400">Durum değişikliği kaydı bulunamadı.</div>
          ) : (
            <div className="relative pl-6">
              <div className="absolute left-2 top-2 bottom-2 w-px bg-slate-200" />
              <ul className="space-y-3">
                {statusHistory.map((item) => (
                  <li key={item.id} className="relative">
                    <div className="absolute -left-[1.1rem] top-3 w-2.5 h-2.5 rounded-full bg-blue-500 border-2 border-white" />
                    <div className="card p-3">
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="text-sm font-medium text-slate-900">
                            {item.fromStatusText ? `${item.fromStatusText} → ` : ''}{item.toStatusText}
                          </p>
                          {item.note && (
                            <p className="text-xs text-slate-500 mt-0.5 italic">&ldquo;{item.note}&rdquo;</p>
                          )}
                        </div>
                        <div className="text-right shrink-0">
                          <p className="text-xs text-slate-400">{dateTimeText(item.createdAt)}</p>
                          {item.changedByUserName && (
                            <p className="text-xs text-slate-400">{item.changedByUserName}</p>
                          )}
                        </div>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}

      {/* ── Tab: Activity Feed ── */}
      {activeTab === 'activity' && (
        <div>
          <h2 className="text-sm font-semibold text-slate-700 mb-3">Aktivite Geçmişi</h2>
          {isActivityLoading ? (
            <div className="card p-6 text-center text-sm text-slate-400">Yükleniyor...</div>
          ) : !activityFeed || activityFeed.length === 0 ? (
            <div className="card p-6 text-center text-sm text-slate-400">Aktivite bulunamadı.</div>
          ) : (
            <ul className="space-y-2">
              {activityFeed.map((item) => (
                <li key={item.id} className="card p-3 flex items-start gap-3">
                  <div className="mt-0.5 shrink-0">{activityIcon(item.iconName, item.severity)}</div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-slate-900">{item.title}</p>
                    <p className="text-xs text-slate-500 truncate">{item.description}</p>
                  </div>
                  <div className="text-right shrink-0 space-y-0.5">
                    {item.amount !== null && item.amount !== undefined && (
                      <p className="text-xs font-semibold text-slate-700">
                        {item.amount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
                      </p>
                    )}
                    <p className="text-xs text-slate-400">{dateTimeText(item.createdAt)}</p>
                    <p className="text-xs text-slate-400">{item.actorName}</p>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {/* ── Tab: Attachments ── */}
      {activeTab === 'attachments' && (
        <div>
          <div className="flex justify-between items-center mb-3">
            <h2 className="text-sm font-semibold text-slate-700">Dosya Ekleri</h2>
            {canWriteServiceOrder && (
              <button onClick={() => setShowAddAttachment((v) => !v)} className="btn-secondary text-sm">
                <Upload size={14} />
                Dosya Yükle
              </button>
            )}
          </div>

          {canWriteServiceOrder && showAddAttachment && (
            <div className="card p-4 mb-4">
              {attachmentError && <p className="error-text mb-2">{attachmentError}</p>}
              <div className="grid sm:grid-cols-3 gap-3">
                <div className="form-group">
                  <label className="label">Dosya Türü</label>
                  <select
                    className="input"
                    value={attachmentForm.attachmentType}
                    onChange={(e) => setAttachmentForm((p) => ({ ...p, attachmentType: e.target.value }))}
                  >
                    {ATTACHMENT_TYPES.map((t) => (
                      <option key={t.value} value={t.value}>{t.label}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="label">Açıklama</label>
                  <input
                    className="input"
                    value={attachmentForm.description}
                    onChange={(e) => setAttachmentForm((p) => ({ ...p, description: e.target.value }))}
                    placeholder="İsteğe bağlı..."
                  />
                </div>
                <div className="form-group">
                  <label className="label">Dosya *</label>
                  <input
                    type="file"
                    className="input py-1.5 text-sm"
                    accept="image/*,.pdf,.doc,.docx,.xls,.xlsx"
                    onChange={(e) => setAttachmentForm((p) => ({ ...p, file: e.target.files?.[0] ?? null }))}
                  />
                </div>
                <div className="sm:col-span-3 flex gap-2">
                  <button
                    onClick={() => {
                      if (!attachmentForm.file) { setAttachmentError('Lütfen bir dosya seçin.'); return; }
                      uploadAttachmentMutation.mutate({
                        file: attachmentForm.file,
                        attachmentType: attachmentForm.attachmentType,
                        description: attachmentForm.description,
                      });
                    }}
                    disabled={uploadAttachmentMutation.isPending}
                    className="btn-primary text-sm"
                  >
                    {uploadAttachmentMutation.isPending ? 'Yükleniyor...' : 'Yükle'}
                  </button>
                  <button type="button" onClick={() => setShowAddAttachment(false)} className="btn-ghost text-sm">
                    İptal
                  </button>
                </div>
              </div>
            </div>
          )}

          {isAttachmentsLoading ? (
            <div className="card p-6 text-center text-sm text-slate-400">Yükleniyor...</div>
          ) : !attachments || attachments.length === 0 ? (
            <div className="card p-6 text-center text-sm text-slate-400">Henüz dosya eki yok.</div>
          ) : (
            <div className="card p-0 overflow-x-auto">
              <table className="table min-w-[760px]">
                <thead>
                  <tr>
                    <th>Dosya</th>
                    <th>Tür</th>
                    <th>Boyut</th>
                    <th>Yükleyen</th>
                    <th>Tarih</th>
                    {canWriteServiceOrder && <th></th>}
                  </tr>
                </thead>
                <tbody>
                  {attachments.map((att) => (
                    <tr key={att.id}>
                      <td>
                        <button
                          type="button"
                          onClick={() => void handleAttachmentDownload(att)}
                          disabled={downloadingAttachmentId === att.id}
                          className="flex max-w-full items-center gap-1.5 text-left text-sm text-brand-600 hover:underline disabled:cursor-wait disabled:opacity-60"
                        >
                          <FileText size={14} className="shrink-0" />
                          <span className="truncate max-w-[200px]">
                            {downloadingAttachmentId === att.id ? 'İndiriliyor...' : att.originalFileName}
                          </span>
                          <ExternalLink size={11} className="shrink-0" />
                        </button>
                        {att.description && (
                          <p className="text-xs text-slate-400 mt-0.5">{att.description}</p>
                        )}
                      </td>
                      <td className="text-sm text-slate-600">{att.attachmentTypeText}</td>
                      <td className="text-sm text-slate-500">{formatBytes(att.fileSize)}</td>
                      <td className="text-sm text-slate-500">{att.uploadedByUserName ?? '-'}</td>
                      <td className="text-sm text-slate-400">{dateText(att.createdAt)}</td>
                      {canWriteServiceOrder && (
                        <td>
                          <button
                            onClick={() => setDeleteTarget({
                              type: 'attachment',
                              id: att.id,
                              label: att.originalFileName,
                            })}
                            disabled={deletePending}
                            className="btn-ghost text-red-500 hover:text-red-700 p-1"
                            title="Sil"
                          >
                            <Trash2 size={14} />
                          </button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* ── Tab: Sharing ── */}
      {activeTab === 'sharing' && (
        <div className="max-w-lg space-y-4">
          <div className="card p-5">
            <div className="flex items-center justify-between gap-4 mb-4">
              <div className="flex items-center gap-2">
                {publicAccess?.isActive
                  ? <Globe size={18} className="text-green-600" />
                  : <GlobeLock size={18} className="text-slate-400" />}
                <div>
                  <p className="font-medium text-slate-900">
                    {publicAccess?.isActive ? 'Genel Erişim Açık' : 'Genel Erişim Kapalı'}
                  </p>
                  <p className="text-xs text-slate-500">
                    {publicAccess?.isActive
                      ? 'Müşteri bu bağlantı ile servis kaydını görebilir.'
                      : 'Paylaşımı etkinleştirerek müşteriye bağlantı verebilirsiniz.'}
                  </p>
                </div>
              </div>
              {canWriteServiceOrder && (
                publicAccess?.isActive ? (
                  <button
                    onClick={() => disablePublicAccessMutation.mutate()}
                    disabled={disablePublicAccessMutation.isPending}
                    className="btn-secondary text-sm text-rose-600 border-rose-200 hover:bg-rose-50 shrink-0"
                  >
                    <GlobeLock size={14} />
                    {disablePublicAccessMutation.isPending ? 'Kapatılıyor...' : 'Kapat'}
                  </button>
                ) : (
                  <button
                    onClick={() => enablePublicAccessMutation.mutate()}
                    disabled={enablePublicAccessMutation.isPending}
                    className="btn-primary text-sm shrink-0"
                  >
                    <Globe size={14} />
                    {enablePublicAccessMutation.isPending ? 'Açılıyor...' : 'Paylaşımı Aç'}
                  </button>
                )
              )}
            </div>

            {publicAccess?.isActive && publicUrl && (
              <div className="space-y-2 border-t border-slate-100 pt-4">
                <p className="text-xs text-slate-500 font-medium">Bağlantı</p>
                <div className="flex items-center gap-2">
                  <input
                    readOnly
                    value={publicUrl}
                    className="input text-xs flex-1 bg-slate-50"
                    onFocus={(e) => e.target.select()}
                  />
                  <a
                    href={publicUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="btn-secondary text-sm shrink-0"
                  >
                    <ExternalLink size={14} />
                    Aç
                  </a>
                </div>
              </div>
            )}

            {!publicUrl && (
              <p className="border-t border-slate-100 pt-4 text-sm text-slate-500">
                Paylaşım bağlantısı henüz oluşturulmamış.
              </p>
            )}

            {publicAccess && (
              <div className="flex gap-4 mt-4 text-xs text-slate-500 border-t border-slate-100 pt-3">
                <span>Görüntülenme: <strong className="text-slate-700">{publicAccess.accessCount}</strong></span>
                {publicAccess.lastAccessedAtUtc && (
                  <span>Son erişim: <strong className="text-slate-700">{dateText(publicAccess.lastAccessedAtUtc)}</strong></span>
                )}
              </div>
            )}
          </div>
        </div>
      )}

      {/* ── Tab: Payments ── */}
      {activeTab === 'payments' && (
        <div>
          <div className="flex justify-between items-center mb-3">
            <h2 className="text-sm font-semibold text-slate-700">Ödemeler</h2>
            {canRecordPayment && (
              <button
                onClick={() => setShowAddPayment((v) => !v)}
                className="btn-secondary text-sm"
              >
                <Plus size={14} />
                Ekle
              </button>
            )}
          </div>

          {canRecordPayment && showAddPayment && (
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
            <div className="card p-0 overflow-x-auto">
              <table className="table min-w-[480px]">
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

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => {
          if (!deletePending) setDeleteTarget(null);
        }}
        onConfirm={handleConfirmDelete}
        title="Kaydı silmek istiyor musunuz?"
        description={
          deleteTarget
            ? `"${deleteTarget.label}" kalıcı olarak silinecek. Bu işlem geri alınamaz.`
            : ''
        }
        confirmLabel="Sil"
        cancelLabel="İptal"
        variant="danger"
        loading={deletePending}
      />
    </div>
  );
}
