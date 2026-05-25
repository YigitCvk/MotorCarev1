// src/shared/hooks/use-api.ts
import { useQuery, useMutation, useQueryClient, type UseQueryOptions } from '@tanstack/react-query';
import apiClient from '@/core/api/client';
import type { AxiosError } from 'axios';

export function useGet<T>(
  queryKey: unknown[],
  url: string,
  params?: Record<string, unknown>,
  options?: Omit<UseQueryOptions<T, AxiosError>, 'queryKey' | 'queryFn'>
) {
  return useQuery<T, AxiosError>({
    queryKey: params ? [...queryKey, params] : queryKey,
    queryFn: async () => {
      const { data } = await apiClient.get<T>(url, { params });
      return data;
    },
    ...options,
  });
}

export function usePost<TData, TVariables = TData>(url: string) {
  const qc = useQueryClient();
  return useMutation<TData, AxiosError, TVariables>({
    mutationFn: async (variables) => {
      const { data } = await apiClient.post<TData>(url, variables);
      return data;
    },
    onSuccess: () => qc.invalidateQueries(),
  });
}

export function usePut<TData, TVariables = TData>(url: string) {
  const qc = useQueryClient();
  return useMutation<TData, AxiosError, TVariables>({
    mutationFn: async (variables) => {
      const { data } = await apiClient.put<TData>(url, variables);
      return data;
    },
    onSuccess: () => qc.invalidateQueries(),
  });
}

export function useDelete(url: string) {
  const qc = useQueryClient();
  return useMutation<void, AxiosError, string>({
    mutationFn: async (id) => {
      await apiClient.delete(`${url}/${id}`);
    },
    onSuccess: () => qc.invalidateQueries(),
  });
}
