import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '@/core/api/client';
import type { PagedResult } from '@/shared/types/api.types';
import type {
  AppointmentDto,
  AppointmentStatus,
  AppointmentType,
  AppointmentUpsertRequest,
  ConvertAppointmentResponse,
} from '@/features/appointments/types';

export interface AppointmentListParams {
  q?: string;
  status?: string;
  type?: string;
  startFrom?: string;
  endTo?: string;
  pageNumber?: number;
  pageSize?: number;
}

export function useAppointments(params: AppointmentListParams) {
  return useQuery<PagedResult<AppointmentDto>>({
    queryKey: ['appointments', params],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<AppointmentDto>>('/api/appointments', {
        params: {
          q: params.q || undefined,
          status: params.status || undefined,
          type: params.type || undefined,
          startFrom: params.startFrom || undefined,
          endTo: params.endTo || undefined,
          pageNumber: params.pageNumber ?? 1,
          pageSize: params.pageSize ?? 20,
        },
      });
      return data;
    },
  });
}

export function useAppointment(id: string) {
  return useQuery<AppointmentDto>({
    queryKey: ['appointment', id],
    queryFn: async () => {
      const { data } = await apiClient.get<AppointmentDto>(`/api/appointments/${id}`);
      return data;
    },
    enabled: Boolean(id),
  });
}

export function useCreateAppointment() {
  const qc = useQueryClient();
  return useMutation<AppointmentDto, Error, AppointmentUpsertRequest>({
    mutationFn: async (body) => {
      const { data } = await apiClient.post<AppointmentDto>('/api/appointments', body);
      return data;
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['appointments'] });
    },
  });
}

export function useUpdateAppointment(id: string) {
  const qc = useQueryClient();
  return useMutation<AppointmentDto, Error, AppointmentUpsertRequest>({
    mutationFn: async (body) => {
      const { data } = await apiClient.put<AppointmentDto>(`/api/appointments/${id}`, body);
      return data;
    },
    onSuccess: (data) => {
      void qc.invalidateQueries({ queryKey: ['appointments'] });
      void qc.invalidateQueries({ queryKey: ['appointment', data.id] });
    },
  });
}

export function useUpdateAppointmentStatus(id: string) {
  const qc = useQueryClient();
  return useMutation<void, Error, AppointmentStatus>({
    mutationFn: async (status) => {
      await apiClient.put(`/api/appointments/${id}/status`, { status });
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['appointments'] });
      void qc.invalidateQueries({ queryKey: ['appointment', id] });
    },
  });
}

export function useCancelAppointment(id: string) {
  const qc = useQueryClient();
  return useMutation<void, Error, void>({
    mutationFn: async () => {
      await apiClient.delete(`/api/appointments/${id}`);
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['appointments'] });
      void qc.invalidateQueries({ queryKey: ['appointment', id] });
    },
  });
}

export function useConvertAppointment(id: string) {
  const qc = useQueryClient();
  return useMutation<ConvertAppointmentResponse, Error, number>({
    mutationFn: async (vehicleKm) => {
      const { data } = await apiClient.post<ConvertAppointmentResponse>(
        `/api/appointments/${id}/convert-to-service-order`,
        { vehicleKm }
      );
      return data;
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['appointments'] });
      void qc.invalidateQueries({ queryKey: ['appointment', id] });
    },
  });
}

export type { AppointmentType, AppointmentStatus };
