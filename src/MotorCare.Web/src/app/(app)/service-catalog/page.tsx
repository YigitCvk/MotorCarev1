'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Plus, Search, Wrench, CheckCircle, XCircle } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import { money } from '@/shared/utils/format';
import type { PagedResult } from '@/shared/types/api.types';
import { useAuth } from '@/core/auth/auth.context';
import { canManageServiceCatalog } from '@/shared/constants/permissions';

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

export default function ServiceCatalogPage() {
  const router = useRouter();
  const { user } = useAuth();
  const canManage = canManageServiceCatalog(user?.role);
  const [q, setQ] = useState('');
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');
  const [activeFilter, setActiveFilter] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading, error, refetch } = useQuery<PagedResult<ServiceCatalogItemDto>>({
    queryKey: ['services', search, category, activeFilter, page],
    queryFn: async () => {
      const params: Record<string, string | number | boolean> = {
        pageNumber: page,
        pageSize: 20,
      };
      if (search) params.q = search;
      if (category) params.category = category;
      if (activeFilter !== '') params.isActive = activeFilter === 'true';
      const { data } = await apiClient.get<PagedResult<ServiceCatalogItemDto>>('/api/services', { params });
      return data;
    },
  });

  function applySearch() {
    setSearch(q);
    setPage(1);
  }

  return (
    <div>
      <PageHeader
        title="Hizmet Kataloğu"
        subtitle={`${data?.totalCount ?? 0} hizmet kayıtlı`}
        actions={canManage ? (
          <button onClick={() => router.push('/service-catalog/create')} className="btn btn-primary">
            <Plus size={16} /> Yeni Hizmet
          </button>
        ) : undefined}
      />

      {/* Filters — flex-wrap ensures they stack neatly on mobile */}
      <div className="flex flex-wrap gap-2 mb-4">
        <div className="relative flex-1 min-w-[180px] max-w-sm">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            className="input pl-9 w-full"
            placeholder="Hizmet adı ara..."
            value={q}
            onChange={(e) => setQ(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') applySearch();
            }}
          />
        </div>
        <button className="btn btn-secondary" onClick={applySearch}>
          Ara
        </button>
        <select
          className="input w-full sm:w-48"
          value={category}
          onChange={(e) => {
            setCategory(e.target.value);
            setPage(1);
          }}
        >
          <option value="">Tüm Kategoriler</option>
          {SERVICE_CATEGORIES.map((c) => (
            <option key={c.value} value={c.value}>
              {c.label}
            </option>
          ))}
        </select>
        <select
          className="input w-full sm:w-36"
          value={activeFilter}
          onChange={(e) => {
            setActiveFilter(e.target.value);
            setPage(1);
          }}
        >
          <option value="">Hepsi</option>
          <option value="true">Aktif</option>
          <option value="false">Pasif</option>
        </select>
      </div>

      {isLoading && <PageLoading />}
      {error && <ErrorState message="Hizmetler yüklenemedi." onRetry={() => void refetch()} />}

      {!isLoading && !error && (
        <>
          {!data?.items || data.items.length === 0 ? (
            <EmptyState
              title="Hizmet bulunamadı"
              description="Arama kriterlerini değiştirin veya yeni hizmet ekleyin."
              action={canManage ? { label: 'Yeni Hizmet', onClick: () => router.push('/service-catalog/create') } : undefined}
            />
          ) : (
            <div className="overflow-x-auto rounded-lg border border-slate-200">
              <table className="table min-w-[500px] w-full">
                <thead>
                  <tr>
                    <th>Hizmet Adı</th>
                    <th>Kategori</th>
                    <th>Süre</th>
                    <th>Fiyat</th>
                    <th>Durum</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((item) => (
                    <tr
                      key={item.id}
                      onClick={() => router.push(`/service-catalog/${item.id}`)}
                      className="cursor-pointer"
                    >
                      <td>
                        <div className="flex items-center gap-2">
                          <Wrench size={14} className="text-slate-400 shrink-0" />
                          <div>
                            <p className="font-medium text-slate-900">{item.name}</p>
                            {item.description && (
                              <p className="text-xs text-slate-400 truncate max-w-xs">
                                {item.description}
                              </p>
                            )}
                          </div>
                        </div>
                      </td>
                      <td>
                        <span className="badge badge-gray">{item.categoryText}</span>
                      </td>
                      <td className="text-slate-600 whitespace-nowrap">{item.defaultDurationMinutes} dk</td>
                      <td className="font-medium text-slate-800 whitespace-nowrap">{money(item.price)}</td>
                      <td>
                        {item.isActive ? (
                          <span className="badge badge-green">
                            <CheckCircle size={11} className="inline mr-1" />
                            Aktif
                          </span>
                        ) : (
                          <span className="badge badge-gray">
                            <XCircle size={11} className="inline mr-1" />
                            Pasif
                          </span>
                        )}
                      </td>
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
                className="btn btn-secondary"
              >
                Önceki
              </button>
              <span className="flex items-center text-sm text-slate-600">
                {page} / {data.totalPages}
              </span>
              <button
                disabled={page === data.totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="btn btn-secondary"
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
