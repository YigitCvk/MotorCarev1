'use client';

import { useRouter } from 'next/navigation';
import { useQueryClient } from '@tanstack/react-query';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { ArrowLeft, Car } from 'lucide-react';
import { z } from 'zod';
import apiClient from '@/core/api/client';
import { friendlyError } from '@/core/api/errors';
import { PageHeader } from '@/components/ui/page-header';
import { customerSchema } from '@/features/customers/components';
import { vehicleSchema } from '@/features/vehicles/components';
import { normalizePlate, vehicleDuplicateMessage } from '@/features/vehicles/hooks';

const vehicleDraftSchema = z.object({
  plate: z.string().optional(),
  brand: z.string().optional(),
  model: z.string().optional(),
  year: z.string().optional(),
  currentKm: z.string().optional(),
  chassisNumber: z.string().optional(),
  engineNumber: z.string().optional(),
  color: z.string().optional(),
});

const customerCreateSchema = customerSchema
  .extend({
    phone: z.string().min(1, 'Telefon zorunludur.'),
    addVehicle: z.boolean(),
    vehicle: vehicleDraftSchema,
  })
  .superRefine((values, ctx) => {
    if (!values.addVehicle) return;

    const vehicleResult = vehicleSchema.safeParse(values.vehicle);
    if (vehicleResult.success) return;

    for (const issue of vehicleResult.error.issues) {
      ctx.addIssue({
        ...issue,
        path: ['vehicle', ...issue.path],
      });
    }
  });

type CustomerCreateFormValues = z.infer<typeof customerCreateSchema>;

function readCreatedId(value: unknown): string | null {
  if (typeof value === 'string' && value.trim()) return value;
  if (typeof value === 'object' && value !== null && 'id' in value) {
    const id = (value as { id?: unknown }).id;
    return typeof id === 'string' && id.trim() ? id : null;
  }
  return null;
}

