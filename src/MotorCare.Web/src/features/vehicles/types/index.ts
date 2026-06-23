export interface Vehicle {
  id: string;
  customerId?: string | null;
  customerName?: string | null;
  plate?: string;
  plateOriginal?: string;
  plateNormalized?: string;
  brand?: string;
  model?: string;
  year?: number;
  vehicleDisplay?: string;
  vehicleType?: string | null;
  motorcycleType?: string | null;
  chassisNumber?: string | null;
  engineNumber?: string | null;
  color?: string | null;
  currentKm?: number | null;
  notes?: string | null;
}

export interface VehicleHistoryEntry {
  id?: string;
  serviceOrderId?: string;
  orderNo: string;
  openedAt: string;
  closedAt?: string | null;
  status: string;
  statusText?: string;
  vehicleKm?: number;
  grandTotal?: number;
  paidTotal?: number;
  remainingTotal?: number;
  complaint?: string | null;
}

export interface VehicleHistoryResponse {
  vehicleId?: string;
  id?: string;
  plate?: string;
  brand?: string;
  model?: string;
  year?: number;
  vehicleDisplay?: string;
  currentKm?: number | null;
  totalServiceOrderCount?: number;
  lastServiceDate?: string | null;
  totalSpent?: number;
  history?: VehicleHistoryEntry[];
  serviceOrders?: VehicleHistoryEntry[];
}

export type VehicleHistoryApiResponse = VehicleHistoryResponse | VehicleHistoryEntry[];
