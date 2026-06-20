// src/core/auth/auth.service.ts
import apiClient from '@/core/api/client';
import {
  clearTokens,
  getCurrentUserFromStorage,
  setCurrentUserInStorage,
  setTokens,
} from '@/core/auth/storage';
import { appConfig } from '@/shared/config/env';
import type { ApiProblem, CurrentUser, LoginRequest, LoginResponse, RegisterResponse } from '@/shared/types/api.types';

const AUTH_REDIRECT_ORIGIN = 'https://garajpass.local';
const publicApiBaseUrl = appConfig.apiBaseUrl.replace(/\/$/, '');
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

function normalizeTenantIdentifier(value: string): string {
  return value.trim().toLowerCase();
}

function normalizeEmail(value: string): string {
  return value.trim().toLowerCase();
}

async function publicAuthPost<TResponse>(path: string, body: unknown): Promise<TResponse> {
  const response = await fetch(`${publicApiBaseUrl}${path}`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw await createApiError(response);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as TResponse;
}

async function createApiError(response: Response) {
  const data = await readApiProblem(response);
  return {
    isAxiosError: true,
    message: data?.message ?? data?.title ?? response.statusText,
    response: {
      status: response.status,
      data,
    },
  };
}

async function readApiProblem(response: Response): Promise<(ApiProblem & { title?: string; detail?: string }) | undefined> {
  const text = await response.text();
  if (!text) return undefined;

  try {
    return JSON.parse(text) as ApiProblem & { title?: string; detail?: string };
  } catch {
    return { message: text };
  }
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
    const data = await publicAuthPost<LoginResponse>('/api/auth/login', {
      ...request,
      tenantIdentifier: normalizeTenantIdentifier(request.tenantIdentifier),
      email: normalizeEmail(request.email),
    });
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
  }): Promise<RegisterResponse> {
    const data = await publicAuthPost<RegisterResponse>('/api/auth/register', {
      ...body,
      tenantIdentifier: normalizeTenantIdentifier(body.tenantIdentifier),
      ownerEmail: normalizeEmail(body.ownerEmail),
    });
    return data;
  },

  async verifyEmail(body: { tenantIdentifier: string; email: string; code: string }): Promise<void> {
    await publicAuthPost<void>('/api/auth/verify-email-code', {
      ...body,
      tenantIdentifier: normalizeTenantIdentifier(body.tenantIdentifier),
      email: normalizeEmail(body.email),
    });
  },

  async resendVerificationCode(body: { email: string; tenantIdentifier: string }): Promise<void> {
    await publicAuthPost<void>('/api/auth/resend-email-verification-code', {
      ...body,
      tenantIdentifier: normalizeTenantIdentifier(body.tenantIdentifier),
      email: normalizeEmail(body.email),
    });
  },

  async forgotPassword(body: { email: string; tenantIdentifier: string }): Promise<void> {
    await publicAuthPost<void>('/api/auth/forgot-password', {
      ...body,
      tenantIdentifier: normalizeTenantIdentifier(body.tenantIdentifier),
      email: normalizeEmail(body.email),
    });
  },

  async resetPassword(body: {
    tenantIdentifier: string;
    email: string;
    code: string;
    newPassword: string;
    confirmPassword: string;
  }): Promise<void> {
    await publicAuthPost<void>('/api/auth/reset-password', {
      ...body,
      tenantIdentifier: normalizeTenantIdentifier(body.tenantIdentifier),
      email: normalizeEmail(body.email),
    });
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
    await publicAuthPost<void>('/api/auth/accept-invite', body);
  },

  async verifyTwoFactor(body: { ticket: string; code: string }): Promise<CurrentUser> {
    const data = await publicAuthPost<LoginResponse>('/api/auth/two-factor/verify', body);
    return applyLoginResponse(data);
  },

  async resendTwoFactorCode(body: { ticket: string }): Promise<void> {
    await publicAuthPost<void>('/api/auth/two-factor/resend', body);
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
