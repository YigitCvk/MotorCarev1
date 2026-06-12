import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '@/core/api/client';
import type { PagedResult } from '@/shared/types/api.types';
import {
  isRecord,
  normalizeApiArray,
  normalizePagedResult,
  readNumber,
  readString,
} from '@/shared/utils/api-normalize';
import type {
  AppointmentCustomerOption,
  AppointmentDto,
  AppointmentStatus,
  AppointmentType,
  AppointmentUpsertRequest,
  AppointmentVehicleOption,
  ConvertAppointmentResponse,
} from '@/features/appointments/types';
import {
  appointmentStatusFromApi,
  appointmentStatusToApi,
  appointmentTypeFromApi,
  appointmentTypeToApi,
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

function nullableString(value: unknown): string | null {
  const result = readString(value).trim();
  return result || null;
}

function nestedRecord(record: Record<string, unknown>, key: string): Record<string, unknown> {
  const value = record[key];
  return isRecord(value) ? value : {};
}

function normalizeAppointment(value: unknown): AppointmentDto {
  const record = isRecord(value) ? value : {};
  const customer = nestedRecord(record, 'customer');
  const vehicle = nestedRecord(record, 'vehicle');

  return {
    id: readString(record.id),
    customerId: nullableString(record.customerId) ?? nullableString(customer.id),
    vehicleId: nullableString(record.vehicleId) ?? nullableString(vehicle.id),
    customerName:
      readString(record.customerName) ||
      readString(customer.fullName) ||
      readString(customer.displayName) ||
      readString(customer.name),
    phone: readString(record.phone) || readString(customer.phone) || readString(customer.phoneNumber),
    plate:
      nullableString(record.plate) ??
      nullableString(vehicle.plate) ??
      nullableString(vehicle.plateOriginal) ??
      nullableString(vehicle.plateNormalized),
    type: appointmentTypeFromApi(record.type),
    typeText: readString(record.typeText),
    status: appointmentStatusFromApi(record.status),
    statusText: readString(record.statusText),
    startAt: readString(record.startAt),
    endAt: readString(record.endAt),
    note: nullableString(record.note),
    complaint: nullableString(record.complaint),
    serviceOrderId: nullableString(record.serviceOrderId),
  };
}

function normalizeCustomer(value: unknown): AppointmentCustomerOption | null {
  if (!isRecord(value)) return null;

  const id = readString(value.id);
  const fullName =
    readString(value.fullName) ||
    readString(value.displayName) ||
    readString(value.name) ||
    readString(value.title);

  if (!id || !fullName) return null;

  return {
    id,
    fullName,
    phone: readString(value.phone) || readString(value.phoneNumber) || undefined,
  };
}

function normalizeVehicle(value: unknown): AppointmentVehicleOption | null {
  if (!isRecord(value)) return null;

  const id = readString(value.id);
  const plate =
    readString(value.plate) ||
    readString(value.plateOriginal) ||
    readString(value.plateNormalized);

  if (!id || !plate) return null;

  return {
    id,
    plate,
    brand: readString(value.brand) || undefined,
    model: readString(value.model) || undefined,
    year: readNumber(value.year),
  };
}

export function useAppointments(params: AppointmentListParams) {
  return useQuery<PagedResult<AppointmentDto>>({
    queryKey: ['appointments', params],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>('/api/appointments', {
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
      const result = normalizePagedResult<unknown>(
        data,
        params.pageNumber ?? 1,
        params.pageSize ?? 20
      );
      return {
        ...result,
        items: result.items.map(normalizeAppointment),
      };
    },
  });
}

export function useAppointment(id: string) {
  return useQuery<AppointmentDto>({
    queryKey: ['appointment', id],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>(`/api/appointments/${id}`);
      return normalizeAppointment(data);
    },
    enabled: Boolean(id),
  });
}

export function useAppointmentCustomerSearch(search: string) {
  const normalizedSearch = search.trim();

  return useQuery<AppointmentCustomerOption[]>({
    queryKey: ['appointment-customer-search', normalizedSearch],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>('/api/customers', {
        params: { q: normalizedSearch, pageNumber: 1, pageSize: 10 },
      });
      return normalizeApiArray(data)
        .map(normalizeCustomer)
        .filter((customer): customer is AppointmentCustomerOption => Boolean(customer));
    },
    enabled: normalizedSearch.length >= 2,
    retry: false,
  });
}

export function useAppointmentCustomerVehicles(customerId: string | null) {
  return useQuery<AppointmentVehicleOption[]>({
    queryKey: ['appointment-customer-vehicles', customerId],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>(`/api/customers/${customerId}/vehicles`);
      return normalizeApiArray(data)
        .map(normalizeVehicle)
        .filter((vehicle): vehicle is AppointmentVehicleOption => Boolean(vehicle));
    },
    enabled: Boolean(customerId),
    retry: false,
  });
}

export function useCreateAppointment() {
  const qc = useQueryClient();
  return useMutation<AppointmentDto, Error, AppointmentUpsertRequest>({
    mutationFn: async (body) => {
      const { data } = await apiClient.post<unknown>('/api/appointments', {
        ...body,
        type: appointmentTypeToApi(body.type),
      });
      return normalizeAppointment(data);
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
      const { data } = await apiClient.put<unknown>(`/api/appointments/${id}`, {
        ...body,
        type: appointmentTypeToApi(body.type),
      });
      return normalizeAppointment(data);
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
      await apiClient.put(`/api/appointments/${id}/status`, {
        status: appointmentStatusToApi(status),
      });
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
