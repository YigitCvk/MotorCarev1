'use client';

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Search, User, Car, Check } from 'lucide-react';
import { toast } from 'sonner';
import apiClient from '@/core/api/client';
import { friendlyError } from '@/core/api/errors';
import { normalizeApiArray, readNumber, readString } from '@/shared/utils/api-normalize';

const orderSchema = z.object({
  complaint: z.string().min(1, 'Şikayet açıklaması zorunludur'),
  notes: z.string().optional(),
  estimatedCompletionDate: z.string().optional(),
});

type OrderFormValues = z.infer<typeof orderSchema>;

interface CustomerSearchResult {
  id: string;
  fullName: string;
  phone?: string;
}

interface VehicleOption {
  id: string;
  plate: string;
  brand?: string;
  model?: string;
  year?: number;
}

interface CreateOrderBody {
  vehicleId: string;
  customerId: string;
  vehicleKm: number;
  complaint: string | null;
  notes: string | null;
  estimatedCompletionDate: string | null;
}

function normalizeCustomer(value: unknown): CustomerSearchResult | null {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) return null;

  const record = value as Record<string, unknown>;
  const id = readString(record.id);
  const fullName =
    readString(record.fullName) ||
    readString(record.displayName) ||
    readString(record.name) ||
    readString(record.title);

  if (!id || !fullName) return null;

  return {
    id,
    fullName,
    phone: readString(record.phone) || readString(record.phoneNumber) || undefined,
  };
}

function normalizeVehicle(value: unknown): VehicleOption | null {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) return null;

  const record = value as Record<string, unknown>;
  const id = readString(record.id);
  const plate =
    readString(record.plate) || readString(record.plateOriginal) || readString(record.plateNormalized);

  if (!id || !plate) return null;

  return {
    id,
    plate,
    brand: readString(record.brand) || undefined,
    model: readString(record.model) || undefined,
    year: readNumber(record.year),
  };
}

