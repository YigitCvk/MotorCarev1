'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { CalendarClock, Check, Search } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import {
  useAppointmentCustomerSearch,
  useAppointmentCustomerVehicles,
} from '@/features/appointments/hooks';
import type {
  AppointmentCustomerOption,
  AppointmentDto,
  AppointmentUpsertRequest,
} from '@/features/appointments/types';
import { APPOINTMENT_TYPE_OPTIONS } from '@/features/appointments/types';

const appointmentSchema = z
  .object({
    customerName: z.string().trim().min(1, 'Müşteri adı zorunludur.').max(150, 'Müşteri adı en fazla 150 karakter olabilir.'),
    phone: z.string().trim().min(1, 'Telefon zorunludur.').max(30, 'Telefon en fazla 30 karakter olabilir.'),
    plate: z.string().trim().max(20, 'Plaka en fazla 20 karakter olabilir.').optional(),
    type: z.enum(['Maintenance', 'Repair', 'Cleaning', 'Washing', 'Inspection', 'TireChange', 'Other']),
    startAt: z.string().min(1, 'Başlangıç zamanı zorunludur.'),
    endAt: z.string().min(1, 'Bitiş zamanı zorunludur.'),
    complaint: z.string().trim().max(1000, 'Şikayet en fazla 1000 karakter olabilir.').optional(),
    note: z.string().trim().max(1000, 'Not en fazla 1000 karakter olabilir.').optional(),
  })
  .refine((value) => new Date(value.endAt).getTime() > new Date(value.startAt).getTime(), {
    path: ['endAt'],
    message: 'Bitiş zamanı başlangıçtan sonra olmalıdır.',
  });

type AppointmentFormValues = z.infer<typeof appointmentSchema>;

interface AppointmentFormProps {
  appointment?: AppointmentDto;
  submitLabel: string;
  isSubmitting?: boolean;
  error?: string;
  onSubmit: (body: AppointmentUpsertRequest) => void;
  onCancel: () => void;
}

