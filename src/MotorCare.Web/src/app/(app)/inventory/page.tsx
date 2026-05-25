'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { Plus, Search, Package, AlertTriangle, CheckCircle, XCircle } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { EmptyState } from '@/components/ui/empty-state';
import { money } from '@/shared/utils/format';
import type { PagedResult } from '@/shared/types/api.types';

interface InventoryItemDto {
  id: string;
  name: string;
  sku: string | null;
  barcode: string | null;
  category: string | null;
  brand: string | null;
  unit: string;
  unitPrice: number;
  stockQuantity: number;
  minimumStockLevel: number;
  isLowStock: boolean;
  isActive: boolean;
}

export default function InventoryPage() {
  const router = useRouter();
  const [q, setQ] = useState('');
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');
  const [activeFilter, setActiveFilter] = useState('');
  const [lowStockOnly, setLowStockOnly] = useState(false);
  const [page, setPage] = useState(1);

  const { data, isLoading, error, refetch } = useQuery<PagedResult<InventoryItemDto>>({
    queryKey: ['inventory', search, category, activeFilter, lowStockOnly, page],
    queryFn: async () => {
      const params: Record<string, string | number | boolean> = {
        pageNumber: page,
        pageSize: 20,
        lowStockOnly,
      };
      if (search) params.q = search;
      if (category) params.category = category;
      if (activeFilter !== '') params.isActive = activeFilter === 'true';
      const { data } = await apiClient.get<PagedResult<InventoryItemDto>>('/api/inventory', { params });
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
        title="Stok Yönetimi"
        subtitle={`${data?.totalCount ?? 0} ürün kayıtlı`}
        actions={
          <button onClick={() => router.push('/inventory/create')} className="btn btn-primary">
            <Plus size={16} /> Yeni Ürün
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
            placeholder="Ürün adı, SKU veya barkod ara..."
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
        <input
          type="text"
          className="input w-40"
          placeholder="Kategori filtrele..."
          value={category}
          onChange={(e) => {
            setCategory(e.target.value);
            setPage(1);
          }}
        />
        <select
          className="input w-36"
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
        <label className="flex items-center gap-2 text-sm text-slate-700 cursor-pointer select-none">
          <input
            type="checkbox"
            checked={lowStockOnly}
            onChange={(e) => {
              setLowStockOnly(e.target.checked);
              setPage(1);
            }}
          />
          <AlertTriangle size={14} className="text-yellow-500" />
          Düşük Stok
        </label>
      </div>

      {isLoading && <PageLoading />}
      {error && <ErrorState message="Stok verileri yüklenemedi." onRetry={() => void refetch()} />}

      {!isLoading && !error && (
        <>
          {!data?.items || data.items.length === 0 ? (
            <EmptyState
              title="Ürün bulunamadı"
              description="Arama kriterlerini değiştirin veya yeni ürün ekleyin."
              action={{ label: 'Yeni Ürün', onClick: () => router.push('/inventory/create') }}
            />
          ) : (
            <div className="overflow-x-auto rounded-lg border border-slate-200">
              <table className="table min-w-[700px] w-full">
                <thead>
                  <tr>
                    <th>Ürün Adı</th>
                    <th>Kategori</th>
                    <th>Marka</th>
                    <th>Birim</th>
                    <th>Birim Fiyat</th>
                    <th>Stok</th>
                    <th>Min. Stok</th>
                    <th>Durum</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((item) => (
                    <tr
                      key={item.id}
                      onClick={() => router.push(`/inventory/${item.id}`)}
                      className={`cursor-pointer${item.isLowStock ? ' bg-yellow-50 hover:bg-yellow-100' : ''}`}
                    >
                      <td>
                        <div className="flex items-center gap-2">
                          <Package size={14} className="text-slate-400 shrink-0" />
                          <div>
                            <p className="font-medium text-slate-900">{item.name}</p>
                            {item.sku && (
                              <p className="text-xs text-slate-400">SKU: {item.sku}</p>
                            )}
                          </div>
                        </div>
                      </td>
                      <td className="text-slate-600">{item.category ?? '-'}</td>
                      <td className="text-slate-600">{item.brand ?? '-'}</td>
                      <td className="text-slate-600">{item.unit}</td>
                      <td className="text-slate-800 font-medium">{money(item.unitPrice)}</td>
                      <td>
                        {item.isLowStock ? (
                          <span className="inline-flex items-center gap-1 rounded-full bg-yellow-100 px-2 py-0.5 text-xs font-semibold text-yellow-800 border border-yellow-300">
                            <AlertTriangle size={11} className="shrink-0" />
                            {item.stockQuantity}
                          </span>
                        ) : (
                          <span className="text-slate-800">{item.stockQuantity}</span>
                        )}
                      </td>
                      <td className="text-slate-500">{item.minimumStockLevel}</td>
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
