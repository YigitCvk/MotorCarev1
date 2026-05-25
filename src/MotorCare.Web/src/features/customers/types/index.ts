import type { Vehicle } from '@/features/vehicles/types';

export interface Customer {
  id: string;
  fullName: string;
  phone?: string | null;
  email?: string | null;
  whatsapp?: string | null;
  notes?: string | null;
  address?: string | null;
  taxNumber?: string | null;
  taxOffice?: string | null;
  vehicleCount?: number;
}

export interface CustomerDetail extends Customer {
  vehicles?: Vehicle[];
}
