'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Plus, Search, ClipboardList } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import { money, dateText } from '@/shared/utils/format';
import type { PagedResult } from '@/shared/types/api.types';

interface MotorcycleInspectionListItemDto {
  id: string;
  inspectionNo: string;
  customerId: string | null;
  vehicleId: string | null;
  customerName: string;
  phone: string;
  plate: string;
  packageType: string;
  packageTypeText: string;
  status: string;
  statusText: string;
  packagePrice: number;
  createdAt: string;
  completedAt: string | null;
}

const STATUS_BADGE: Record<string, string> = {
  Draft: 'badge-gray',
  InProgress: 'badge-yellow',
  Completed: 'badge-green',
  Cancelled: 'badge-red',
};

const PACKAGE_TYPES: Array<{ value: string; label: string }> = [
  { value: '', label: 'Tüm Paketler' },
  { value: 'MechanicalAndRunningGear', label: 'Mekanik ve Yürüyen Aksam' },
  { value: 'BodyAndFairing', label: 'Karenaj ve Kaporta' },
  { value: 'ObdAndElectrical', label: 'OBD Test ve Elektrik/Elektronik' },
  { value: 'Full', label: 'Full Ekspertiz' },
];

const STATUSES: Array<{ value: string; label: string }> = [
  { value: '', label: 'Tüm Durumlar' },
  { value: 'Draft', label: 'Taslak' },
  { value: 'InProgress', label: 'Devam Ediyor' },
  { value: 'Completed', label: 'Tamamlandı' },
  { value: 'Cancelled', label: 'İptal' },
];

export default function InspectionsPage() {
  const router = useRouter();
  const [q, setQ] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [packageType, setPackageType] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading, error, refetch } = useQuery<PagedResult<MotorcycleInspectionListItemDto>>({
    queryKey: ['inspections', search, status, packageType, page],
    queryFn: async () => {
      const params: Record<string, string | number> = { pageNumber: page, pageSize: 20 };
      if (search.trim()) params.q = search.trim();
      if (status) params.status = status;
      if (packageType) params.packageType = packageType;

      const { data } = await apiClient.get<PagedResult<MotorcycleInspectionListItemDto>>(
        '/api/inspections',
        { params }
      );
      return data;
    },
  });

  function handleSearch() {
    setSearch(q);
    setPage(1);
  }

  return (
    <div>
      <PageHeader
        title="Ekspertizler"
        subtitle={`${data?.totalCount ?? 0} kayıt`}
        actions={
          <button onClick={() => router.push('/inspections/new')} className="btn-primary">
            <Plus size={16} /> Yeni Ekspertiz
          </button>
        }
      />

      {/* Filters */}
      <div className="flex flex-wrap gap-2 mb-4">
        <div className="relative flex-1 min-w-[200px] max-w-sm">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            className="input pl-9"
            placeholder="Müşteri, plaka veya ekspertiz no ara..."
            value={q}
            onChange={(e) => setQ(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') handleSearch();
            }}
          />
        </div>
        <select
          className="input w-auto"
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(1);
          }}
        >
          {STATUSES.map((s) => (
            <option key={s.value} value={s.value}>
              {s.label}
            </option>
          ))}
        </select>
        <select
          className="input w-auto"
          value={packageType}
          onChange={(e) => {
            setPackageType(e.target.value);
            setPage(1);
          }}
        >
          {PACKAGE_TYPES.map((p) => (
            <option key={p.value} value={p.value}>
              {p.label}
            </option>
          ))}
        </select>
        <button className="btn" onClick={handleSearch}>
          Ara
        </button>
      </div>

      {isLoading && <PageLoading />}
      {error && <ErrorState message="Ekspertizler yüklenemedi." onRetry={() => void refetch()} />}

      {!isLoading && !error && (
        <>
          {!data?.items || data.items.length === 0 ? (
            <EmptyState
              title="Ekspertiz bulunamadı"
              description="Arama kriterlerini değiştirin veya yeni ekspertiz oluşturun."
              action={{ label: 'Yeni Ekspertiz', onClick: () => router.push('/inspections/new') }}
            />
          ) : (
            <div className="table-container overflow-x-auto">
              <table className="table min-w-[640px]">
                <thead>
                  <tr>
                    <th>Ekspertiz No</th>
                    <th>Müşteri</th>
                    <th>Plaka</th>
                    <th>Paket</th>
                    <th>Durum</th>
                    <th>Ücret</th>
                    <th>Tarih</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((item) => (
                    <tr
                      key={item.id}
                      onClick={() => router.push(`/inspections/${item.id}`)}
                      className="cursor-pointer"
                    >
                      <td className="font-mono text-sm font-medium text-slate-900">
                        <span className="flex items-center gap-1.5">
                          <ClipboardList size={14} className="text-slate-400" />
                          {item.inspectionNo}
                        </span>
                      </td>
                      <td>
                        <div className="font-medium text-slate-900">{item.customerName}</div>
                        <div className="text-xs text-slate-400">{item.phone}</div>
                      </td>
                      <td className="font-mono font-semibold text-slate-800">{item.plate}</td>
                      <td className="text-sm text-slate-600">{item.packageTypeText}</td>
                      <td>
                        <span className={STATUS_BADGE[item.status] ?? 'badge-gray'}>
                          {item.statusText}
                        </span>
                      </td>
                      <td className="text-sm font-medium text-slate-800">
                        {money(item.packagePrice)}
                      </td>
                      <td className="text-sm text-slate-500">{dateText(item.createdAt)}</td>
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
                className="btn"
              >
                Önceki
              </button>
              <span className="flex items-center text-sm text-slate-600">
                {page} / {data.totalPages}
              </span>
              <button
                disabled={page === data.totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="btn"
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