function toLocalInputValue(value: string | Date | null | undefined): string {
  if (!value) return '';
  const date = typeof value === 'string' ? new Date(value) : value;
  if (Number.isNaN(date.getTime())) return '';
  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

function defaultStart(): string {
  const date = new Date();
  date.setMinutes(Math.ceil(date.getMinutes() / 15) * 15, 0, 0);
  return toLocalInputValue(date);
}

function defaultEnd(startValue: string): string {
  const date = new Date(startValue);
  date.setHours(date.getHours() + 1);
  return toLocalInputValue(date);
}

function toIso(value: string): string {
  return new Date(value).toISOString();
}

export function AppointmentForm({
  appointment,
  submitLabel,
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: AppointmentFormProps): React.ReactElement {
  const start = appointment ? toLocalInputValue(appointment.startAt) : defaultStart();
  const dropdownRef = useRef<HTMLDivElement>(null);
  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [customerSearch, setCustomerSearch] = useState<string>('');
  const [showCustomerDropdown, setShowCustomerDropdown] = useState<boolean>(false);
  const [selectedCustomer, setSelectedCustomer] = useState<AppointmentCustomerOption | null>(
    appointment?.customerId
      ? {
          id: appointment.customerId,
          fullName: appointment.customerName,
          phone: appointment.phone || undefined,
        }
      : null
  );
  const [selectedVehicleId, setSelectedVehicleId] = useState<string | null>(
    appointment?.vehicleId ?? null
  );

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<AppointmentFormValues>({
    resolver: zodResolver(appointmentSchema),
    defaultValues: {
      customerName: appointment?.customerName ?? '',
      phone: appointment?.phone ?? '',
      plate: appointment?.plate ?? '',
      type: appointment?.type ?? 'Maintenance',
      startAt: start,
      endAt: appointment ? toLocalInputValue(appointment.endAt) : defaultEnd(start),
      complaint: appointment?.complaint ?? '',
      note: appointment?.note ?? '',
    },
  });
  const customerName = watch('customerName');
  const customerNameField = register('customerName');
  const plateField = register('plate');
  const {
    data: customerResults = [],
    isFetching: searchingCustomers,
    isError: customerSearchError,
  } = useAppointmentCustomerSearch(customerSearch);
  const {
    data: customerVehicles = [],
    isFetching: loadingVehicles,
    isError: vehiclesError,
  } = useAppointmentCustomerVehicles(selectedCustomer?.id ?? null);

  useEffect(() => {
    if (!appointment) return;
    reset({
      customerName: appointment.customerName,
      phone: appointment.phone,
      plate: appointment.plate ?? '',
      type: appointment.type,
      startAt: toLocalInputValue(appointment.startAt),
      endAt: toLocalInputValue(appointment.endAt),
      complaint: appointment.complaint ?? '',
      note: appointment.note ?? '',
    });
    setSelectedCustomer(
      appointment.customerId
        ? {
            id: appointment.customerId,
            fullName: appointment.customerName,
            phone: appointment.phone || undefined,
          }
        : null
    );
    setSelectedVehicleId(appointment.vehicleId);
  }, [appointment, reset]);

  useEffect(() => {
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);

    if (selectedCustomer || customerName.trim().length < 2) {
      setCustomerSearch('');
      return;
    }

    searchTimerRef.current = setTimeout(() => {
      setCustomerSearch(customerName.trim());
      setShowCustomerDropdown(true);
    }, 300);

    return () => {
      if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    };
  }, [customerName, selectedCustomer]);

  useEffect(() => {
    function handleOutsideClick(event: MouseEvent): void {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setShowCustomerDropdown(false);
      }
    }

    document.addEventListener('mousedown', handleOutsideClick);
    return () => document.removeEventListener('mousedown', handleOutsideClick);
  }, []);

  function selectCustomer(customer: AppointmentCustomerOption): void {
    setSelectedCustomer(customer);
    setSelectedVehicleId(null);
    setShowCustomerDropdown(false);
    setCustomerSearch('');
    setValue('customerName', customer.fullName, { shouldValidate: true });
    setValue('phone', customer.phone ?? '', { shouldValidate: true });
    setValue('plate', '');
  }

  function clearCustomerLink(): void {
    setSelectedCustomer(null);
    setSelectedVehicleId(null);
    setShowCustomerDropdown(false);
    setCustomerSearch('');
  }

  function selectVehicle(vehicleId: string): void {
    setSelectedVehicleId(vehicleId || null);
    const vehicle = customerVehicles.find((item) => item.id === vehicleId);
    setValue('plate', vehicle?.plate ?? '', { shouldValidate: true });
  }

  const submit = handleSubmit((values) => {
    onSubmit({
      customerId: selectedCustomer?.id ?? null,
      vehicleId: selectedVehicleId,
      customerName: values.customerName.trim(),
      phone: values.phone.trim(),
      plate: values.plate?.trim() || null,
      type: values.type,
      startAt: toIso(values.startAt),
      endAt: toIso(values.endAt),
      note: values.note?.trim() || null,
      complaint: values.complaint?.trim() || null,
    });
  });

  return (
    <div className="card max-w-3xl p-4 sm:p-6">
      {error && (
        <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <form onSubmit={submit} className="space-y-6">
        <div className="flex items-center gap-2 border-b border-slate-100 pb-3">
          <CalendarClock size={18} className="text-brand-600" />
          <h2 className="text-sm font-semibold text-slate-800">Randevu Bilgileri</h2>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="form-group sm:col-span-2">
            <label className="label">Müşteri *</label>
            <div className="relative" ref={dropdownRef}>
              <Search
                size={16}
                className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              />
              <input
                className={`input pl-9${errors.customerName ? ' border-red-400' : ''}`}
                autoComplete="off"
                placeholder="Müşteri adı veya telefon ile arayın..."
                {...customerNameField}
                onChange={(event) => {
                  void customerNameField.onChange(event);
                  if (selectedCustomer && event.target.value !== selectedCustomer.fullName) {
                    setSelectedCustomer(null);
                    setSelectedVehicleId(null);
                    setValue('plate', '');
                  }
                }}
                onFocus={() => {
                  if (!selectedCustomer && customerName.trim().length >= 2) {
                    setShowCustomerDropdown(true);
                  }
                }}
              />
              {showCustomerDropdown && customerSearch.length >= 2 && (
                <div className="absolute z-20 mt-1 max-h-60 w-full overflow-y-auto rounded-lg border border-slate-200 bg-white shadow-lg">
                  {searchingCustomers && (
                    <div className="px-4 py-3 text-sm text-slate-500">Müşteriler aranıyor...</div>
                  )}
                  {!searchingCustomers && customerSearchError && (
                    <div className="px-4 py-3 text-sm text-red-600">
                      Müşteri araması şu anda yapılamıyor. Lütfen tekrar deneyin.
                    </div>
                  )}
                  {!searchingCustomers &&
                    !customerSearchError &&
                    customerResults.length === 0 && (
                      <div className="px-4 py-3 text-sm text-slate-500">Müşteri bulunamadı.</div>
                    )}
                  {!searchingCustomers &&
                    !customerSearchError &&
                    customerResults.map((customer) => (
                      <button
                        key={customer.id}
                        type="button"
                        className="flex w-full items-center gap-3 px-4 py-2.5 text-left transition-colors hover:bg-slate-50"
                        onClick={() => selectCustomer(customer)}
                      >
                        <div className="min-w-0">
                          <p className="truncate text-sm font-medium text-slate-900">
                            {customer.fullName}
                          </p>
                          {customer.phone && (
                            <p className="truncate text-xs text-slate-500">{customer.phone}</p>
                          )}
                        </div>
                      </button>
                    ))}
                </div>
              )}
            </div>
            {errors.customerName && <p className="error-text">{errors.customerName.message}</p>}
            {selectedCustomer && (
              <div className="mt-2 flex flex-wrap items-center gap-2 text-xs">
                <span className="inline-flex items-center gap-1 rounded-full bg-emerald-50 px-2 py-1 font-medium text-emerald-700">
                  <Check size={12} />
                  Kayıtlı müşteri seçildi
                </span>
                <button
                  type="button"
                  className="text-slate-500 hover:text-slate-800"
                  onClick={clearCustomerLink}
                >
                  Müşteri bağlantısını kaldır
                </button>
              </div>
            )}
          </div>

          <div className="form-group">
            <label className="label">Telefon *</label>
            <input className={`input${errors.phone ? ' border-red-400' : ''}`} {...register('phone')} />
            {errors.phone && <p className="error-text">{errors.phone.message}</p>}
          </div>

          <div className="form-group">
            <label className="label">Araç</label>
            {!selectedCustomer ? (
              <>
                <input
                  className={`input uppercase${errors.plate ? ' border-red-400' : ''}`}
                  placeholder="Plaka"
                  {...plateField}
                />
                <p className="mt-1 text-xs text-slate-500">
                  Kayıtlı bir araç seçmek için önce müşteri seçin.
                </p>
              </>
            ) : loadingVehicles ? (
              <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-500">
                Araçlar yükleniyor...
              </div>
            ) : vehiclesError ? (
              <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                Müşteriye bağlı araçlar yüklenemedi. Lütfen tekrar deneyin.
              </div>
            ) : customerVehicles.length === 0 ? (
              <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-500">
                Bu müşteriye bağlı araç bulunmuyor.
              </div>
            ) : (
              <>
                <select
                  className="input"
                  value={selectedVehicleId ?? ''}
                  onChange={(event) => selectVehicle(event.target.value)}
                >
                  <option value="">Araç seçin</option>
                  {selectedVehicleId &&
                    appointment?.vehicleId === selectedVehicleId &&
                    !customerVehicles.some((vehicle) => vehicle.id === selectedVehicleId) && (
                      <option value={selectedVehicleId}>{appointment.plate || 'Mevcut araç'}</option>
                    )}
                  {customerVehicles.map((vehicle) => (
                    <option key={vehicle.id} value={vehicle.id}>
                      {vehicle.plate}
                      {[vehicle.brand, vehicle.model, vehicle.year].filter(Boolean).length > 0
                        ? ` - ${[vehicle.brand, vehicle.model, vehicle.year].filter(Boolean).join(' ')}`
                        : ''}
                    </option>
                  ))}
                </select>
                <p className="mt-1 text-xs text-slate-500">
                  Servis emrine dönüştürmek için müşteri ve araç seçimi gereklidir.
                </p>
              </>
            )}
            {errors.plate && <p className="error-text">{errors.plate.message}</p>}
          </div>

          <div className="form-group">
            <label className="label">Randevu Tipi *</label>
            <select className="input" {...register('type')}>
              {APPOINTMENT_TYPE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label className="label">Başlangıç *</label>
            <input type="datetime-local" className={`input${errors.startAt ? ' border-red-400' : ''}`} {...register('startAt')} />
            {errors.startAt && <p className="error-text">{errors.startAt.message}</p>}
          </div>

          <div className="form-group">
            <label className="label">Bitiş *</label>
            <input type="datetime-local" className={`input${errors.endAt ? ' border-red-400' : ''}`} {...register('endAt')} />
            {errors.endAt && <p className="error-text">{errors.endAt.message}</p>}
          </div>
        </div>

        <div className="grid gap-4">
          <div className="form-group">
            <label className="label">Şikayet</label>
            <textarea rows={3} className={`input${errors.complaint ? ' border-red-400' : ''}`} {...register('complaint')} />
            {errors.complaint && <p className="error-text">{errors.complaint.message}</p>}
          </div>

          <div className="form-group">
            <label className="label">Not</label>
            <textarea rows={3} className={`input${errors.note ? ' border-red-400' : ''}`} {...register('note')} />
            {errors.note && <p className="error-text">{errors.note.message}</p>}
          </div>
        </div>

        <div className="flex flex-col gap-2 pt-2 sm:flex-row">
          <button type="submit" disabled={isSubmitting} className="btn-primary">
            {isSubmitting ? 'Kaydediliyor...' : submitLabel}
          </button>
          <button type="button" onClick={onCancel} className="btn-secondary">
            İptal
          </button>
        </div>
      </form>
    </div>
  );
}
