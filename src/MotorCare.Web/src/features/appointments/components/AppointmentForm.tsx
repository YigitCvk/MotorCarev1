'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { CalendarClock } from 'lucide-react';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import type { AppointmentDto, AppointmentUpsertRequest } from '@/features/appointments/types';
import { APPOINTMENT_TYPE_OPTIONS } from '@/features/appointments/types';

const appointmentSchema = z
  .object({
    customerName: z.string().trim().min(1, 'Musteri adi zorunludur.').max(150, 'Musteri adi en fazla 150 karakter olabilir.'),
    phone: z.string().trim().min(1, 'Telefon zorunludur.').max(30, 'Telefon en fazla 30 karakter olabilir.'),
    plate: z.string().trim().max(20, 'Plaka en fazla 20 karakter olabilir.').optional(),
    type: z.enum(['Maintenance', 'Repair', 'Cleaning', 'Washing', 'Inspection', 'TireChange', 'Other']),
    startAt: z.string().min(1, 'Baslangic zamani zorunludur.'),
    endAt: z.string().min(1, 'Bitis zamani zorunludur.'),
    complaint: z.string().trim().max(1000, 'Sikayet en fazla 1000 karakter olabilir.').optional(),
    note: z.string().trim().max(1000, 'Not en fazla 1000 karakter olabilir.').optional(),
  })
  .refine((value) => new Date(value.endAt).getTime() > new Date(value.startAt).getTime(), {
    path: ['endAt'],
    message: 'Bitis zamani baslangictan sonra olmalidir.',
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

  const {
    register,
    handleSubmit,
    reset,
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
  }, [appointment, reset]);

  const submit = handleSubmit((values) => {
    onSubmit({
      customerId: appointment?.customerId ?? null,
      vehicleId: appointment?.vehicleId ?? null,
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
          <div className="form-group">
            <label className="label">Musteri *</label>
            <input className={`input${errors.customerName ? ' border-red-400' : ''}`} {...register('customerName')} />
            {errors.customerName && <p className="error-text">{errors.customerName.message}</p>}
          </div>

          <div className="form-group">
            <label className="label">Telefon *</label>
            <input className={`input${errors.phone ? ' border-red-400' : ''}`} {...register('phone')} />
            {errors.phone && <p className="error-text">{errors.phone.message}</p>}
          </div>

          <div className="form-group">
            <label className="label">Plaka</label>
            <input className={`input uppercase${errors.plate ? ' border-red-400' : ''}`} {...register('plate')} />
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
            <label className="label">Baslangic *</label>
            <input type="datetime-local" className={`input${errors.startAt ? ' border-red-400' : ''}`} {...register('startAt')} />
            {errors.startAt && <p className="error-text">{errors.startAt.message}</p>}
          </div>

          <div className="form-group">
            <label className="label">Bitis *</label>
            <input type="datetime-local" className={`input${errors.endAt ? ' border-red-400' : ''}`} {...register('endAt')} />
            {errors.endAt && <p className="error-text">{errors.endAt.message}</p>}
          </div>
        </div>

        <div className="grid gap-4">
          <div className="form-group">
            <label className="label">Sikayet</label>
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
            Iptal
          </button>
        </div>
      </form>
    </div>
  );
}
