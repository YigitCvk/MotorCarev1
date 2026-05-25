'use client';

import { use, useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import { ArrowLeft, Wrench, CheckCircle, XCircle } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { friendlyError } from '@/core/api/errors';
import { money } from '@/shared/utils/format';

interface ServiceCatalogItemDto {
  id: string;
  name: string;
  category: string;
  categoryText: string;
  description: string | null;
  defaultDurationMinutes: number;
  defaultPrice: number;
  price: number;
  currency: string;
  isActive: boolean;
}

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

export default function ServiceCatalogDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const qc = useQueryClient();

  const [toggling, setToggling] = useState(false);

  const { data, isLoading, error, refetch } = useQuery<ServiceCatalogItemDto>({
    queryKey: ['service-item', id],
    queryFn: async () => {
      const { data } = await apiClient.get<ServiceCatalogItemDto>(`/api/services/${id}`);
      return data;
    },
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting: saving },
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

  useEffect(() => {
    if (data) {
      reset({
        name: data.name,
        category: data.category,
        description: data.description ?? '',
        defaultPrice: data.price,
        estimatedDurationMinutes: data.defaultDurationMinutes,
        currency: data.currency,
        isActive: data.isActive,
      });
    }
  }, [data, reset]);

  async function onSave(values: ServiceFormValues) {
    try {
      await apiClient.put(`/api/services/${id}`, {
        name: values.name.trim(),
        category: values.category,
        description: values.description?.trim() || undefined,
        defaultDurationMinutes: values.estimatedDurationMinutes ?? 60,
        price: values.defaultPrice,
        currency: values.currency.trim() || 'TRY',
        isActive: values.isActive,
      });
      void qc.invalidateQueries({ queryKey: ['service-item', id] });
      void qc.invalidateQueries({ queryKey: ['services'] });
      toast.success('Hizmet kaydedildi');
    } catch (err) {
      toast.error(friendlyError(err, 'Hizmet kaydedilemedi.'));
    }
  }

  async function handleToggleActive() {
    if (!data) return;
    setToggling(true);
    try {
      const endpoint = data.isActive
        ? `/api/services/${id}/deactivate`
        : `/api/services/${id}/activate`;
      await apiClient.put(endpoint);
      void qc.invalidateQueries({ queryKey: ['service-item', id] });
      void qc.invalidateQueries({ queryKey: ['services'] });
      toast.success(data.isActive ? 'Hizmet pasif yapıldı' : 'Hizmet aktif yapıldı');
    } catch (err) {
      toast.error(friendlyError(err, 'Durum değiştirilemedi.'));
    } finally {
      setToggling(false);
    }
  }

  if (isLoading) return <PageLoading />;
  if (error || !data) return <ErrorState message="Hizmet bulunamadı." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.push('/service-catalog')} className="btn btn-ghost text-sm">
          <ArrowLeft size={14} />
          Hizmet Kataloğu
        </button>
      </div>

      <div className="flex items-start justify-between gap-4 flex-wrap mb-6">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-lg bg-slate-100 flex items-center justify-center shrink-0">
            <Wrench size={20} className="text-slate-500" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{data.name}</h1>
            <p className="text-sm text-slate-500">{data.categoryText}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => void handleToggleActive()}
            disabled={toggling}
            className={`btn ${data.isActive ? 'btn-danger' : 'btn-primary'}`}
          >
            {data.isActive ? (
              <>
                <XCircle size={14} /> Pasif Yap
              </>
            ) : (
              <>
                <CheckCircle size={14} /> Aktif Yap
              </>
            )}
          </button>
        </div>
      </div>

      {/* Summary cards */}
      <div className="grid sm:grid-cols-3 gap-4 mb-6">
        <div className="card p-4 text-center">
          <p className="text-xs text-slate-500 mb-1">Fiyat</p>
          <p className="text-3xl font-bold text-slate-900">{money(data.price)}</p>
          <p className="text-sm text-slate-500 mt-1">{data.currency}</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-slate-500 mb-1">Süre</p>
          <p className="text-3xl font-bold text-slate-700">{data.defaultDurationMinutes}</p>
          <p className="text-sm text-slate-500 mt-1">dakika</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-slate-500 mb-1">Durum</p>
          <div className="flex justify-center mt-2">
            {data.isActive ? (
              <span className="badge badge-green text-sm px-3 py-1">
                <CheckCircle size={13} className="inline mr-1" />
                Aktif
              </span>
            ) : (
              <span className="badge badge-gray text-sm px-3 py-1">
                <XCircle size={13} className="inline mr-1" />
                Pasif
              </span>
            )}
          </div>
        </div>
      </div>

      {/* Edit form */}
      <div className="card p-6 max-w-2xl">
        <h2 className="text-base font-semibold text-slate-800 mb-4">Hizmet Bilgileri</h2>
        <form onSubmit={(e) => void handleSubmit(onSave)(e)} className="space-y-4">
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group sm:col-span-2">
              <label className="label">
                Hizmet Adı <span className="text-red-500">*</span>
              </label>
              <input className="input" {...register('name')} />
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
                {...register('defaultPrice', { valueAsNumber: true })}
              />
              {errors.defaultPrice && <p className="error-text mt-1">{errors.defaultPrice.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">Para Birimi</label>
              <input className="input" {...register('currency')} />
            </div>
          </div>

          <div className="form-group">
            <label className="label">Açıklama</label>
            <textarea
              className="input"
              rows={3}
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
            <button type="submit" disabled={saving} className="btn btn-primary">
              {saving ? 'Kaydediliyor...' : 'Kaydet'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