export default function ServiceOrderNewPage(): React.ReactElement {
  const router = useRouter();

  const [customerInput, setCustomerInput] = useState<string>('');
  const [customerSearch, setCustomerSearch] = useState<string>('');
  const [showDropdown, setShowDropdown] = useState<boolean>(false);
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerSearchResult | null>(null);
  const [selectedVehicle, setSelectedVehicle] = useState<VehicleOption | null>(null);
  const [vehicleKm, setVehicleKm] = useState<string>('');
  const [formError, setFormError] = useState<string>('');

  const {
    register,
    handleSubmit: handleRHFSubmit,
    formState: { errors: rhfErrors },
  } = useForm<OrderFormValues>({
    resolver: zodResolver(orderSchema),
    defaultValues: { complaint: '', notes: '', estimatedCompletionDate: '' },
  });

  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      if (customerInput.trim().length >= 2) {
        setCustomerSearch(customerInput.trim());
        setShowDropdown(true);
      } else {
        setCustomerSearch('');
        setShowDropdown(false);
      }
    }, 300);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [customerInput]);

  // Close dropdown on outside click
  useEffect(() => {
    function handleClick(e: MouseEvent): void {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setShowDropdown(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const {
    data: customerResults = [],
    isFetching: searchingCustomers,
    isError: customerSearchError,
  } = useQuery<CustomerSearchResult[]>({
    queryKey: ['customer-search', customerSearch],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>('/api/customers', {
        params: { q: customerSearch, pageSize: 10 },
      });
      return normalizeApiArray(data)
        .map(normalizeCustomer)
        .filter((customer): customer is CustomerSearchResult => Boolean(customer));
    },
    enabled: customerSearch.length >= 2,
    retry: false,
  });

  const {
    data: vehicles = [],
    isFetching: loadingVehicles,
    isError: vehiclesError,
  } = useQuery<VehicleOption[]>({
    queryKey: ['customer-vehicles', selectedCustomer?.id],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>(`/api/customers/${selectedCustomer!.id}/vehicles`);
      return normalizeApiArray(data)
        .map(normalizeVehicle)
        .filter((vehicle): vehicle is VehicleOption => Boolean(vehicle));
    },
    enabled: Boolean(selectedCustomer?.id),
    retry: false,
  });

  const createMutation = useMutation<string, Error, CreateOrderBody>({
    mutationFn: async (body) => {
      const { data } = await apiClient.post<string>('/api/service-orders', body);
      return data;
    },
    onSuccess: (newId) => {
      toast.success('Servis emri oluşturuldu');
      router.push(`/service-orders/${newId}`);
    },
    onError: (err) => {
      const msg = friendlyError(err, 'Servis kaydı oluşturulamadı.');
      setFormError(msg);
      toast.error(msg);
    },
  });

  function selectCustomer(customer: CustomerSearchResult): void {
    setSelectedCustomer(customer);
    setSelectedVehicle(null);
    setCustomerInput(customer.fullName);
    setShowDropdown(false);
  }

  const handleSubmit = handleRHFSubmit((values: OrderFormValues) => {
    setFormError('');

    if (!selectedCustomer) {
      setFormError('Lütfen bir müşteri seçin.');
      return;
    }
    if (!selectedVehicle) {
      setFormError('Lütfen bir araç seçin.');
      return;
    }
    if (!vehicleKm || isNaN(Number(vehicleKm)) || Number(vehicleKm) < 0) {
      setFormError('Geçerli bir KM değeri girin.');
      return;
    }

    createMutation.mutate({
      vehicleId: selectedVehicle.id,
      customerId: selectedCustomer.id,
      vehicleKm: Number(vehicleKm),
      complaint: values.complaint.trim() || null,
      notes: values.notes?.trim() || null,
      estimatedCompletionDate: values.estimatedCompletionDate?.trim() || null,
    });
  });

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.push('/service-orders')} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Servis Kayıtları
        </button>
      </div>

      <div className="page-header">
        <div>
          <h1 className="page-title">Yeni Servis Kaydı</h1>
          <p className="page-subtitle">Müşteri ve araç seçerek servis kaydı oluşturun.</p>
        </div>
      </div>

      {formError && (
        <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {formError}
        </div>
      )}

      <div className="card p-6 max-w-2xl">
        <form onSubmit={handleSubmit} className="space-y-6">

          {/* Customer search */}
          <div className="form-group">
            <label className="label">
              Müşteri <span className="text-red-500">*</span>
            </label>
            <div className="relative" ref={dropdownRef}>
              <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
              <input
                type="text"
                className="input pl-9"
                placeholder="Müşteri adı veya telefon ile arayın..."
                value={customerInput}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                  setCustomerInput(e.target.value);
                  if (selectedCustomer && e.target.value !== selectedCustomer.fullName) {
                    setSelectedCustomer(null);
                    setSelectedVehicle(null);
                  }
                }}
                onFocus={() => {
                  if (customerSearch.length >= 2) setShowDropdown(true);
                }}
                autoComplete="off"
              />
              {showDropdown && (
                <div className="absolute z-20 w-full mt-1 bg-white border border-slate-200 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                  {searchingCustomers && (
                    <div className="px-4 py-3 text-sm text-slate-400">Aranıyor...</div>
                  )}
                  {!searchingCustomers && customerSearchError && (
                    <div className="px-4 py-3 text-sm text-red-600">
                      Müşteri arama şu anda yapılamıyor. Lütfen tekrar deneyin.
                    </div>
                  )}
                  {!searchingCustomers && !customerSearchError && customerResults.length === 0 && (
                    <div className="px-4 py-3 text-sm text-slate-400">Müşteri bulunamadı.</div>
                  )}
                  {!searchingCustomers && !customerSearchError && customerResults.map((c) => (
                    <button
                      key={c.id}
                      type="button"
                      className="w-full flex items-center gap-3 px-4 py-2.5 hover:bg-slate-50 text-left transition-colors"
                      onClick={() => selectCustomer(c)}
                    >
                      <User size={15} className="text-slate-400 shrink-0" />
                      <div>
                        <p className="text-sm font-medium text-slate-900">{c.fullName}</p>
                        {c.phone && <p className="text-xs text-slate-400">{c.phone}</p>}
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </div>
            {selectedCustomer && (
              <p className="mt-1 text-xs text-green-600 flex items-center gap-1">
                <Check size={12} />
                Seçilen: {selectedCustomer.fullName}
              </p>
            )}
          </div>

          {/* Vehicle selection */}
          {selectedCustomer && (
            <div className="form-group">
              <label className="label">
                Araç <span className="text-red-500">*</span>
              </label>
              {loadingVehicles ? (
                <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-500">
                  Araçlar yükleniyor...
                </div>
              ) : vehiclesError ? (
                <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                  Müşteriye bağlı araçlar şu anda yüklenemedi. Lütfen tekrar deneyin.
                </div>
              ) : vehicles.length === 0 ? (
                <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-500">
                  Bu müşteriye ait araç kaydı bulunamadı.
                </div>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  {vehicles.map((vehicle) => {
                    const isSelected = selectedVehicle?.id === vehicle.id;
                    return (
                      <button
                        key={vehicle.id}
                        type="button"
                        onClick={() => setSelectedVehicle(vehicle)}
                        className={[
                          'flex items-start gap-3 rounded-lg border p-3 text-left transition-colors',
                          isSelected
                            ? 'border-blue-500 bg-blue-50'
                            : 'border-slate-200 hover:border-slate-300 hover:bg-slate-50',
                        ].join(' ')}
                      >
                        <Car
                          size={18}
                          className={isSelected ? 'text-blue-500 mt-0.5 shrink-0' : 'text-slate-400 mt-0.5 shrink-0'}
                        />
                        <div>
                          <p className={`font-semibold text-sm ${isSelected ? 'text-blue-700' : 'text-slate-900'}`}>
                            {vehicle.plate}
                          </p>
                          <p className="text-xs text-slate-500">
                            {[vehicle.brand, vehicle.model, vehicle.year].filter(Boolean).join(' ') || '-'}
                          </p>
                        </div>
                        {isSelected && <Check size={14} className="text-blue-500 ml-auto mt-0.5" />}
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
          )}

          {/* KM & complaint */}
          {selectedVehicle && (
            <>
              <div className="form-group">
                <label className="label">
                  Araç KM <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  className="input"
                  placeholder="ör. 85000"
                  min={0}
                  value={vehicleKm}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) => setVehicleKm(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="label">
                  Müşteri Şikayeti <span className="text-red-500">*</span>
                </label>
                <textarea
                  className={`input${rhfErrors.complaint ? ' border-red-400' : ''}`}
                  rows={3}
                  placeholder="Müşterinin belirttiği arıza veya şikayeti yazın..."
                  {...register('complaint')}
                />
                {rhfErrors.complaint && (
                  <p className="mt-1 text-xs text-red-600">{rhfErrors.complaint.message}</p>
                )}
              </div>

              <div className="form-group">
                <label className="label">Notlar</label>
                <textarea
                  className="input"
                  rows={2}
                  placeholder="İç notlar (isteğe bağlı)..."
                  {...register('notes')}
                />
              </div>

              <div className="form-group">
                <label className="label">Tahmini Tamamlanma Tarihi</label>
                <input
                  type="date"
                  className="input"
                  {...register('estimatedCompletionDate')}
                />
              </div>
            </>
          )}

          <div className="flex gap-3 pt-2">
            <button
              type="submit"
              disabled={createMutation.isPending || !selectedCustomer || !selectedVehicle}
              className="btn-primary"
            >
              {createMutation.isPending ? 'Oluşturuluyor...' : 'Servis Kaydı Oluştur'}
            </button>
            <button
              type="button"
              onClick={() => router.push('/service-orders')}
              className="btn-secondary"
            >
              İptal
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
