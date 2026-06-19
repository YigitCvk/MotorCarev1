// src/shared/types/api.types.ts

import type { UserRole } from '@/shared/constants/roles';

export type { UserRole } from '@/shared/constants/roles';

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
  role: UserRole;
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
  role: UserRole;
}

export interface RegisterResponse {
  tenantId: string;
  tenantIdentifier: string;
  ownerId: string;
  ownerEmail: string;
  verificationEmailSent: boolean;
}

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
