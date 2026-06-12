// src/core/auth/auth.service.ts
import apiClient from '@/core/api/client';
import {
  clearTokens,
  getCurrentUserFromStorage,
  setCurrentUserInStorage,
  setTokens,
} from '@/core/auth/storage';
import type { CurrentUser, LoginRequest, LoginResponse } from '@/shared/types/api.types';

const AUTH_REDIRECT_ORIGIN = 'https://garajpass.local';
const AUTH_ROUTES = [
  '/accept-invite',
  '/forgot-password',
  '/login',
  '/register',
  '/reset-password',
  '/two-factor',
  '/verify-email',
];

function applyLoginResponse(response: LoginResponse): CurrentUser {
  setTokens(response.accessToken, response.refreshToken);
  const user: CurrentUser = {
    userId: response.userId,
    tenantId: response.tenantId,
    tenantIdentifier: response.tenantIdentifier,
    email: response.email,
    role: response.role,
  };
  setCurrentUserInStorage(user);
  return user;
}

export function sanitizeAuthRedirect(value: string | null): string | undefined {
  if (!value) return undefined;

  try {
    const candidate = value.trim();
    if (!candidate.startsWith('/')) return undefined;

    const url = new URL(candidate, AUTH_REDIRECT_ORIGIN);
    if (url.origin !== AUTH_REDIRECT_ORIGIN) return undefined;

    const pathname = url.pathname.toLowerCase().replace(/\/+$/, '') || '/';
    const isAuthRoute = AUTH_ROUTES.some(
      (route) => pathname === route || pathname.startsWith(`${route}/`),
    );
    if (isAuthRoute) return undefined;

    return `${url.pathname}${url.search}${url.hash}`;
  } catch {
    return undefined;
  }
}

export const authService = {
  async login(request: LoginRequest): Promise<LoginResponse> {
    const { data } = await apiClient.post<LoginResponse>('/api/auth/login', request);
    if (!data.requiresTwoFactor) {
      applyLoginResponse(data);
    }
    return data;
  },

  async register(body: {
    tenantIdentifier: string;
    tenantName: string;
    ownerFullName: string;
    ownerEmail: string;
    ownerPassword: string;
  }): Promise<void> {
    await apiClient.post('/api/auth/register', body);
  },

  async verifyEmail(body: { tenantIdentifier: string; email: string; code: string }): Promise<void> {
    await apiClient.post('/api/auth/verify-email-code', body);
  },

  async resendVerificationCode(body: { email: string; tenantIdentifier: string }): Promise<void> {
    await apiClient.post('/api/auth/resend-email-verification-code', body);
  },

  async forgotPassword(body: { email: string; tenantIdentifier: string }): Promise<void> {
    await apiClient.post('/api/auth/forgot-password', body);
  },

  async resetPassword(body: {
    tenantIdentifier: string;
    email: string;
    code: string;
    newPassword: string;
    confirmPassword: string;
  }): Promise<void> {
    await apiClient.post('/api/auth/reset-password', body);
  },

  async validateInvite(token: string): Promise<{ email: string; fullName?: string; role: string; isValid: boolean }> {
    const { data } = await apiClient.get(`/api/users/invitations/${encodeURIComponent(token)}/validate`);
    return data as { email: string; fullName?: string; role: string; isValid: boolean };
  },

  async acceptInvite(body: {
    token: string;
    fullName: string;
    password: string;
    confirmPassword: string;
  }): Promise<void> {
    await apiClient.post('/api/auth/accept-invite', body);
  },

  async verifyTwoFactor(body: { ticket: string; code: string }): Promise<CurrentUser> {
    const { data } = await apiClient.post<LoginResponse>('/api/auth/two-factor/verify', body);
    return applyLoginResponse(data);
  },

  async resendTwoFactorCode(body: { ticket: string }): Promise<void> {
    await apiClient.post('/api/auth/two-factor/resend', body);
  },

  async loadCurrentUser(): Promise<CurrentUser | null> {
    try {
      const { data } = await apiClient.get<CurrentUser>('/api/auth/me');
      setCurrentUserInStorage(data);
      return data;
    } catch {
      return getCurrentUserFromStorage<CurrentUser>();
    }
  },

  async logout(refreshToken: string): Promise<void> {
    try {
      await apiClient.post('/api/auth/logout', { refreshToken });
    } finally {
      clearTokens();
    }
  },

  roleLanding(role?: string): string {
    switch (role) {
      case 'Technician':
        return '/service-orders';
      case 'Inspector':
        return '/inspections';
      case 'Accountant':
        return '/dashboard';
      case 'ReadOnly':
        return '/dashboard';
      default:
        return '/dashboard';
    }
  },
};
