'use client';

import { use } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import apiClient from '@/core/api/client';
import { friendlyError } from '@/core/api/errors';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { VehicleForm, type VehicleFormValues } from '@/features/vehicles/components';
import { vehicleDuplicateMessage } from '@/features/vehicles/hooks';
import type { Customer } from '@/features/customers/types';

export default function CustomerVehicleCreatePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const qc = useQueryClient();

  const { data: customer, isLoading, error, refetch } = useQuery<Customer>({
    queryKey: ['customer', id],
    queryFn: async () => {
      const { data } = await apiClient.get<Customer>(`/api/customers/${id}`);
      return data;
    },
  });

  async function onSubmit(values: VehicleFormValues) {
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
      await qc.invalidateQueries({ queryKey: ['customer-vehicles', id] });
      await qc.invalidateQueries({ queryKey: ['customers'] });
      toast.success('Araç müşteriye başarıyla atandı.');
      router.push(`/customers/${id}`);
    } catch (err) {
      toast.error(vehicleDuplicateMessage(err) ?? friendlyError(err, 'Araç eklenemedi.'));
    }
  }

  if (isLoading) return <PageLoading />;
  if (error || !customer) return <ErrorState message="Müşteri bulunamadı." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.back()} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Geri
        </button>
      </div>
      <PageHeader title="Yeni Araç Ata" subtitle={customer.fullName} />
      <div className="card max-w-2xl p-4 sm:p-6">
        <VehicleForm
          submitLabel="Yeni Araç Ata"
          submittingLabel="Ekleniyor..."
          onCancel={() => router.back()}
          onSubmit={onSubmit}
        />
      </div>
    </div>
  );
}
