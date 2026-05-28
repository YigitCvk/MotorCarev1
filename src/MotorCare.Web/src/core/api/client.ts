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

function resolveBaseUrl(): string {
  return apiBaseUrl;
}

function isAuthRefreshCandidate(url?: string): boolean {
  return !!url && !url.includes('/api/auth/refresh-token') && !url.includes('/api/auth/login');
}

async function refreshAccessToken(): Promise<string> {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      const refreshToken = getRefreshToken();
      if (!refreshToken) throw new Error('Refresh token bulunamadı.');

      const response = await axios.post<LoginResponse>(
        `${resolveBaseUrl()}/api/auth/refresh-token`,
        { refreshToken },
        { headers: { 'Content-Type': 'application/json' }, timeout: 15_000 },
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
      isAuthRefreshCandidate(originalRequest.url)
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
