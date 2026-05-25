// src/shared/types/api.types.ts

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages?: number;
}

export interface ApiProblem {
  code?: string;
  message?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export interface LoginRequest {
  tenantIdentifier: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  tenantId: string;
  tenantIdentifier: string;
  email: string;
  role: string;
  requiresTwoFactor?: boolean;
  twoFactorToken?: string;
}

export interface CurrentUser {
  userId?: string;
  id?: string;
  tenantId?: string;
  tenantIdentifier?: string;
  email: string;
  fullName?: string;
  role: string;
}

export type UserRole = 'Owner' | 'Admin' | 'Manager' | 'Technician' | 'Inspector' | 'Accountant' | 'ReadOnly';

export const ROLE_VALUES: Record<string, number> = {
  Owner: 1,
  Admin: 2,
  Receptionist: 3,
  Technician: 4,
  Manager: 5,
  Inspector: 6,
  Accountant: 7,
  ReadOnly: 8,
};

export const ROLE_LABELS: Record<string, string> = {
  Owner: 'Sahip',
  Admin: 'Yönetici',
  Manager: 'Müdür',
  Technician: 'Teknisyen',
  Inspector: 'Eksper',
  Accountant: 'Muhasebe',
  ReadOnly: 'Salt Okuma',
};

export interface TenantProfile {
  name: string;
  legalName?: string;
  taxNumber?: string;
  taxOffice?: string;
  address?: string;
  phone?: string;
  email?: string;
  website?: string;
  logoUrl?: string;
}

export interface EntityRecord {
  [key: string]: unknown;
}
