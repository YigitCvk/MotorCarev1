export type AppointmentType =
  | 'Maintenance'
  | 'Repair'
  | 'Cleaning'
  | 'Washing'
  | 'Inspection'
  | 'TireChange'
  | 'Other';

export type AppointmentStatus =
  | 'Scheduled'
  | 'Confirmed'
  | 'CheckedIn'
  | 'ConvertedToOrder'
  | 'Cancelled'
  | 'NoShow'
  | 'Completed';

const APPOINTMENT_TYPE_API_VALUES: Record<AppointmentType, number> = {
  Maintenance: 1,
  Repair: 2,
  Cleaning: 3,
  Washing: 4,
  Inspection: 5,
  TireChange: 6,
  Other: 7,
};

const APPOINTMENT_STATUS_API_VALUES: Record<AppointmentStatus, number> = {
  Scheduled: 1,
  Confirmed: 2,
  CheckedIn: 3,
  ConvertedToOrder: 4,
  Cancelled: 5,
  NoShow: 6,
  Completed: 7,
};

export function appointmentTypeToApi(type: AppointmentType): number {
  return APPOINTMENT_TYPE_API_VALUES[type];
}

export function appointmentStatusToApi(status: AppointmentStatus): number {
  return APPOINTMENT_STATUS_API_VALUES[status];
}

export function appointmentTypeFromApi(value: unknown): AppointmentType {
  if (typeof value === 'string' && value in APPOINTMENT_TYPE_API_VALUES) {
    return value as AppointmentType;
  }

  const entry = Object.entries(APPOINTMENT_TYPE_API_VALUES).find(([, apiValue]) => apiValue === value);
  return (entry?.[0] as AppointmentType | undefined) ?? 'Other';
}

export function appointmentStatusFromApi(value: unknown): AppointmentStatus {
  if (typeof value === 'string' && value in APPOINTMENT_STATUS_API_VALUES) {
    return value as AppointmentStatus;
  }

  const entry = Object.entries(APPOINTMENT_STATUS_API_VALUES).find(([, apiValue]) => apiValue === value);
  return (entry?.[0] as AppointmentStatus | undefined) ?? 'Scheduled';
}

export interface AppointmentCustomerOption {
  id: string;
  fullName: string;
  phone?: string;
}

export interface AppointmentVehicleOption {
  id: string;
  plate: string;
  brand?: string;
  model?: string;
  year?: number;
}

export interface AppointmentDto {
  id: string;
  customerId: string | null;
  vehicleId: string | null;
  customerName: string;
  phone: string;
  plate: string | null;
  type: AppointmentType;
  typeText: string;
  status: AppointmentStatus;
  statusText: string;
  startAt: string;
  endAt: string;
  note: string | null;
  complaint: string | null;
  serviceOrderId: string | null;
}

export interface AppointmentUpsertRequest {
  customerId: string | null;
  vehicleId: string | null;
  customerName: string;
  phone: string;
  plate: string | null;
  type: AppointmentType;
  startAt: string;
  endAt: string;
  note: string | null;
  complaint: string | null;
}

export interface ConvertAppointmentResponse {
  serviceOrderId: string;
}

export const APPOINTMENT_TYPE_OPTIONS: Array<{ value: AppointmentType; label: string }> = [
  { value: 'Maintenance', label: 'Bakım' },
  { value: 'Repair', label: 'Onarım' },
  { value: 'Cleaning', label: 'Temizlik' },
  { value: 'Washing', label: 'Yıkama' },
  { value: 'Inspection', label: 'Ekspertiz' },
  { value: 'TireChange', label: 'Lastik Değişimi' },
  { value: 'Other', label: 'Diğer' },
];

export const APPOINTMENT_STATUS_OPTIONS: Array<{ value: AppointmentStatus; label: string }> = [
  { value: 'Scheduled', label: 'Planlandı' },
  { value: 'Confirmed', label: 'Onaylandı' },
  { value: 'CheckedIn', label: 'Servise Alındı' },
  { value: 'ConvertedToOrder', label: 'İş Emrine Dönüştü' },
  { value: 'Cancelled', label: 'İptal' },
  { value: 'NoShow', label: 'Gelmedi' },
  { value: 'Completed', label: 'Tamamlandı' },
];

export function appointmentTypeLabel(type: AppointmentType | string, fallback?: string): string {
  return APPOINTMENT_TYPE_OPTIONS.find((item) => item.value === type)?.label || fallback || type;
}

export function appointmentStatusLabel(status: AppointmentStatus | string, fallback?: string): string {
  return APPOINTMENT_STATUS_OPTIONS.find((item) => item.value === status)?.label || fallback || status;
}
