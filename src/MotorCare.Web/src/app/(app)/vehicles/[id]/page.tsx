'use client';

import { use } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Edit } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import { ServiceOrderStatusBadge } from '@/components/ui/badge';
import { money, dateText } from '@/shared/utils/format';
import type { VehicleHistoryApiResponse, VehicleHistoryResponse, VehicleHistoryEntry } from '@/features/vehicles/types';

export default function VehicleDetailPage({ params }: { params: Promise<{ id: string }> }) {
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
  const history: VehicleHistoryEntry[] = Array.isArray(data) ? data : vehicle?.serviceOrders ?? vehicle?.history ?? [];

  if (isLoading) return <PageLoading />;
  if (error) return <ErrorState message="Araç yüklenemedi." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <button onClick={() => router.back()} className="btn-ghost text-sm self-start">
          <ArrowLeft size={14} />
          Geri
        </button>
        <Link href={`/vehicles/${id}/edit`} className="btn-secondary text-sm">
          <Edit size={14} />
          Düzenle
        </Link>
      </div>

      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900 font-mono break-all">{vehicle?.plate ?? id}</h1>
        <p className="text-slate-500">{[vehicle?.brand, vehicle?.model, vehicle?.year].filter(Boolean).join(' ')}</p>
      </div>

      <div className="grid sm:grid-cols-3 gap-4 mb-6">
        <div className="card p-4">
          <p className="text-xs text-slate-400">Güncel KM</p>
          <p className="text-lg font-semibold text-slate-900">{vehicle?.currentKm ? `${vehicle.currentKm.toLocaleString('tr-TR')} km` : '-'}</p>
        </div>
        <div className="card p-4">
          <p className="text-xs text-slate-400">Servis Sayısı</p>
          <p className="text-lg font-semibold text-slate-900">{vehicle?.totalServiceOrderCount ?? history.length}</p>
        </div>
        <div className="card p-4">
          <p className="text-xs text-slate-400">Toplam Harcama</p>
          <p className="text-lg font-semibold text-slate-900">{money(vehicle?.totalSpent)}</p>
        </div>
      </div>

      {history.length === 0 ? (
        <EmptyState title="Servis geçmişi yok" description="Bu araç için henüz servis kaydı oluşturulmamış." />
      ) : (
        <div className="card p-0 overflow-x-auto">
          <table className="table min-w-[640px]">
            <thead>
              <tr>
                <th>Servis No</th>
                <th>Tarih</th>
                <th>Durum</th>
                <th className="text-right">Toplam</th>
              </tr>
            </thead>
            <tbody>
              {history.map((entry) => {
                const serviceOrderId = entry.serviceOrderId ?? entry.id;
                return (
                  <tr key={serviceOrderId ?? entry.orderNo} onClick={() => serviceOrderId && router.push(`/service-orders/${serviceOrderId}`)} className="cursor-pointer">
                    <td className="font-medium text-brand-700">{entry.orderNo}</td>
                    <td className="text-slate-500">{dateText(entry.openedAt)}</td>
                    <td><ServiceOrderStatusBadge status={entry.status} /></td>
                    <td className="text-right font-medium">{money(entry.grandTotal)}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
