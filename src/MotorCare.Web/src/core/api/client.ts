// src/core/api/client.ts
// runs both server and client

import axios, { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import { clearTokens, getAccessToken, getClientKey, getRefreshToken, setTokens } from '@/core/auth/storage';
import type { LoginResponse } from '@/shared/types/api.types';

interface RetryConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

type RefreshQueue = {
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}[];

let isRefreshing = false;
let failedQueue: RefreshQueue = [];

function processQueue(error: unknown, token: string | null = null): void {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else if (token) resolve(token);
  });
  failedQueue = [];
}

export const apiClient: AxiosInstance = axios.create({
  baseURL: typeof window !== 'undefined' ? window.location.origin : '',
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
    const isAuthEndpoint =
      originalRequest?.url?.includes('/api/auth/refresh-token') ||
      originalRequest?.url?.includes('/api/auth/login') ||
      originalRequest?.url?.includes('/api/auth/register');

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !isAuthEndpoint) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${token}`;
          }
          return apiClient(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = getRefreshToken();
      if (!refreshToken) {
        clearTokens();
        if (typeof window !== 'undefined') window.location.href = '/login';
        isRefreshing = false;
        return Promise.reject(error);
      }

      try {
        const response = await axios.post<LoginResponse>('/api/auth/refresh-token', { refreshToken });
        const { accessToken, refreshToken: newRefreshToken } = response.data;
        setTokens(accessToken, newRefreshToken);
        processQueue(null, accessToken);
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }
        return apiClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError);
        clearTokens();
        if (typeof window !== 'undefined') window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
