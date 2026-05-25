'use client';

import { use, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import { ArrowLeft, Car, Plus, Edit } from 'lucide-react';
import { useState } from 'react';
import apiClient from '@/core/api/client';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { friendlyError } from '@/core/api/errors';

interface Vehicle {
  id: string;
  plate: string;
  brand?: string;
  model?: string;
  year?: number;
  currentKm?: number;
}

interface CustomerDetail {
  id: string;
  fullName: string;
  phone?: string;
  email?: string;
  address?: string;
  taxNumber?: string;
  taxOffice?: string;
  notes?: string;
  vehicles: Vehicle[];
}

// ---- Edit customer schema ----
const editSchema = z.object({
  fullName: z.string().min(2, 'Ad soyad en az 2 karakter olmalıdır.'),
  phone: z.string().optional(),
  email: z.string().email('Geçerli bir e-posta girin.').optional().or(z.literal('')),
  address: z.string().optional(),
  taxNumber: z.string().optional(),
  taxOffice: z.string().optional(),
  notes: z.string().optional(),
});

type EditFormValues = z.infer<typeof editSchema>;

const EDITABLE_FIELDS: Array<{ label: string; key: keyof EditFormValues; type: string }> = [
  { label: 'Ad Soyad', key: 'fullName', type: 'text' },
  { label: 'Telefon', key: 'phone', type: 'tel' },
  { label: 'E-posta', key: 'email', type: 'email' },
  { label: 'Adres', key: 'address', type: 'text' },
  { label: 'Vergi No', key: 'taxNumber', type: 'text' },
  { label: 'Vergi Dairesi', key: 'taxOffice', type: 'text' },
];

// ---- Add vehicle schema ----
const vehicleSchema = z.object({
  plate: z.string().min(1, 'Plaka zorunludur.'),
  brand: z.string().optional(),
  model: z.string().optional(),
  year: z
    .string()
    .optional()
    .refine(
      (v) => !v || (Number(v) >= 1950 && Number(v) <= 2030),
      'Geçerli bir yıl girin (1950–2030).',
    ),
  currentKm: z
    .string()
    .optional()
    .refine((v) => !v || Number(v) >= 0, 'KM negatif olamaz.'),
});

type VehicleFormValues = z.infer<typeof vehicleSchema>;

export default function CustomerDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const qc = useQueryClient();

  const [editMode, setEditMode] = useState(false);
  const [showVehicleForm, setShowVehicleForm] = useState(false);

  const { data, isLoading, error, refetch } = useQuery<CustomerDetail>({
    queryKey: ['customer', id],
    queryFn: async () => {
      const { data } = await apiClient.get<CustomerDetail>(`/api/customers/${id}`);
      return data;
    },
  });

  // ---- Edit form ----
  const {
    register: registerEdit,
    handleSubmit: handleEditSubmit,
    reset: resetEdit,
    formState: { errors: editErrors, isSubmitting: isSaving },
  } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
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

  // Populate edit form when data loads or edit mode opens
  useEffect(() => {
    if (data && editMode) {
      resetEdit({
        fullName: data.fullName,
        phone: data.phone ?? '',
        email: data.email ?? '',
        address: data.address ?? '',
        taxNumber: data.taxNumber ?? '',
        taxOffice: data.taxOffice ?? '',
        notes: data.notes ?? '',
      });
    }
  }, [data, editMode, resetEdit]);

  async function onSaveCustomer(values: EditFormValues) {
    try {
      await apiClient.put(`/api/customers/${id}`, values);
      void qc.invalidateQueries({ queryKey: ['customer', id] });
      void qc.invalidateQueries({ queryKey: ['customers'] });
      setEditMode(false);
      toast.success('Müşteri güncellendi');
    } catch (err) {
      toast.error(friendlyError(err, 'Kaydedilemedi.'));
    }
  }

  // ---- Vehicle form ----
  const {
    register: registerVehicle,
    handleSubmit: handleVehicleSubmit,
    reset: resetVehicle,
    formState: { errors: vehicleErrors, isSubmitting: isAddingVehicle },
  } = useForm<VehicleFormValues>({
    resolver: zodResolver(vehicleSchema),
    defaultValues: { plate: '', brand: '', model: '', year: '', currentKm: '' },
  });

  async function onAddVehicle(values: VehicleFormValues) {
    try {
      await apiClient.post('/api/vehicles', {
        customerId: id,
        plate: values.plate.toUpperCase(),
        brand: values.brand || undefined,
        model: values.model || undefined,
        year: values.year ? parseInt(values.year, 10) : undefined,
        currentKm: values.currentKm ? parseInt(values.currentKm, 10) : undefined,
      });
      void refetch();
      setShowVehicleForm(false);
      resetVehicle();
      toast.success('Araç eklendi');
    } catch (err) {
      toast.error(friendlyError(err, 'Araç eklenemedi.'));
    }
  }

  if (isLoading) return <PageLoading />;
  if (error || !data) return <ErrorState message="Müşteri bulunamadı." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <button onClick={() => router.back()} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Müşteriler
        </button>
        <button
          onClick={() => {
            if (editMode) {
              setEditMode(false);
            } else {
              setEditMode(true);
            }
          }}
          className="btn-secondary text-sm"
        >
          <Edit size={14} />
          {editMode ? 'İptal' : 'Düzenle'}
        </button>
      </div>

      <h1 className="text-2xl font-bold text-slate-900 mb-6">{data.fullName}</h1>

      {/* Info card */}
      <div className="card p-6 mb-6 max-w-2xl">
        {editMode ? (
          <form onSubmit={handleEditSubmit(onSaveCustomer)} className="space-y-3">
            {EDITABLE_FIELDS.map(({ label, key, type }) => (
              <div key={key} className="form-group">
                <label className="label">{label}</label>
                <input type={type} className="input" {...registerEdit(key)} />
                {editErrors[key] && (
                  <p className="text-xs text-red-500 mt-1">{editErrors[key]?.message}</p>
                )}
              </div>
            ))}
            <div className="flex gap-2 pt-2">
              <button type="submit" disabled={isSaving} className="btn-primary">
                {isSaving ? 'Kaydediliyor...' : 'Kaydet'}
              </button>
              <button
                type="button"
                onClick={() => setEditMode(false)}
                className="btn-secondary"
              >
                İptal
              </button>
            </div>
          </form>
        ) : (
          <div className="grid sm:grid-cols-2 gap-4">
            {(
              [
                { label: 'Ad Soyad', value: data.fullName },
                { label: 'Telefon', value: data.phone },
                { label: 'E-posta', value: data.email },
                { label: 'Adres', value: data.address },
                { label: 'Vergi No', value: data.taxNumber },
                { label: 'Vergi Dairesi', value: data.taxOffice },
              ] as Array<{ label: string; value?: string }>
            )
              .filter(({ value }) => Boolean(value))
              .map(({ label, value }) => (
                <div key={label}>
                  <p className="text-xs text-slate-400">{label}</p>
                  <p className="text-sm font-medium text-slate-800">{value}</p>
                </div>
              ))}
          </div>
        )}
      </div>

      {/* Vehicles */}
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-slate-900">
          Araçlar ({data.vehicles?.length ?? 0})
        </h2>
        <button onClick={() => setShowVehicleForm(true)} className="btn-secondary text-sm">
          <Plus size={14} />
          Araç Ekle
        </button>
      </div>

      {showVehicleForm && (
        <div className="card p-4 mb-4 max-w-lg">
          <form onSubmit={handleVehicleSubmit(onAddVehicle)} className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div className="form-group">
                <label className="label">
                  Plaka <span className="text-red-500">*</span>
                </label>
                <input
                  className="input uppercase"
                  placeholder="34ABC123"
                  {...registerVehicle('plate')}
                />
                {vehicleErrors.plate && (
                  <p className="text-xs text-red-500 mt-1">{vehicleErrors.plate.message}</p>
                )}
              </div>
              <div className="form-group">
                <label className="label">Marka</label>
                <input className="input" placeholder="Toyota" {...registerVehicle('brand')} />
              </div>
              <div className="form-group">
                <label className="label">Model</label>
                <input className="input" placeholder="Corolla" {...registerVehicle('model')} />
              </div>
              <div className="form-group">
                <label className="label">Yıl</label>
                <input
                  type="number"
                  className="input"
                  placeholder="2020"
                  min={1950}
                  max={2030}
                  {...registerVehicle('year')}
                />
                {vehicleErrors.year && (
                  <p className="text-xs text-red-500 mt-1">{vehicleErrors.year.message}</p>
                )}
              </div>
              <div className="form-group col-span-2">
                <label className="label">Güncel KM</label>
                <input
                  type="number"
                  className="input"
                  placeholder="45000"
                  min={0}
                  {...registerVehicle('currentKm')}
                />
                {vehicleErrors.currentKm && (
                  <p className="text-xs text-red-500 mt-1">{vehicleErrors.currentKm.message}</p>
                )}
              </div>
            </div>
            <div className="flex gap-2">
              <button type="submit" disabled={isAddingVehicle} className="btn-primary">
                {isAddingVehicle ? 'Ekleniyor...' : 'Araç Ekle'}
              </button>
              <button
                type="button"
                onClick={() => {
                  setShowVehicleForm(false);
                  resetVehicle();
                }}
                className="btn-secondary"
              >
                İptal
              </button>
            </div>
          </form>
        </div>
      )}

      {!data.vehicles || data.vehicles.length === 0 ? (
        <div className="card p-6 text-center text-sm text-slate-400">
          Bu müşteriye ait araç kaydı yok.
        </div>
      ) : (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {data.vehicles.map((vehicle) => (
            <a
              key={vehicle.id}
              href={`/vehicles/${vehicle.id}/history`}
              className="card p-4 hover:border-brand-300 hover:shadow-md transition-all flex items-start gap-3"
            >
              <div className="h-9 w-9 rounded-lg bg-slate-100 flex items-center justify-center shrink-0">
                <Car size={18} className="text-slate-500" />
              </div>
              <div>
                <p className="font-semibold text-slate-900">{vehicle.plate}</p>
                <p className="text-sm text-slate-500">
                  {[vehicle.brand, vehicle.model, vehicle.year].filter(Boolean).join(' ')}
                </p>
                {vehicle.currentKm && (
                  <p className="text-xs text-slate-400">
                    {vehicle.currentKm.toLocaleString('tr-TR')} km
                  </p>
                )}
              </div>
            </a>
          ))}
        </div>
      )}
    </div>
  );
}
