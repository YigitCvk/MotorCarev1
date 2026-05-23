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
  twoFactorExpiresAt?: string;
  twoFactorProvider?: string;
}

export interface CurrentUser {
  id?: string;
  userId?: string;
  tenantId?: string;
  tenantIdentifier?: string;
  email: string;
  fullName?: string;
  role: string;
}

export interface AuthActionResponse {
  message: string;
}

export type EntityRecord = Record<string, unknown>;

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
