export type ServiceOrderStatus =
  | 'Open'
  | 'InProgress'
  | 'WaitingForParts'
  | 'Completed'
  | 'Cancelled'
  | 'Delivered';

export type PaymentMethodValue = 1 | 2 | 3;

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ServiceOperationItem {
  id: string;
  description: string;
  price: number;
  quantity: number;
  unitPrice: number;
  discount: number;
  lineTotal: number;
  notes?: string | null;
  serviceCatalogItemId?: string | null;
}

export interface ServicePartItem {
  id: string;
  partName: string;
  partNumber?: string | null;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  discount: number;
  lineTotal: number;
  notes?: string | null;
  inventoryItemId?: string | null;
}

export interface ServiceConsumableItem {
  id: string;
  category: string;
  brand?: string | null;
  productName: string;
  subCategory?: string | null;
  specification?: string | null;
  notes?: string | null;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface ServicePayment {
  id: string;
  amount: number;
  method: string;
  paymentDate: string;
}

export interface ServiceOrder {
  id: string;
  orderNo: string;
  vehicleId: string;
  customerId: string;
  customerName?: string | null;
  vehiclePlate?: string | null;
  vehicleDisplay?: string | null;
  status: ServiceOrderStatus | string;
  openedAt: string;
  updatedAt?: string | null;
  closedAt?: string | null;
  vehicleKm: number;
  complaint?: string | null;
  workDescription?: string | null;
  internalNote?: string | null;
  laborTotal: number;
  partsTotal: number;
  consumablesTotal: number;
  discountTotal: number;
  grandTotal: number;
  paidTotal: number;
  remainingTotal: number;
  operations: ServiceOperationItem[];
  parts: ServicePartItem[];
  consumables: ServiceConsumableItem[];
  payments: ServicePayment[];
}

export interface ServiceOrderListQuery {
  customerId?: string | null;
  status?: ServiceOrderStatus | null;
  q?: string | null;
  openedFrom?: string | null;
  openedTo?: string | null;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateServiceOrderRequest {
  vehicleId: string;
  customerId: string;
  vehicleKm: number;
  complaint?: string | null;
  consumables?: CreateServiceOrderConsumableItem[] | null;
}

export interface CreateServiceOrderConsumableItem {
  category: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  brand?: string | null;
  subCategory?: string | null;
  specification?: string | null;
  notes?: string | null;
}

export interface AddOperationToOrderRequest {
  description: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  notes?: string | null;
  serviceCatalogItemId?: string | null;
}

export interface AddPartToOrderRequest {
  partName: string;
  partNumber?: string | null;
  unitPrice: number;
  quantity: number;
  inventoryItemId?: string | null;
  discount: number;
  notes?: string | null;
}

export interface AddConsumableToOrderRequest {
  category: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  brand?: string | null;
  subCategory?: string | null;
  specification?: string | null;
  notes?: string | null;
}

export interface AddPaymentToOrderRequest {
  amount: number;
  method: PaymentMethodValue;
  paymentDate?: string | null;
}

export interface SetOrderDiscountRequest {
  discount: number;
}

export interface UpdateServiceOrderStatusRequest {
  status: number;
  note?: string | null;
}

export interface CustomerLookup {
  id: string;
  fullName: string;
  phone?: string | null;
  email?: string | null;
  whatsapp?: string | null;
  notes?: string | null;
}

export interface VehicleLookup {
  id: string;
  customerId?: string | null;
  customerName?: string | null;
  plateOriginal: string;
  plateNormalized: string;
  brand: string;
  model: string;
  year: number;
  vehicleDisplay: string;
  chassisNumber?: string | null;
  engineNumber?: string | null;
  currentKm?: number | null;
}

export interface ApiProblem {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export const SERVICE_ORDER_STATUS_OPTIONS: ReadonlyArray<{ value: ServiceOrderStatus; label: string }> = [
  { value: 'Open', label: 'Açık' },
  { value: 'InProgress', label: 'İşlemde' },
  { value: 'WaitingForParts', label: 'Parça Bekliyor' },
  { value: 'Completed', label: 'Teslime Hazır' },
  { value: 'Delivered', label: 'Teslim Edildi' },
  { value: 'Cancelled', label: 'İptal' }
];

export const PAYMENT_METHOD_OPTIONS: ReadonlyArray<{ value: PaymentMethodValue; label: string }> = [
  { value: 1, label: 'Nakit' },
  { value: 2, label: 'Kredi Kartı' },
  { value: 3, label: 'Havale/EFT' }
];

export function serviceOrderStatusLabel(status?: string | null): string {
  return SERVICE_ORDER_STATUS_OPTIONS.find((option) => option.value === status)?.label ?? status ?? '-';
}

export function serviceOrderStatusApiValue(status: ServiceOrderStatus): number {
  switch (status) {
    case 'Open':
      return 1;
    case 'InProgress':
      return 2;
    case 'WaitingForParts':
      return 3;
    case 'Completed':
      return 4;
    case 'Cancelled':
      return 5;
    case 'Delivered':
      return 6;
  }
}

export function paymentMethodLabel(method?: string | number | null): string {
  if (typeof method === 'number') {
    return PAYMENT_METHOD_OPTIONS.find((option) => option.value === method)?.label ?? String(method);
  }

  switch (method) {
    case 'Cash':
      return 'Nakit';
    case 'CreditCard':
      return 'Kredi Kartı';
    case 'BankTransfer':
      return 'Havale/EFT';
    default:
      return method ?? '-';
  }
}

export function roundCurrency(value: number): number {
  return Math.round((Number(value || 0) + Number.EPSILON) * 100) / 100;
}

export function lineTotal(quantity: number, unitPrice: number, discount: number): number {
  return Math.max(0, roundCurrency(Number(quantity || 0) * Number(unitPrice || 0) - Number(discount || 0)));
}

export function normalizeOptionalText(value?: string | null): string | null {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}
