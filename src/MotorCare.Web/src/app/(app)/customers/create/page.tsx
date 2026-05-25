'use client';

import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { friendlyError } from '@/core/api/errors';

const schema = z.object({
  fullName: z.string().min(2, 'Ad soyad en az 2 karakter olmalıdır.'),
  phone: z.string().optional(),
  email: z.string().email('Geçerli bir e-posta girin.').optional().or(z.literal('')),
  address: z.string().optional(),
  taxNumber: z.string().optional(),
  taxOffice: z.string().optional(),
  notes: z.string().optional(),
});

type CustomerFormValues = z.infer<typeof schema>;

export default function CustomerCreatePage() {
  const router = useRouter();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CustomerFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      fullName: '',
      phone: '',
      email: '',
      address: '',
      taxNumber: '',
      taxOffice: '',
      notes: '',
    },
  });

  async function onSubmit(values: CustomerFormValues) {
    try {
      await apiClient.post('/api/customers', values);
      toast.success('Müşteri oluşturuldu');
      router.push('/customers');
    } catch (err) {
      toast.error(friendlyError(err, 'Müşteri kaydedilemedi.'));
    }
  }

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.back()} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Geri
        </button>
      </div>
      <PageHeader title="Yeni Müşteri" subtitle="Müşteri bilgilerini girin" />

      <div className="card p-6 max-w-2xl">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group">
              <label className="label">
                Ad Soyad <span className="text-red-500">*</span>
              </label>
              <input className="input" {...register('fullName')} />
              {errors.fullName && (
                <p className="text-xs text-red-500 mt-1">{errors.fullName.message}</p>
              )}
            </div>
            <div className="form-group">
              <label className="label">Telefon</label>
              <input type="tel" className="input" placeholder="0532 123 4567" {...register('phone')} />
            </div>
            <div className="form-group">
              <label className="label">E-posta</label>
              <input type="email" className="input" {...register('email')} />
              {errors.email && (
                <p className="text-xs text-red-500 mt-1">{errors.email.message}</p>
              )}
            </div>
            <div className="form-group">
              <label className="label">Vergi No</label>
              <input className="input" {...register('taxNumber')} />
            </div>
            <div className="form-group">
              <label className="label">Vergi Dairesi</label>
              <input className="input" {...register('taxOffice')} />
            </div>
          </div>
          <div className="form-group">
            <label className="label">Adres</label>
            <textarea className="input" rows={2} {...register('address')} />
          </div>
          <div className="form-group">
            <label className="label">Notlar</label>
            <textarea className="input" rows={2} {...register('notes')} />
          </div>
          <div className="flex gap-3 pt-2">
            <button type="submit" disabled={isSubmitting} className="btn-primary">
              {isSubmitting ? 'Kaydediliyor...' : 'Kaydet'}
            </button>
            <button type="button" onClick={() => router.back()} className="btn-secondary">
              İptal
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
