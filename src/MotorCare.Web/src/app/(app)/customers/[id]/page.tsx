'use client';

import { use, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ArrowLeft, Car, Edit, Plus } from 'lucide-react';
import apiClient from '@/core/api/client';
import { friendlyError } from '@/core/api/errors';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import { VehicleForm, type VehicleFormValues } from '@/features/vehicles/components';
import { getVehiclePlate, vehicleDuplicateMessage } from '@/features/vehicles/hooks';
import type { Customer } from '@/features/customers/types';
import type { Vehicle } from '@/features/vehicles/types';
import { useAuth } from '@/core/auth/auth.context';
import { canCreateVehicle, canEditCustomer } from '@/shared/constants/permissions';

export default function CustomerDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const qc = useQueryClient();
  const { user } = useAuth();
  const canEdit = canEditCustomer(user?.role);
  const canAddVehicle = canCreateVehicle(user?.role);
  const [showVehicleForm, setShowVehicleForm] = useState(false);

  const customerQuery = useQuery<Customer>({
    queryKey: ['customer', id],
    queryFn: async () => {
      const { data } = await apiClient.get<Customer>(`/api/customers/${id}`);
      return data;
    },
  });

  const vehiclesQuery = useQuery<Vehicle[]>({
    queryKey: ['customer-vehicles', id],
    queryFn: async () => {
      const { data } = await apiClient.get<Vehicle[]>(`/api/customers/${id}/vehicles`);
      return data;
    },
  });

  async function onAddVehicle(values: VehicleFormValues) {
    try {
      await apiClient.post('/api/vehicles', {
        currentCustomerId: id,
        plate: values.plate,
        brand: values.brand,
        model: values.model,
        year: parseInt(values.year, 10),
        currentKm: values.currentKm ? parseInt(values.currentKm, 10) : undefined,
        chassisNumber: values.chassisNumber || undefined,
        engineNumber: values.engineNumber || undefined,
        color: values.color || undefined,
      });
      await vehiclesQuery.refetch();
      await qc.invalidateQueries({ queryKey: ['customer-vehicles', id] });
      await qc.invalidateQueries({ queryKey: ['customers'] });
      setShowVehicleForm(false);
      toast.success('Araç müşteriye başarıyla atandı.');
    } catch (err) {
      toast.error(vehicleDuplicateMessage(err) ?? friendlyError(err, 'Araç eklenemedi.'));
    }
  }

  if (customerQuery.isLoading) return <PageLoading />;
  if (customerQuery.error || !customerQuery.data) {
    return <ErrorState message="Müşteri bulunamadı." onRetry={() => void customerQuery.refetch()} />;
  }

  const customer = customerQuery.data;
  const vehicles = vehiclesQuery.data ?? [];

  return (
    <div>
      <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <button onClick={() => router.back()} className="btn-ghost self-start text-sm">
          <ArrowLeft size={14} />
          Müşteriler
        </button>
        {canEdit && (
          <Link href={`/customers/${id}/edit`} className="btn-secondary text-sm">
            <Edit size={14} />
            Düzenle
          </Link>
        )}
      </div>

      <h1 className="mb-6 text-2xl font-bold text-slate-900">{customer.fullName}</h1>

      <div className="card mb-6 max-w-2xl p-4 sm:p-6">
        <div className="grid gap-4 sm:grid-cols-2">
          {[
            { label: 'Ad Soyad', value: customer.fullName },
            { label: 'Telefon', value: customer.phone },
            { label: 'WhatsApp', value: customer.whatsapp },
            { label: 'E-posta', value: customer.email },
            { label: 'Notlar', value: customer.notes },
          ]
            .filter(({ value }) => Boolean(value))
            .map(({ label, value }) => (
              <div key={label} className={label === 'Notlar' ? 'sm:col-span-2' : undefined}>
                <p className="text-xs text-slate-400">{label}</p>
                <p className="break-words text-sm font-medium text-slate-800">{value}</p>
              </div>
            ))}
        </div>
      </div>

      <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-900">Araçlar</h2>
          <p className="text-sm text-slate-500">Toplam {vehicles.length} araç</p>
        </div>
        {canAddVehicle && (
          <button onClick={() => setShowVehicleForm(true)} className="btn-secondary self-start text-sm sm:self-auto">
            <Plus size={14} />
            Yeni Araç Ata
          </button>
        )}
      </div>

      {canAddVehicle && showVehicleForm && (
        <div className="card mb-4 max-w-2xl p-4">
          <VehicleForm
            submitLabel="Yeni Araç Ata"
            submittingLabel="Ekleniyor..."
            onCancel={() => setShowVehicleForm(false)}
            onSubmit={onAddVehicle}
          />
          <Link href={`/customers/${id}/vehicles/new`} className="mt-3 inline-block text-sm text-brand-700 hover:underline">
            Tam sayfada aç
          </Link>
        </div>
      )}

      {vehiclesQuery.isLoading && <PageLoading />}
      {vehiclesQuery.error && (
        <ErrorState message="Araçlar yüklenemedi." onRetry={() => void vehiclesQuery.refetch()} />
      )}
      {!vehiclesQuery.isLoading && !vehiclesQuery.error && vehicles.length === 0 && (
        <EmptyState
          title="Araç kaydı yok"
          description="Bu müşteriye henüz araç eklenmemiş."
          action={canAddVehicle ? { label: 'Yeni Araç Ata', onClick: () => setShowVehicleForm(true) } : undefined}
        />
      )}
      {vehicles.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {vehicles.map((vehicle) => (
            <Link
              key={vehicle.id}
              href={`/vehicles/${vehicle.id}`}
              className="card flex items-start gap-3 p-4 transition-all hover:border-brand-300 hover:shadow-md"
            >
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-slate-100">
                <Car size={18} className="text-slate-500" />
              </div>
              <div className="min-w-0">
                <p className="break-all font-mono font-semibold text-slate-900">{getVehiclePlate(vehicle)}</p>
                <p className="truncate text-sm text-slate-500">
                  {[vehicle.brand, vehicle.model, vehicle.year].filter(Boolean).join(' ')}
                </p>
                {vehicle.currentKm ? (
                  <p className="text-xs text-slate-400">{vehicle.currentKm.toLocaleString('tr-TR')} km</p>
                ) : null}
                {vehicle.vehicleType ? <p className="text-xs text-slate-400">{vehicle.vehicleType}</p> : null}
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
