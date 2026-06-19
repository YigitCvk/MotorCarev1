// src/core/api/client.ts
// Runs both server and client.

import axios, { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import { clearTokens, getAccessToken, getClientKey, getRefreshToken, setTokens } from '@/core/auth/storage';
import { appConfig } from '@/shared/config/env';
import type { LoginResponse } from '@/shared/types/api.types';

interface RetryConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

const apiBaseUrl = appConfig.apiBaseUrl.replace(/\/$/, '');
let refreshPromise: Promise<string> | null = null;
const AUTH_REFRESH_EXCLUDED_PATHS = [
  '/api/auth/accept-invite',
  '/api/auth/forgot-password',
  '/api/auth/login',
  '/api/auth/refresh-token',
  '/api/auth/register',
  '/api/auth/resend-email-verification',
  '/api/auth/resend-email-verification-code',
  '/api/auth/reset-password',
  '/api/auth/two-factor/resend',
  '/api/auth/two-factor/verify',
  '/api/auth/verify-email',
  '/api/auth/verify-email-code',
];
const AUTH_REFRESH_EXCLUDED_PREFIXES = ['/api/users/invitations/'];

function resolveBaseUrl(): string {
  return apiBaseUrl;
}

function isAuthRefreshCandidate(config?: RetryConfig): boolean {
  if (!config?.url || !hasAuthorizationHeader(config)) return false;

  const path = requestPath(config.url);
  if (!path) return false;

  return (
    !AUTH_REFRESH_EXCLUDED_PATHS.includes(path) &&
    !AUTH_REFRESH_EXCLUDED_PREFIXES.some((prefix) => path.startsWith(prefix))
  );
}

function requestPath(url: string): string {
  try {
    return (
      new URL(url, 'https://garajpass.local').pathname.toLowerCase().replace(/\/+$/, '') || '/'
    );
  } catch {
    return url.split(/[?#]/, 1)[0]?.toLowerCase().replace(/\/+$/, '') ?? '';
  }
}

function hasAuthorizationHeader(config: InternalAxiosRequestConfig): boolean {
  const headers = config.headers;
  return (
    !!headers.get?.('Authorization') ||
    !!headers.get?.('authorization') ||
    !!headers.Authorization ||
    !!headers.authorization
  );
}

async function refreshAccessToken(): Promise<string> {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      const refreshToken = getRefreshToken();
      if (!refreshToken) throw new Error('Refresh token bulunamadı.');

      const clientKey = getClientKey();

      const response = await axios.post<LoginResponse>(
        `${resolveBaseUrl()}/api/auth/refresh-token`,
        { refreshToken },
        {
          headers: {
            'Content-Type': 'application/json',
            ...(clientKey ? { 'X-MotorCare-Client-Key': clientKey } : {}),
          },
          timeout: 15_000,
        },
      );

      const { accessToken, refreshToken: newRefreshToken } = response.data;
      setTokens(accessToken, newRefreshToken);
      return accessToken;
    })().finally(() => {
      refreshPromise = null;
    });
  }

  return refreshPromise;
}

export const apiClient: AxiosInstance = axios.create({
  baseURL: resolveBaseUrl(),
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000,
});

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (typeof window !== 'undefined') {
    const token = getAccessToken();
    const clientKey = getClientKey();
    if (token) config.headers.Authorization = `Bearer ${token}`;
    if (clientKey) config.headers['X-MotorCare-Client-Key'] = clientKey;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryConfig | undefined;

    if (
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      isAuthRefreshCandidate(originalRequest)
    ) {
      originalRequest._retry = true;

      try {
        const accessToken = await refreshAccessToken();
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        clearTokens();
        if (typeof window !== 'undefined') {
          const next = encodeURIComponent(`${window.location.pathname}${window.location.search}`);
          window.location.replace(`/login?from=${next}`);
        }
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  },
);

export default apiClient;
