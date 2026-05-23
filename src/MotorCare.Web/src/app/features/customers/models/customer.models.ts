export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CustomerLookupResponse {
  id: string;
  fullName: string;
  phone?: string | null;
  email?: string | null;
  whatsapp?: string | null;
  notes?: string | null;
}

export interface CreateCustomerRequest {
  fullName: string;
  phone: string;
  email?: string | null;
  whatsapp?: string | null;
  notes?: string | null;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {}

export interface CustomerSummary extends CustomerLookupResponse {
  vehicles: CustomerVehicleItem[];
  appointments: CustomerAppointmentHistoryItem[];
  serviceOrders: CustomerServiceOrderHistoryItem[];
}

export interface CustomerVehicleItem {
  id: string;
  plate: string;
  brand: string;
  model: string;
  year: number;
  vehicleDisplay: string;
  currentKm?: number | null;
  lastServiceDate?: string | null;
}

export interface CustomerAppointmentHistoryItem {
  id: string;
  type: number;
  typeText: string;
  status: number;
  statusText: string;
  startAt: string;
  endAt: string;
  plate?: string | null;
  serviceOrderId?: string | null;
}

export interface CustomerServiceOrderHistoryItem {
  id: string;
  orderNo: string;
  status: string;
  statusText: string;
  openedAt: string;
  closedAt?: string | null;
  vehiclePlate?: string | null;
  complaint?: string | null;
  grandTotal: number;
  paidTotal: number;
  operations: CustomerOperationHistoryItem[];
  parts: CustomerPartHistoryItem[];
  payments: CustomerPaymentHistoryItem[];
}

export interface CustomerOperationHistoryItem {
  description: string;
  price: number;
}

export interface CustomerPartHistoryItem {
  partName: string;
  partNumber?: string | null;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface CustomerPaymentHistoryItem {
  amount: number;
  method: string | number;
  methodText: string;
  paymentDate: string;
}
