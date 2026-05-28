'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import type { Customer } from '@/features/customers/types';

export const customerSchema = z.object({
  fullName: z.string().min(2, 'Ad soyad en az 2 karakter olmalıdır.'),
  phone: z.string().optional(),
  email: z.string().email('Geçerli bir e-posta girin.').optional().or(z.literal('')),
  whatsapp: z.string().optional(),
  notes: z.string().optional(),
});

export type CustomerFormValues = z.infer<typeof customerSchema>;

interface CustomerFormProps {
  initialValues?: Partial<Customer>;
  submitLabel: string;
  submittingLabel: string;
  isSubmitting?: boolean;
  onCancel: () => void;
  onSubmit: (values: CustomerFormValues) => void | Promise<void>;
}

export function CustomerForm({
  initialValues,
  submitLabel,
  submittingLabel,
  isSubmitting,
  onCancel,
  onSubmit,
}: CustomerFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting: formSubmitting },
  } = useForm<CustomerFormValues>({
    resolver: zodResolver(customerSchema),
    defaultValues: {
      fullName: initialValues?.fullName ?? '',
      phone: initialValues?.phone ?? '',
      email: initialValues?.email ?? '',
      whatsapp: initialValues?.whatsapp ?? '',
      notes: initialValues?.notes ?? '',
    },
  });

  const submitting = isSubmitting ?? formSubmitting;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid sm:grid-cols-2 gap-4">
        <div className="form-group sm:col-span-2">
          <label className="label">
            Ad Soyad <span className="text-red-500">*</span>
          </label>
          <input className="input" {...register('fullName')} />
          {errors.fullName && <p className="text-xs text-red-500 mt-1">{errors.fullName.message}</p>}
        </div>
        <div className="form-group">
          <label className="label">Telefon</label>
          <input type="tel" className="input" placeholder="0532 123 4567" {...register('phone')} />
        </div>
        <div className="form-group">
          <label className="label">WhatsApp</label>
          <input type="tel" className="input" placeholder="0532 123 4567" {...register('whatsapp')} />
        </div>
        <div className="form-group sm:col-span-2">
          <label className="label">E-posta</label>
          <input type="email" className="input" {...register('email')} />
          {errors.email && <p className="text-xs text-red-500 mt-1">{errors.email.message}</p>}
        </div>
      </div>
      <div className="form-group">
        <label className="label">Notlar</label>
        <textarea className="input" rows={3} {...register('notes')} />
      </div>
      <div className="flex flex-col sm:flex-row gap-3 pt-2">
        <button type="submit" disabled={submitting} className="btn-primary">
          {submitting ? submittingLabel : submitLabel}
        </button>
        <button type="button" onClick={onCancel} className="btn-secondary">
          İptal
        </button>
      </div>
    </form>
  );
}
