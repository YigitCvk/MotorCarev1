'use client';

import { useRouter } from 'next/navigation';
import { ArrowLeft } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { friendlyError } from '@/core/api/errors';

const serviceSchema = z.object({
  name: z.string().min(2, 'Hizmet adı zorunludur'),
  category: z.string().min(1, 'Kategori seçimi zorunludur'),
  description: z.string().optional(),
  defaultPrice: z.number().min(0, 'Fiyat 0 veya daha büyük olmalıdır'),
  estimatedDurationMinutes: z.number().int().min(0).optional(),
  currency: z.string().min(1),
  isActive: z.boolean(),
});

type ServiceFormValues = z.infer<typeof serviceSchema>;

const SERVICE_CATEGORIES: Array<{ value: string; label: string }> = [
  { value: 'PeriodicMaintenance', label: 'Periyodik Bakım' },
  { value: 'FaultDiagnosis', label: 'Arıza Teşhis' },
  { value: 'CarWash', label: 'Araç Yıkama' },
  { value: 'Detailing', label: 'Detaylı Temizlik' },
  { value: 'TireService', label: 'Lastik Servisi' },
  { value: 'MotorcycleMaintenance', label: 'Motosiklet Bakım' },
  { value: 'BodyPaint', label: 'Kaporta/Boya' },
  { value: 'Other', label: 'Diğer' },
];

export default function ServiceCatalogCreatePage() {
  const router = useRouter();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ServiceFormValues>({
    resolver: zodResolver(serviceSchema),
    defaultValues: {
      name: '',
      category: '',
      description: '',
      defaultPrice: 0,
      estimatedDurationMinutes: 60,
      currency: 'TRY',
      isActive: true,
    },
  });

  async function onSubmit(values: ServiceFormValues) {
    try {
      const payload = {
        name: values.name.trim(),
        category: values.category,
        description: values.description?.trim() || undefined,
        defaultDurationMinutes: values.estimatedDurationMinutes ?? 60,
        price: values.defaultPrice,
        currency: values.currency.trim() || 'TRY',
        isActive: values.isActive,
      };
      const { data } = await apiClient.post<{ id: string }>('/api/services', payload);
      toast.success('Hizmet kaydedildi');
      router.push(`/service-catalog/${data.id}`);
    } catch (err) {
      toast.error(friendlyError(err, 'Hizmet kaydedilemedi.'));
    }
  }

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.push('/service-catalog')} className="btn btn-ghost text-sm">
          <ArrowLeft size={14} />
          Hizmet Kataloğu
        </button>
      </div>

      <PageHeader title="Yeni Hizmet" subtitle="Hizmet kataloğuna yeni kayıt ekleyin" />

      <div className="card p-6 max-w-2xl">
        <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="space-y-4">
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group sm:col-span-2">
              <label className="label">
                Hizmet Adı <span className="text-red-500">*</span>
              </label>
              <input
                className="input"
                placeholder="Örn: Yağ Değişimi"
                {...register('name')}
              />
              {errors.name && <p className="error-text mt-1">{errors.name.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">
                Kategori <span className="text-red-500">*</span>
              </label>
              <select className="input" {...register('category')}>
                <option value="">Kategori seçin...</option>
                {SERVICE_CATEGORIES.map((c) => (
                  <option key={c.value} value={c.value}>
                    {c.label}
                  </option>
                ))}
              </select>
              {errors.category && <p className="error-text mt-1">{errors.category.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">Varsayılan Süre (dakika)</label>
              <input
                type="number"
                className="input"
                min={0}
                placeholder="60"
                {...register('estimatedDurationMinutes', { valueAsNumber: true })}
              />
              {errors.estimatedDurationMinutes && (
                <p className="error-text mt-1">{errors.estimatedDurationMinutes.message}</p>
              )}
            </div>

            <div className="form-group">
              <label className="label">
                Fiyat <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                className="input"
                min={0}
                step={0.01}
                placeholder="0.00"
                {...register('defaultPrice', { valueAsNumber: true })}
              />
              {errors.defaultPrice && <p className="error-text mt-1">{errors.defaultPrice.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">Para Birimi</label>
              <input
                className="input"
                placeholder="TRY"
                {...register('currency')}
              />
            </div>
          </div>

          <div className="form-group">
            <label className="label">Açıklama</label>
            <textarea
              className="input"
              rows={3}
              placeholder="Hizmet hakkında kısa açıklama..."
              {...register('description')}
            />
          </div>

          <div className="form-group">
            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" className="w-4 h-4" {...register('isActive')} />
              <span className="label mb-0">Aktif</span>
            </label>
          </div>

          <div className="flex gap-3 pt-2">
            <button type="submit" disabled={isSubmitting} className="btn btn-primary">
              {isSubmitting ? 'Kaydediliyor...' : 'Kaydet'}
            </button>
            <button
              type="button"
              onClick={() => router.push('/service-catalog')}
              className="btn btn-secondary"
            >
              İptal
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
