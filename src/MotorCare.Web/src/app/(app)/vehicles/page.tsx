'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Plus, Search, Car } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import { getVehiclePlate, normalizePlate } from '@/features/vehicles/hooks';
import type { Vehicle } from '@/features/vehicles/types';
import type { PagedResult } from '@/shared/types/api.types';

type VehicleApiResponse = PagedResult<Vehicle> | Vehicle;

export default function VehiclesPage() {
  const router = useRouter();
  const [plate, setPlate] = useState('');
  const [search, setSearch] = useState('');

  const { data, isLoading, error, refetch } = useQuery<VehicleApiResponse>({
    queryKey: ['vehicles', search],
    queryFn: async () => {
      const { data } = await apiClient.get<VehicleApiResponse>(`/api/vehicles/${encodeURIComponent(search)}`);
      return data;
    },
    enabled: !!search,
  });

  const vehicles = data
    ? Array.isArray((data as PagedResult<Vehicle>).items)
      ? (data as PagedResult<Vehicle>).items
      : [data as Vehicle]
    : [];

  function handleSearch() {
    const normalized = normalizePlate(plate);
    if (normalized) setSearch(normalized);
  }

  return (
    <div>
      <PageHeader
        title="Araçlar"
        subtitle="Plakaya göre araç ara"
        actions={
          <Link href="/vehicles/new" className="btn-primary">
            <Plus size={16} />
            Yeni Araç
          </Link>
        }
      />

      <div className="flex flex-col sm:flex-row gap-2 mb-6 w-full sm:max-w-md">
        <div className="relative flex-1">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            className="input pl-9 uppercase font-mono"
            placeholder="Plaka girin (örn: 34ABC123)"
            value={plate}
            onChange={(e) => setPlate(normalizePlate(e.target.value))}
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
        <EmptyState
          title="Plaka ile arama yapın"
          description="Araç detayına ulaşmak için plaka numarasını girin."
          action={{ label: 'Yeni Araç', onClick: () => router.push('/vehicles/new') }}
        />
      )}
      {search && isLoading && <PageLoading />}
      {search && error && <ErrorState message="Araç bulunamadı." onRetry={() => void refetch()} />}
      {search && !isLoading && !error && vehicles.length === 0 && (
        <EmptyState title="Araç bulunamadı" description={`"${search}" plakalı araç sistemde kayıtlı değil.`} />
      )}
      {vehicles.length > 0 && (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {vehicles.map((vehicle) => (
            <Link
              key={vehicle.id}
              href={`/vehicles/${vehicle.id}`}
              className="card p-4 hover:border-brand-300 hover:shadow-md transition-all flex items-start gap-3"
            >
              <div className="h-9 w-9 rounded-lg bg-slate-100 flex items-center justify-center shrink-0">
                <Car size={18} className="text-slate-500" />
              </div>
              <div className="min-w-0">
                <p className="font-semibold text-slate-900 font-mono break-all">{getVehiclePlate(vehicle)}</p>
                <p className="text-sm text-slate-500 truncate">
                  {[vehicle.brand, vehicle.model, vehicle.year].filter(Boolean).join(' ')}
                </p>
                {vehicle.customerName && <p className="text-xs text-slate-400 truncate">{vehicle.customerName}</p>}
                {vehicle.currentKm ? (
                  <p className="text-xs text-slate-400">{vehicle.currentKm.toLocaleString('tr-TR')} km</p>
                ) : null}
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
