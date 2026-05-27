'use client';

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Plus, Search, ClipboardList } from 'lucide-react';
import Link from 'next/link';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import { money, dateText } from '@/shared/utils/format';
import type { PagedResult } from '@/shared/types/api.types';

interface ServiceOrderSummaryDto {
  id: string;
  orderNo: string;
  customerName: string | null;
  vehiclePlate: string | null;
  vehicleDisplay: string | null;
  status: string;
  openedAt: string;
  grandTotal: number;
}

const STATUS_OPTIONS: Array<{ value: string; label: string }> = [
  { value: '', label: 'Tüm Durumlar' },
  { value: 'Open', label: 'Açık' },
  { value: 'InProgress', label: 'Devam Ediyor' },
  { value: 'WaitingForParts', label: 'Parça Bekleniyor' },
  { value: 'Completed', label: 'Tamamlandı' },
  { value: 'Cancelled', label: 'İptal' },
];

function statusBadge(status: string): React.ReactElement {
  switch (status) {
    case 'Open':
      return <span className="badge badge-blue">Açık</span>;
    case 'InProgress':
      return <span className="badge badge-yellow">Devam Ediyor</span>;
    case 'WaitingForParts':
      return (
        <span className="badge" style={{ backgroundColor: '#fff7ed', color: '#ea580c', borderColor: '#fed7aa' }}>
          Parça Bekleniyor
        </span>
      );
    case 'Completed':
      return <span className="badge badge-green">Tamamlandı</span>;
    case 'Cancelled':
      return <span className="badge badge-red">İptal</span>;
    default:
      return <span className="badge badge-gray">{status}</span>;
  }
}

export default function ServiceOrdersPage(): React.ReactElement {
  const router = useRouter();
  const [inputValue, setInputValue] = useState<string>('');
  const [search, setSearch] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [page, setPage] = useState<number>(1);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      setSearch(inputValue);
      setPage(1);
    }, 300);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [inputValue]);

  const { data, isLoading, error, refetch } = useQuery<PagedResult<ServiceOrderSummaryDto>>({
    queryKey: ['service-orders', search, statusFilter, page],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<ServiceOrderSummaryDto>>('/api/service-orders', {
        params: { q: search, status: statusFilter || undefined, pageNumber: page, pageSize: 20 },
      });
      return data;
    },
  });

  const totalPages: number = data?.totalPages ?? Math.ceil((data?.totalCount ?? 0) / 20);

  return (
    <div>
      <PageHeader
        title="Servis Kayıtları"
        subtitle={data ? `${data.totalCount} kayıt` : ''}
        actions={
          <Link href="/service-orders/new" className="btn-primary">
            <Plus size={16} />
            Yeni Servis Kaydı
          </Link>
        }
      />

      {/* Filters */}
      <div className="flex flex-wrap gap-2 mb-4">
        <div className="relative flex-1 min-w-[200px] max-w-sm">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            className="input pl-9"
            placeholder="Sipariş no, müşteri veya plaka ara..."
            value={inputValue}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setInputValue(e.target.value)}
          />
        </div>
        <select
          className="input w-auto"
          value={statusFilter}
          onChange={(e: React.ChangeEvent<HTMLSelectElement>) => {
            setStatusFilter(e.target.value);
            setPage(1);
          }}
        >
          {STATUS_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <PageLoading />}
      {error && <ErrorState message="Servis kayıtları yüklenemedi." onRetry={() => void refetch()} />}

      {!isLoading && !error && (
        <>
          {!data?.items || data.items.length === 0 ? (
            (search || statusFilter) ? (
              <EmptyState
                title="Servis kaydı bulunamadı"
                description="Arama kriterlerini değiştirin veya yeni servis kaydı oluşturun."
                action={{ label: 'Yeni Servis Kaydı', onClick: () => router.push('/service-orders/new') }}
              />
            ) : (
              <div className="text-center py-16 text-slate-400">
                <ClipboardList size={48} className="mx-auto mb-3 opacity-30" />
                <p className="text-sm">Henüz servis kaydı yok.</p>
                <button
                  onClick={() => router.push('/service-orders/new')}
                  className="mt-4 btn-primary text-sm"
                >
                  <Plus size={14} />
                  Yeni Servis Kaydı Oluştur
                </button>
              </div>
            )
          ) : (
            <div className="card p-0 overflow-hidden">
              <div className="overflow-x-auto">
              <table className="table">
                <thead>
                  <tr>
                    <th>Sipariş No</th>
                    <th>Müşteri</th>
                    <th>Araç</th>
                    <th>Durum</th>
                    <th>Tarih</th>
                    <th className="text-right">Toplam</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((order) => (
                    <tr
                      key={order.id}
                      onClick={() => router.push(`/service-orders/${order.id}`)}
                      className="cursor-pointer"
                    >
                      <td>
                        <span className="font-mono font-medium text-slate-900">{order.orderNo}</span>
                      </td>
                      <td className="font-medium text-slate-800 max-w-[160px] truncate">{order.customerName ?? '-'}</td>
                      <td>
                        <div className="flex flex-col max-w-[140px]">
                          <span className="font-medium text-slate-900 truncate">{order.vehiclePlate ?? '-'}</span>
                          {order.vehicleDisplay && (
                            <span className="text-xs text-slate-400 truncate">{order.vehicleDisplay}</span>
                          )}
                        </div>
                      </td>
                      <td>{statusBadge(order.status)}</td>
                      <td className="text-slate-500 text-sm whitespace-nowrap">{dateText(order.openedAt)}</td>
                      <td className="text-right font-medium text-slate-900 whitespace-nowrap">{money(order.grandTotal)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              </div>
            </div>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex justify-center items-center gap-3 mt-4">
              <button
                disabled={page === 1}
                onClick={() => setPage((p) => p - 1)}
                className="btn-secondary"
              >
                Önceki
              </button>
              <span className="text-sm text-slate-600">
                {page} / {totalPages}
              </span>
              <button
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="btn-secondary"
              >
                Sonraki
              </button>
            </div>
          )}
        </>
      )}

    </div>
  );
}
