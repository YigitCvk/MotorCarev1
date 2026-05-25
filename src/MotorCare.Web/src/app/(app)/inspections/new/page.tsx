'use client';

import { useState, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Search, User, Bike } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { friendlyError } from '@/core/api/errors';
import type { PagedResult } from '@/shared/types/api.types';

interface CustomerSearchItem {
  id: string;
  fullName: string;
  phone?: string;
}

interface CustomerVehicle {
  id: string;
  plate: string;
  brand?: string;
  model?: string;
  year?: number;
}

interface CustomerDetail {
  id: string;
  fullName: string;
  phone?: string;
  vehicles: CustomerVehicle[];
}

interface InspectionFormState {
  customerId: string;
  vehicleId: string;
  customerName: string;
  phone: string;
  plate: string;
  brand: string;
  model: string;
  year: string;
  mileage: string;
  chassisNumber: string;
  engineNumber: string;
  query5664: string;
  mileageQuery: string;
  packageType: string;
  generalNotes: string;
  testRideNotes: string;
  cosmeticNotes: string;
}

const PACKAGE_TYPES: Array<{ value: string; label: string }> = [
  { value: 'MechanicalAndRunningGear', label: 'Mekanik ve Yürüyen Aksam' },
  { value: 'BodyAndFairing', label: 'Karenaj ve Kaporta' },
  { value: 'ObdAndElectrical', label: 'OBD Test ve Elektrik/Elektronik' },
  { value: 'Full', label: 'Full Ekspertiz' },
];

const emptyForm: InspectionFormState = {
  customerId: '',
  vehicleId: '',
  customerName: '',
  phone: '',
  plate: '',
  brand: '',
  model: '',
  year: '',
  mileage: '',
  chassisNumber: '',
  engineNumber: '',
  query5664: '',
  mileageQuery: '',
  packageType: 'Full',
  generalNotes: '',
  testRideNotes: '',
  cosmeticNotes: '',
};

