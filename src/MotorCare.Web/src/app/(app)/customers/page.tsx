'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Plus, Search, Phone, Mail } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import type { PagedResult } from '@/shared/types/api.types';

interface Customer {
  id: string;
  fullName: string;
  phone?: string;
  email?: string;
  vehicleCount?: number;
}

export default function CustomersPage() {
  const router = useRouter();
  const [q, setQ] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading, error, refetch } = useQuery<PagedResult<Customer>>({
    queryKey: ['customers', search, page],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<Customer>>('/api/customers', {
        params: { q: search, pageNumber: page, pageSize: 20 },
      });
      return data;
    },
  });

  return (
    <div>
      <PageHeader
        title="Müşteriler"
        subtitle={`${data?.totalCount ?? 0} müşteri kayıtlı`}
        actions={
          <button onClick={() => router.push('/customers/create')} className="btn-primary">
            <Plus size={16} /> Yeni Müşteri
          </button>
        }
      />

      {/* Search */}
      <div className="flex gap-2 mb-4">
        <div className="relative flex-1 sm:max-w-sm">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            className="input pl-9"
            placeholder="Ad, telefon veya e-posta ara..."
            value={q}
            onChange={(e) => setQ(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                setSearch(q);
                setPage(1);
              }
            }}
          />
        </div>
        <button
          className="btn-secondary"
          onClick={() => {
            setSearch(q);
            setPage(1);
          }}
        >
          Ara
        </button>
      </div>

      {isLoading && <PageLoading />}
      {error && <ErrorState message="Müşteriler yüklenemedi." onRetry={() => void refetch()} />}

      {!isLoading && !error && (
        <>
          {!data?.items || data.items.length === 0 ? (
            <EmptyState
              title="Müşteri bulunamadı"
              description="Arama kriterlerini değiştirin veya yeni müşteri ekleyin."
              action={{ label: 'Yeni Müşteri', onClick: () => router.push('/customers/create') }}
            />
          ) : (
            <div className="card p-0 overflow-hidden">
              <table className="table">
                <thead>
                  <tr>
                    <th>Ad Soyad</th>
                    <th>Telefon</th>
                    <th>E-posta</th>
                    <th>Araç Sayısı</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((customer) => (
                    <tr
                      key={customer.id}
                      onClick={() => router.push(`/customers/${customer.id}`)}
                      className="cursor-pointer"
                    >
                      <td className="font-medium text-slate-900">{customer.fullName}</td>
                      <td>
                        {customer.phone ? (
                          <span className="flex items-center gap-1 text-slate-600">
                            <Phone size={12} />
                            {customer.phone}
                          </span>
                        ) : (
                          '-'
                        )}
                      </td>
                      <td>
                        {customer.email ? (
                          <span className="flex items-center gap-1 text-slate-500 text-xs">
                            <Mail size={12} />
                            {customer.email}
                          </span>
                        ) : (
                          '-'
                        )}
                      </td>
                      <td className="text-slate-500">{customer.vehicleCount ?? 0} araç</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Pagination */}
          {data && data.totalPages && data.totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-4">
              <button
                disabled={page === 1}
                onClick={() => setPage((p) => p - 1)}
                className="btn-secondary"
              >
                Önceki
              </button>
              <span className="flex items-center text-sm text-slate-600">
                {page} / {data.totalPages}
              </span>
              <button
                disabled={page === data.totalPages}
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
