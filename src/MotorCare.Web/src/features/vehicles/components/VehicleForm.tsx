'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { normalizePlate } from '@/features/vehicles/hooks';
import type { Vehicle } from '@/features/vehicles/types';

export const vehicleSchema = z.object({
  plate: z.string().min(1, 'Plaka zorunludur.').transform(normalizePlate),
  brand: z.string().min(1, 'Marka zorunludur.'),
  model: z.string().min(1, 'Model zorunludur.'),
  year: z
    .string()
    .min(1, 'Yıl zorunludur.')
    .refine((v) => Number(v) >= 1900 && Number(v) <= new Date().getFullYear() + 1, 'Geçerli bir yıl girin.'),
  currentKm: z.string().optional().refine((v) => !v || Number(v) >= 0, 'KM negatif olamaz.'),
  chassisNumber: z.string().optional(),
  engineNumber: z.string().optional(),
  color: z.string().optional(),
});

export type VehicleFormValues = z.infer<typeof vehicleSchema>;

interface VehicleFormProps {
  initialValues?: Partial<Vehicle>;
  submitLabel: string;
  submittingLabel: string;
  disabled?: boolean;
  isSubmitting?: boolean;
  onCancel: () => void;
  onSubmit: (values: VehicleFormValues) => void | Promise<void>;
}

export function VehicleForm({
  initialValues,
  submitLabel,
  submittingLabel,
  disabled,
  isSubmitting,
  onCancel,
  onSubmit,
}: VehicleFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting: formSubmitting },
  } = useForm<VehicleFormValues>({
    resolver: zodResolver(vehicleSchema),
    defaultValues: {
      plate: initialValues?.plate ?? initialValues?.plateOriginal ?? '',
      brand: initialValues?.brand ?? '',
      model: initialValues?.model ?? '',
      year: initialValues?.year ? String(initialValues.year) : '',
      currentKm: initialValues?.currentKm ? String(initialValues.currentKm) : '',
      chassisNumber: initialValues?.chassisNumber ?? '',
      engineNumber: initialValues?.engineNumber ?? '',
      color: initialValues?.color ?? '',
    },
  });

  const submitting = isSubmitting ?? formSubmitting;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid sm:grid-cols-2 gap-4">
        <div className="form-group">
          <label className="label">
            Plaka <span className="text-red-500">*</span>
          </label>
          <input
            className="input uppercase font-mono"
            placeholder="34ABC123"
            disabled={disabled}
            {...register('plate', {
              onChange: (event) => {
                event.target.value = normalizePlate(event.target.value);
              },
            })}
          />
          {errors.plate && <p className="text-xs text-red-500 mt-1">{errors.plate.message}</p>}
        </div>
        <div className="form-group">
          <label className="label">
            Yıl <span className="text-red-500">*</span>
          </label>
          <input type="number" className="input" min={1900} max={new Date().getFullYear() + 1} disabled={disabled} {...register('year')} />
          {errors.year && <p className="text-xs text-red-500 mt-1">{errors.year.message}</p>}
        </div>
        <div className="form-group">
          <label className="label">
            Marka <span className="text-red-500">*</span>
          </label>
          <input className="input" placeholder="Toyota" disabled={disabled} {...register('brand')} />
          {errors.brand && <p className="text-xs text-red-500 mt-1">{errors.brand.message}</p>}
        </div>
        <div className="form-group">
          <label className="label">
            Model <span className="text-red-500">*</span>
          </label>
          <input className="input" placeholder="Corolla" disabled={disabled} {...register('model')} />
          {errors.model && <p className="text-xs text-red-500 mt-1">{errors.model.message}</p>}
        </div>
        <div className="form-group">
          <label className="label">Güncel KM</label>
          <input type="number" className="input" min={0} disabled={disabled} {...register('currentKm')} />
          {errors.currentKm && <p className="text-xs text-red-500 mt-1">{errors.currentKm.message}</p>}
        </div>
        <div className="form-group">
          <label className="label">Renk</label>
          <input className="input" disabled={disabled} {...register('color')} />
        </div>
        <div className="form-group">
          <label className="label">Şasi No</label>
          <input className="input" disabled={disabled} {...register('chassisNumber')} />
        </div>
        <div className="form-group">
          <label className="label">Motor No</label>
          <input className="input" disabled={disabled} {...register('engineNumber')} />
        </div>
      </div>
      <div className="flex flex-col sm:flex-row gap-3 pt-2">
        <button type="submit" disabled={disabled || submitting} className="btn-primary">
          {submitting ? submittingLabel : submitLabel}
        </button>
        <button type="button" onClick={onCancel} className="btn-secondary">
          İptal
        </button>
      </div>
    </form>
  );
}
