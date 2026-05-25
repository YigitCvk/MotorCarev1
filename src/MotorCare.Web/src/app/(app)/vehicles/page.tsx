'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Search, Car } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import type { PagedResult } from '@/shared/types/api.types';

interface Vehicle {
  id: string;
  plate: string;
  brand?: string;
  model?: string;
  year?: number;
  currentKm?: number;
  customerName?: string;
  customerId?: string;
}

type VehicleApiResponse = PagedResult<Vehicle> | Vehicle;

export default function VehiclesPage() {
  const router = useRouter();
  const [plate, setPlate] = useState('');
  const [search, setSearch] = useState('');

  const { data, isLoading, error, refetch } = useQuery<VehicleApiResponse>({
    queryKey: ['vehicles', search],
    queryFn: async () => {
      const { data } = await apiClient.get<VehicleApiResponse>(
        `/api/vehicles/${encodeURIComponent(search)}`,
      );
      return data;
    },
    enabled: !!search,
  });

  function getVehicles(): Vehicle[] {
    if (!data) return [];
    const paged = data as PagedResult<Vehicle>;
    if (Array.isArray(paged.items)) return paged.items;
    return [data as Vehicle];
  }

  const vehicles = getVehicles();

  function handleSearch() {
    if (plate.trim()) setSearch(plate.trim());
  }

  return (
    <div>
      <PageHeader title="Araçlar" subtitle="Plakaya göre araç ara" />

      <div className="flex gap-2 mb-6 w-full sm:max-w-sm">
        <div className="relative flex-1">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            className="input pl-9"
            placeholder="Plaka girin (örn: 34ABC123)"
            value={plate}
            onChange={(e) => setPlate(e.target.value.toUpperCase())}
            onKeyDown={(e) => {
              if (e.key === 'Enter') handleSearch();
            }}
          />
        </div>
        <button className="btn-primary" onClick={handleSearch} disabled={isLoading}>
          {isLoading ? 'Aranıyor...' : 'Ara'}
        </button>
      </div>

      {!search && (
        <div className="card p-8 text-center">
          <Car size={40} className="text-slate-200 mx-auto mb-3" />
          <p className="text-slate-500 text-sm">Araç aramak için plaka numarası girin.</p>
        </div>
      )}
      {search && isLoading && <PageLoading />}
      {search && error && (
        <ErrorState message="Araç bulunamadı." onRetry={() => void refetch()} />
      )}
      {search && !isLoading && !error && vehicles.length === 0 && (
        <EmptyState
          title="Araç bulunamadı"
          description={`"${search}" plakalı araç sistemde kayıtlı değil.`}
        />
      )}
      {vehicles.length > 0 && (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {vehicles.map((v) => (
            <a
              key={v.id}
              href={`/vehicles/${v.id}/history`}
              className="card p-4 hover:border-brand-300 hover:shadow-md transition-all flex items-start gap-3"
            >
              <div className="h-9 w-9 rounded-lg bg-slate-100 flex items-center justify-center shrink-0">
                <Car size={18} className="text-slate-500" />
              </div>
              <div>
                <p className="font-semibold text-slate-900">{v.plate}</p>
                <p className="text-sm text-slate-500">
                  {[v.brand, v.model, v.year].filter(Boolean).join(' ')}
                </p>
                {v.customerName && (
                  <p className="text-xs text-slate-400">{v.customerName}</p>
                )}
                {v.currentKm && (
                  <p className="text-xs text-slate-400">
                    {v.currentKm.toLocaleString('tr-TR')} km
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
