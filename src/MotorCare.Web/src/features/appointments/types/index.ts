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
  { value: 'Maintenance', label: 'Bakim' },
  { value: 'Repair', label: 'Onarim' },
  { value: 'Cleaning', label: 'Temizlik' },
  { value: 'Washing', label: 'Yikama' },
  { value: 'Inspection', label: 'Ekspertiz' },
  { value: 'TireChange', label: 'Lastik Degisimi' },
  { value: 'Other', label: 'Diger' },
];

export const APPOINTMENT_STATUS_OPTIONS: Array<{ value: AppointmentStatus; label: string }> = [
  { value: 'Scheduled', label: 'Planlandi' },
  { value: 'Confirmed', label: 'Onaylandi' },
  { value: 'CheckedIn', label: 'Servise Alindi' },
  { value: 'ConvertedToOrder', label: 'Is Emrine Donustu' },
  { value: 'Cancelled', label: 'Iptal' },
  { value: 'NoShow', label: 'Gelmedi' },
  { value: 'Completed', label: 'Tamamlandi' },
];

export function appointmentTypeLabel(type: AppointmentType | string, fallback?: string): string {
  return fallback || APPOINTMENT_TYPE_OPTIONS.find((item) => item.value === type)?.label || type;
}

export function appointmentStatusLabel(status: AppointmentStatus | string, fallback?: string): string {
  return fallback || APPOINTMENT_STATUS_OPTIONS.find((item) => item.value === status)?.label || status;
}