export default function InspectionNewPage() {
  const router = useRouter();
  const [form, setForm] = useState<InspectionFormState>(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  // Customer search
  const [customerSearch, setCustomerSearch] = useState('');
  const [customerSearchQuery, setCustomerSearchQuery] = useState('');
  const [showCustomerDropdown, setShowCustomerDropdown] = useState(false);
  const customerSearchRef = useRef<HTMLDivElement>(null);

  const { data: customerResults, isFetching: searchingCustomers } =
    useQuery<PagedResult<CustomerSearchItem>>({
      queryKey: ['customer-search-inspection', customerSearchQuery],
      queryFn: async () => {
        const { data } = await apiClient.get<PagedResult<CustomerSearchItem>>('/api/customers', {
          params: { q: customerSearchQuery, pageSize: 10 },
        });
        return data;
      },
      enabled: customerSearchQuery.length >= 2,
    });

  const { data: selectedCustomerDetail } = useQuery<CustomerDetail>({
    queryKey: ['customer-detail-inspection', form.customerId],
    queryFn: async () => {
      const { data } = await apiClient.get<CustomerDetail>(`/api/customers/${form.customerId}`);
      return data;
    },
    enabled: Boolean(form.customerId),
  });

  function update(field: keyof InspectionFormState, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function handleCustomerSearchChange(value: string) {
    setCustomerSearch(value);
    if (value.length >= 2) {
      setCustomerSearchQuery(value);
      setShowCustomerDropdown(true);
    } else {
      setShowCustomerDropdown(false);
    }
  }

  function selectCustomer(customer: CustomerSearchItem) {
    setCustomerSearch(customer.fullName);
    setShowCustomerDropdown(false);
    setForm((prev) => ({
      ...prev,
      customerId: customer.id,
      vehicleId: '',
      customerName: customer.fullName,
      phone: customer.phone ?? '',
    }));
  }

  function clearCustomer() {
    setCustomerSearch('');
    setCustomerSearchQuery('');
    setShowCustomerDropdown(false);
    setForm((prev) => ({
      ...prev,
      customerId: '',
      vehicleId: '',
      customerName: '',
      phone: '',
    }));
  }

  function selectVehicle(vehicle: CustomerVehicle) {
    setForm((prev) => ({
      ...prev,
      vehicleId: vehicle.id,
      plate: vehicle.plate,
      brand: vehicle.brand ?? '',
      model: vehicle.model ?? '',
      year: vehicle.year ? String(vehicle.year) : '',
    }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!form.customerName.trim()) {
      setError('Müşteri adı zorunludur.');
      return;
    }
    if (!form.plate.trim()) {
      setError('Plaka zorunludur.');
      return;
    }
    if (!form.packageType) {
      setError('Paket tipi seçiniz.');
      return;
    }
    setSaving(true);
    setError('');
    try {
      const body = {
        customerId: form.customerId || undefined,
        vehicleId: form.vehicleId || undefined,
        customerName: form.customerName.trim(),
        phone: form.phone.trim(),
        plate: form.plate.trim().toUpperCase(),
        brand: form.brand.trim() || undefined,
        model: form.model.trim() || undefined,
        year: form.year ? parseInt(form.year, 10) : undefined,
        mileage: form.mileage ? parseInt(form.mileage, 10) : undefined,
        chassisNumber: form.chassisNumber.trim() || undefined,
        engineNumber: form.engineNumber.trim() || undefined,
        query5664: form.query5664.trim() || undefined,
        mileageQuery: form.mileageQuery.trim() || undefined,
        packageType: form.packageType,
        generalNotes: form.generalNotes.trim() || undefined,
        testRideNotes: form.testRideNotes.trim() || undefined,
        cosmeticNotes: form.cosmeticNotes.trim() || undefined,
      };
      const { data } = await apiClient.post<{ id: string; inspectionNo: string }>(
        '/api/inspections',
        body
      );
      router.push(`/inspections/${data.id}`);
    } catch (err) {
      setError(friendlyError(err, 'Ekspertiz oluşturulamadı.'));
    } finally {
      setSaving(false);
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
      <PageHeader title="Yeni Ekspertiz" subtitle="Ekspertiz bilgilerini girin" />

      {error && <div className="alert-error mb-4">{error}</div>}

      <form onSubmit={handleSubmit} className="space-y-6 max-w-3xl">
        {/* Customer lookup */}
        <div className="card p-6">
          <h2 className="text-base font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <User size={16} />
            Müşteri
          </h2>

          <div className="mb-4">
            <label className="label">Müşteri Ara (opsiyonel)</label>
            <div className="relative" ref={customerSearchRef}>
              <Search
                size={16}
                className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
              />
              <input
                type="text"
                className="input pl-9"
                placeholder="İsim veya telefon ile ara..."
                value={customerSearch}
                onChange={(e) => handleCustomerSearchChange(e.target.value)}
                onFocus={() => {
                  if (customerSearch.length >= 2) setShowCustomerDropdown(true);
                }}
                onBlur={() => setTimeout(() => setShowCustomerDropdown(false), 150)}
              />
              {showCustomerDropdown && (
                <div className="absolute z-20 top-full left-0 right-0 mt-1 bg-white border border-slate-200 rounded-lg shadow-lg max-h-52 overflow-y-auto">
                  {searchingCustomers && (
                    <div className="px-4 py-3 text-sm text-slate-400">Aranıyor...</div>
                  )}
                  {!searchingCustomers &&
                    (!customerResults?.items || customerResults.items.length === 0) && (
                      <div className="px-4 py-3 text-sm text-slate-400">Müşteri bulunamadı.</div>
                    )}
                  {customerResults?.items.map((c) => (
                    <button
                      key={c.id}
                      type="button"
                      className="w-full text-left px-4 py-2.5 hover:bg-slate-50 text-sm"
                      onMouseDown={() => selectCustomer(c)}
                    >
                      <div className="font-medium text-slate-900">{c.fullName}</div>
                      {c.phone && <div className="text-xs text-slate-400">{c.phone}</div>}
                    </button>
                  ))}
                </div>
              )}
            </div>
            {form.customerId && (
              <div className="mt-2 flex items-center gap-2">
                <span className="badge-blue text-xs">Bağlı: {form.customerName}</span>
                <button
                  type="button"
                  className="text-xs text-slate-400 hover:text-slate-700"
                  onClick={clearCustomer}
                >
                  Bağlantıyı Kaldır
                </button>
              </div>
            )}
          </div>

          {/* Vehicle selection from linked customer */}
          {form.customerId && selectedCustomerDetail && selectedCustomerDetail.vehicles.length > 0 && (
            <div className="mb-4">
              <label className="label">Araç Seç</label>
              <div className="flex flex-wrap gap-2">
                {selectedCustomerDetail.vehicles.map((v) => (
                  <button
                    key={v.id}
                    type="button"
                    onClick={() => selectVehicle(v)}
                    className={`px-3 py-1.5 rounded-lg border text-sm font-mono transition-colors ${
                      form.vehicleId === v.id
                        ? 'border-brand-500 bg-brand-50 text-brand-700'
                        : 'border-slate-200 hover:border-slate-400 text-slate-700'
                    }`}
                  >
                    {v.plate}
                    {v.brand && (
                      <span className="ml-1 font-sans font-normal text-xs text-slate-400">
                        {v.brand} {v.model}
                      </span>
                    )}
                  </button>
                ))}
              </div>
            </div>
          )}

          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group">
              <label className="label">
                Müşteri Adı <span className="text-red-500">*</span>
              </label>
              <input
                className="input"
                value={form.customerName}
                onChange={(e) => update('customerName', e.target.value)}
                required
              />
            </div>
            <div className="form-group">
              <label className="label">Telefon</label>
              <input
                type="tel"
                className="input"
                value={form.phone}
                onChange={(e) => update('phone', e.target.value)}
                placeholder="0532 123 4567"
              />
            </div>
          </div>
        </div>

        {/* Vehicle info */}
        <div className="card p-6">
          <h2 className="text-base font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <Bike size={16} />
            Motorsiklet Bilgileri
          </h2>
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group">
              <label className="label">
                Plaka <span className="text-red-500">*</span>
              </label>
              <input
                className="input uppercase"
                value={form.plate}
                onChange={(e) => update('plate', e.target.value.toUpperCase())}
                placeholder="34ABC123"
                required
              />
            </div>
            <div className="form-group">
              <label className="label">Marka</label>
              <input
                className="input"
                value={form.brand}
                onChange={(e) => update('brand', e.target.value)}
                placeholder="Honda"
              />
            </div>
            <div className="form-group">
              <label className="label">Model</label>
              <input
                className="input"
                value={form.model}
                onChange={(e) => update('model', e.target.value)}
                placeholder="CB500F"
              />
            </div>
            <div className="form-group">
              <label className="label">Yıl</label>
              <input
                type="number"
                className="input"
                value={form.year}
                onChange={(e) => update('year', e.target.value)}
                min={1950}
                max={2030}
                placeholder="2020"
              />
            </div>
            <div className="form-group">
              <label className="label">Kilometre</label>
              <input
                type="number"
                className="input"
                value={form.mileage}
                onChange={(e) => update('mileage', e.target.value)}
                min={0}
                placeholder="15000"
              />
            </div>
            <div className="form-group">
              <label className="label">Şasi No</label>
              <input
                className="input"
                value={form.chassisNumber}
                onChange={(e) => update('chassisNumber', e.target.value)}
              />
            </div>
            <div className="form-group">
              <label className="label">Motor No</label>
              <input
                className="input"
                value={form.engineNumber}
                onChange={(e) => update('engineNumber', e.target.value)}
              />
            </div>
            <div className="form-group">
              <label className="label">5664 Sorgu</label>
              <input
                className="input"
                value={form.query5664}
                onChange={(e) => update('query5664', e.target.value)}
              />
            </div>
            <div className="form-group">
              <label className="label">KM Sorgu</label>
              <input
                className="input"
                value={form.mileageQuery}
                onChange={(e) => update('mileageQuery', e.target.value)}
              />
            </div>
          </div>
        </div>

        {/* Package & notes */}
        <div className="card p-6">
          <h2 className="text-base font-semibold text-slate-800 mb-4">Ekspertiz Paketi ve Notlar</h2>
          <div className="form-group mb-4">
            <label className="label">
              Paket Tipi <span className="text-red-500">*</span>
            </label>
            <select
              className="input"
              value={form.packageType}
              onChange={(e) => update('packageType', e.target.value)}
              required
            >
              {PACKAGE_TYPES.map((p) => (
                <option key={p.value} value={p.value}>
                  {p.label}
                </option>
              ))}
            </select>
          </div>
          <div className="form-group mb-4">
            <label className="label">Genel Notlar</label>
            <textarea
              className="input"
              rows={3}
              value={form.generalNotes}
              onChange={(e) => update('generalNotes', e.target.value)}
            />
          </div>
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group">
              <label className="label">Test Sürüşü Notları</label>
              <textarea
                className="input"
                rows={3}
                value={form.testRideNotes}
                onChange={(e) => update('testRideNotes', e.target.value)}
              />
            </div>
            <div className="form-group">
              <label className="label">Kozmetik Notlar</label>
              <textarea
                className="input"
                rows={3}
                value={form.cosmeticNotes}
                onChange={(e) => update('cosmeticNotes', e.target.value)}
              />
            </div>
          </div>
        </div>

        <div className="flex gap-3">
          <button type="submit" disabled={saving} className="btn-primary">
            {saving ? 'Oluşturuluyor...' : 'Ekspertiz Oluştur'}
          </button>
          <button type="button" onClick={() => router.back()} className="btn">
            İptal
          </button>
        </div>
      </form>
    </div>
  );
}