export default function CustomerCreatePage() {
  const router = useRouter();
  const qc = useQueryClient();

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<CustomerCreateFormValues>({
    resolver: zodResolver(customerCreateSchema),
    defaultValues: {
      fullName: '',
      phone: '',
      email: '',
      whatsapp: '',
      notes: '',
      addVehicle: false,
      vehicle: {
        plate: '',
        brand: '',
        model: '',
        year: '',
        currentKm: '',
        chassisNumber: '',
        engineNumber: '',
        color: '',
      },
    },
  });

  const addVehicle = watch('addVehicle');

  async function onSubmit(values: CustomerCreateFormValues) {
    let customerId: string | null = null;

    try {
      const { data } = await apiClient.post<unknown>('/api/customers', {
        fullName: values.fullName.trim(),
        phone: values.phone.trim(),
        email: values.email?.trim() || undefined,
        whatsapp: values.whatsapp?.trim() || undefined,
        notes: values.notes?.trim() || undefined,
      });

      customerId = readCreatedId(data);
      await qc.invalidateQueries({ queryKey: ['customers'] });
    } catch (err) {
      toast.error(friendlyError(err, 'Müşteri kaydedilemedi.'));
      return;
    }

    if (!customerId) {
      toast.error('Müşteri oluşturuldu ancak detay sayfası açılamadı.');
      router.push('/customers');
      return;
    }

    if (values.addVehicle) {
      const vehicle = vehicleSchema.parse(values.vehicle);

      try {
        await apiClient.post('/api/vehicles', {
          currentCustomerId: customerId,
          plate: vehicle.plate,
          brand: vehicle.brand.trim(),
          model: vehicle.model.trim(),
          year: parseInt(vehicle.year, 10),
          currentKm: vehicle.currentKm ? parseInt(vehicle.currentKm, 10) : undefined,
          chassisNumber: vehicle.chassisNumber?.trim() || undefined,
          engineNumber: vehicle.engineNumber?.trim() || undefined,
          color: vehicle.color?.trim() || undefined,
        });
        await qc.invalidateQueries({ queryKey: ['customer-vehicles', customerId] });
        toast.success('Müşteri ve araç oluşturuldu.');
      } catch (err) {
        toast.warning(
          vehicleDuplicateMessage(err) ??
            'Müşteri oluşturuldu ancak araç kaydedilemedi. Araç bilgilerini müşteri detayından tekrar ekleyebilirsiniz.'
        );
      }
    } else {
      toast.success('Müşteri oluşturuldu.');
    }

    router.push(`/customers/${customerId}`);
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

      <form onSubmit={handleSubmit(onSubmit)} className="card max-w-3xl space-y-6 p-4 sm:p-6">
        <section>
          <h2 className="mb-4 text-base font-semibold text-slate-900">Müşteri Bilgileri</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="form-group sm:col-span-2">
              <label className="label">
                Ad Soyad <span className="text-red-500">*</span>
              </label>
              <input className="input" {...register('fullName')} />
              {errors.fullName && <p className="mt-1 text-xs text-red-500">{errors.fullName.message}</p>}
            </div>
            <div className="form-group">
              <label className="label">
                Telefon <span className="text-red-500">*</span>
              </label>
              <input type="tel" className="input" placeholder="0532 123 4567" {...register('phone')} />
              {errors.phone && <p className="mt-1 text-xs text-red-500">{errors.phone.message}</p>}
            </div>
            <div className="form-group">
              <label className="label">WhatsApp</label>
              <input type="tel" className="input" placeholder="0532 123 4567" {...register('whatsapp')} />
            </div>
            <div className="form-group sm:col-span-2">
              <label className="label">E-posta</label>
              <input type="email" className="input" {...register('email')} />
              {errors.email && <p className="mt-1 text-xs text-red-500">{errors.email.message}</p>}
            </div>
            <div className="form-group sm:col-span-2">
              <label className="label">Notlar</label>
              <textarea className="input" rows={3} {...register('notes')} />
            </div>
          </div>
        </section>

        <section className="rounded-lg border border-slate-200 bg-slate-50 p-4">
          <label className="flex items-start gap-3">
            <input type="checkbox" className="mt-1 h-4 w-4 rounded border-slate-300" {...register('addVehicle')} />
            <span>
              <span className="flex items-center gap-2 text-sm font-semibold text-slate-900">
                <Car size={16} />
                Bu müşteriye araç ekle
              </span>
              <span className="mt-1 block text-sm text-slate-500">
                Plaka, marka, model ve kilometre bilgileri müşteri oluşturulduktan sonra aynı işlemde kaydedilir.
              </span>
            </span>
          </label>

          {addVehicle && (
            <div className="mt-5 grid gap-4 border-t border-slate-200 pt-5 sm:grid-cols-2">
              <div className="form-group">
                <label className="label">
                  Plaka <span className="text-red-500">*</span>
                </label>
                <input
                  className="input font-mono uppercase"
                  placeholder="34ABC123"
                  {...register('vehicle.plate', {
                    onChange: (event) => {
                      event.target.value = normalizePlate(event.target.value);
                    },
                  })}
                />
                {errors.vehicle?.plate && (
                  <p className="mt-1 text-xs text-red-500">{errors.vehicle.plate.message}</p>
                )}
              </div>
              <div className="form-group">
                <label className="label">
                  Yıl <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  min={1900}
                  max={new Date().getFullYear() + 1}
                  className="input"
                  {...register('vehicle.year')}
                />
                {errors.vehicle?.year && (
                  <p className="mt-1 text-xs text-red-500">{errors.vehicle.year.message}</p>
                )}
              </div>
              <div className="form-group">
                <label className="label">
                  Marka <span className="text-red-500">*</span>
                </label>
                <input className="input" placeholder="Honda" {...register('vehicle.brand')} />
                {errors.vehicle?.brand && (
                  <p className="mt-1 text-xs text-red-500">{errors.vehicle.brand.message}</p>
                )}
              </div>
              <div className="form-group">
                <label className="label">
                  Model <span className="text-red-500">*</span>
                </label>
                <input className="input" placeholder="CB500F" {...register('vehicle.model')} />
                {errors.vehicle?.model && (
                  <p className="mt-1 text-xs text-red-500">{errors.vehicle.model.message}</p>
                )}
              </div>
              <div className="form-group">
                <label className="label">Kilometre</label>
                <input type="number" min={0} className="input" {...register('vehicle.currentKm')} />
                {errors.vehicle?.currentKm && (
                  <p className="mt-1 text-xs text-red-500">{errors.vehicle.currentKm.message}</p>
                )}
              </div>
              <div className="form-group">
                <label className="label">Renk</label>
                <input className="input" {...register('vehicle.color')} />
              </div>
              <div className="form-group">
                <label className="label">Şasi No</label>
                <input className="input" {...register('vehicle.chassisNumber')} />
              </div>
              <div className="form-group">
                <label className="label">Motor No</label>
                <input className="input" {...register('vehicle.engineNumber')} />
              </div>
            </div>
          )}
        </section>

        <div className="flex flex-col gap-3 pt-2 sm:flex-row">
          <button type="submit" disabled={isSubmitting} className="btn-primary">
            {isSubmitting ? 'Kaydediliyor...' : 'Kaydet'}
          </button>
          <button type="button" onClick={() => router.back()} className="btn-secondary">
            İptal
          </button>
        </div>
      </form>
    </div>
  );
}
