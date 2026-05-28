'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ArrowLeft, Search } from 'lucide-react';
import apiClient from '@/core/api/client';
import { friendlyError } from '@/core/api/errors';
import { PageHeader } from '@/components/ui/page-header';
import { VehicleForm, type VehicleFormValues } from '@/features/vehicles/components';
import { vehicleDuplicateMessage } from '@/features/vehicles/hooks';
import type { Customer } from '@/features/customers/types';
import type { PagedResult } from '@/shared/types/api.types';
import { normalizePagedResult } from '@/shared/utils/api-normalize';

export default function VehicleCreatePage() {
  const router = useRouter();
  const qc = useQueryClient();
  const [customerInput, setCustomerInput] = useState('');
  const [customerSearch, setCustomerSearch] = useState('');
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);

  const { data: customers, isFetching } = useQuery<PagedResult<Customer>>({
    queryKey: ['customers', customerSearch, 1],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>('/api/customers', {
        params: { q: customerSearch, pageNumber: 1, pageSize: 8 },
      });
      return normalizePagedResult<Customer>(data, 1, 8);
    },
    retry: false,
    enabled: customerSearch.length >= 2,
  });

  async function onSubmit(values: VehicleFormValues) {
    try {
      const { data: vehicleId } = await apiClient.post<string>('/api/vehicles', {
        currentCustomerId: selectedCustomer?.id,
        plate: values.plate,
        brand: values.brand,
        model: values.model,
        year: parseInt(values.year, 10),
        currentKm: values.currentKm ? parseInt(values.currentKm, 10) : undefined,
        chassisNumber: values.chassisNumber || undefined,
        engineNumber: values.engineNumber || undefined,
        color: values.color || undefined,
      });
      await qc.invalidateQueries({ queryKey: ['vehicles'] });
      await qc.invalidateQueries({ queryKey: ['customers'] });
      toast.success('Araç oluşturuldu');
      router.push(vehicleId ? `/vehicles/${vehicleId}` : '/vehicles');
    } catch (err) {
      toast.error(vehicleDuplicateMessage(err) ?? friendlyError(err, 'Araç kaydedilemedi.'));
    }
  }

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.back()} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Geri
        </button>
      </div>
      <PageHeader title="Yeni Araç" subtitle="Araç bilgilerini ve isteğe bağlı müşteri atamasını girin" />

      <div className="card p-4 sm:p-6 max-w-2xl mb-4">
        <label className="label">Müşteri</label>
        <div className="flex flex-col sm:flex-row gap-2">
          <div className="relative flex-1">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              className="input pl-9"
              value={customerInput}
              placeholder="Ad, telefon veya e-posta ara..."
              onChange={(event) => {
                setCustomerInput(event.target.value);
                setSelectedCustomer(null);
              }}
              onKeyDown={(event) => {
                if (event.key === 'Enter') setCustomerSearch(customerInput.trim());
              }}
            />
          </div>
          <button className="btn-secondary" onClick={() => setCustomerSearch(customerInput.trim())}>
            Ara
          </button>
        </div>
        {selectedCustomer && (
          <p className="text-sm text-emerald-700 mt-2">Seçilen müşteri: {selectedCustomer.fullName}</p>
        )}
        {customerSearch.length >= 2 && (
          <div className="mt-3 divide-y rounded-md border border-slate-200 overflow-hidden">
            {isFetching && <p className="p-3 text-sm text-slate-500">Müşteriler aranıyor...</p>}
            {!isFetching && customers?.items?.length === 0 && (
              <p className="p-3 text-sm text-slate-500">Müşteri bulunamadı.</p>
            )}
            {!isFetching &&
              customers?.items?.map((customer) => (
                <button
                  key={customer.id}
                  type="button"
                  className="w-full text-left p-3 hover:bg-slate-50"
                  onClick={() => {
                    setSelectedCustomer(customer);
                    setCustomerInput(customer.fullName);
                  }}
                >
                  <span className="block text-sm font-medium text-slate-900">{customer.fullName}</span>
                  <span className="block text-xs text-slate-500">{customer.phone ?? customer.email ?? 'İletişim bilgisi yok'}</span>
                </button>
              ))}
          </div>
        )}
      </div>

      <div className="card p-4 sm:p-6 max-w-2xl">
        <VehicleForm
          submitLabel="Kaydet"
          submittingLabel="Kaydediliyor..."
          onCancel={() => router.back()}
          onSubmit={onSubmit}
        />
      </div>
    </div>
  );
}
