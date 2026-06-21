import type { AxiosError } from 'axios';
import type { ApiProblem } from '@/shared/types/api.types';

export function normalizePlate(value: string): string {
  return value.replace(/[^a-zA-Z0-9]/g, '').toLocaleUpperCase('tr-TR');
}

export function getVehiclePlate(vehicle: {
  plate?: string;
  plateOriginal?: string;
  plateNormalized?: string;
}): string {
  return vehicle.plate ?? vehicle.plateOriginal ?? vehicle.plateNormalized ?? '';
}

export function vehicleDuplicateMessage(error: unknown): string | null {
  const axiosErr = error as AxiosError<ApiProblem>;
  const status = axiosErr?.response?.status;
  const data = axiosErr?.response?.data;
  const text = `${data?.code ?? ''} ${data?.message ?? ''}`.toLocaleLowerCase('tr-TR');

  if (
    status === 409 ||
    text.includes('duplicate') ||
    text.includes('already') ||
    text.includes('zaten') ||
    text.includes('plaka')
  ) {
    return 'Bu plakaya ait bir araç sistemde zaten kayıtlı.';
  }

  return null;
}
