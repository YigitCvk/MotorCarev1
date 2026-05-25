'use client';

import { use } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft } from 'lucide-react';
import { useRouter } from 'next/navigation';
import apiClient from '@/core/api/client';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import { ServiceOrderStatusBadge } from '@/components/ui/badge';
import { money, dateText } from '@/shared/utils/format';

interface HistoryEntry {
  serviceOrderId: string;
  orderNo: string;
  openedAt: string;
  status: string;
  vehicleKm?: number;
  grandTotal?: number;
}

interface VehicleHistoryResponse {
  plate?: string;
  brand?: string;
  model?: string;
  history?: HistoryEntry[];
}

type ApiResponse = VehicleHistoryResponse | HistoryEntry[];

export default function VehicleHistoryPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();

  const { data, isLoading, error, refetch } = useQuery<ApiResponse>({
    queryKey: ['vehicle-history', id],
    queryFn: async () => {
      const { data } = await apiClient.get<ApiResponse>(`/api/vehicles/${id}/history`);
      return data;
    },
  });

  const isArray = Array.isArray(data);
  const history: HistoryEntry[] = isArray
    ? (data as HistoryEntry[])
    : ((data as VehicleHistoryResponse)?.history ?? []);
  const vehicle: VehicleHistoryResponse | null = isArray
    ? null
    : (data as VehicleHistoryResponse) ?? null;

  if (isLoading) return <PageLoading />;
  if (error) return <ErrorState message="Araç geçmişi yüklenemedi." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.back()} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Geri
        </button>
      </div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900">{vehicle?.plate ?? id}</h1>
        {vehicle && (vehicle.brand || vehicle.model) && (
          <p className="text-slate-500">
            {[vehicle.brand, vehicle.model].filter(Boolean).join(' ')}
          </p>
        )}
      </div>

      {history.length === 0 ? (
        <EmptyState
          title="Servis geçmişi yok"
          description="Bu araç için henüz servis kaydı oluşturulmamış."
        />
      ) : (
        <div className="card p-0 overflow-x-auto">
          <table className="table min-w-[560px]">
            <thead>
              <tr>
                <th>Servis No</th>
                <th>Tarih</th>
                <th>KM</th>
                <th>Durum</th>
                <th className="text-right">Toplam</th>
              </tr>
            </thead>
            <tbody>
              {history.map((entry) => (
                <tr
                  key={entry.serviceOrderId}
                  onClick={() => router.push(`/service-orders/${entry.serviceOrderId}`)}
                  className="cursor-pointer"
                >
                  <td className="font-medium text-brand-700">{entry.orderNo}</td>
                  <td className="text-slate-500">{dateText(entry.openedAt)}</td>
                  <td className="text-slate-500">
                    {entry.vehicleKm
                      ? entry.vehicleKm.toLocaleString('tr-TR') + ' km'
                      : '-'}
                  </td>
                  <td>
                    <ServiceOrderStatusBadge status={entry.status} />
                  </td>
                  <td className="text-right font-medium">{money(entry.grandTotal)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
