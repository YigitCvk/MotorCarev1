'use client';

import { use } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { VehicleForm } from '@/features/vehicles/components';
import type { VehicleHistoryApiResponse, VehicleHistoryResponse } from '@/features/vehicles/types';

export default function VehicleEditPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();

  const { data, isLoading, error, refetch } = useQuery<VehicleHistoryApiResponse>({
    queryKey: ['vehicle-history', id],
    queryFn: async () => {
      const { data } = await apiClient.get<VehicleHistoryApiResponse>(`/api/vehicles/${id}/history`);
      return data;
    },
  });

  const vehicle = Array.isArray(data) ? null : (data as VehicleHistoryResponse | undefined);

  function onSubmit() {
    toast.error('Araç düzenleme API desteği henüz hazır değil.');
  }

  if (isLoading) return <PageLoading />;
  if (error || !vehicle) return <ErrorState message="Araç bulunamadı." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.back()} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Geri
        </button>
      </div>
      <PageHeader title="Araç Düzenle" subtitle={vehicle.plate ?? id} />
      <div className="card p-4 sm:p-6 max-w-2xl">
        <VehicleForm
          initialValues={{ plate: vehicle.plate, brand: vehicle.brand, model: vehicle.model, year: vehicle.year, currentKm: vehicle.currentKm }}
          submitLabel="Kaydet"
          submittingLabel="Kaydediliyor..."
          disabled
          onCancel={() => router.back()}
          onSubmit={onSubmit}
        />
        <p className="text-sm text-slate-500 mt-4">Bu sürümde araç bilgisi düzenleme için backend endpoint yok; route hazır, form salt okunur gösteriliyor.</p>
      </div>
    </div>
  );
}
