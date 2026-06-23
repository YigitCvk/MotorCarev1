'use client';

import { use, useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Bike, User } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { friendlyError } from '@/core/api/errors';
import {
  inspectionPackageTypeFromApi,
  inspectionPackageTypeToApi,
  inspectionStatusFromApi,
} from '@/features/inspections/api-enums';
import { MotorcycleInspectionDiagram } from '@/features/inspections/components';

interface InspectionFormState {
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

interface MotorcycleInspectionDto {
  id: string;
  inspectionNo: string;
  customerName: string;
  phone: string;
  plate: string;
  brand: string | null;
  model: string | null;
  year: number | null;
  mileage: number | null;
  chassisNumber: string | null;
  engineNumber: string | null;
  query5664: string | null;
  mileageQuery: string | null;
  packageType: string;
  generalNotes: string | null;
  testRideNotes: string | null;
  cosmeticNotes: string | null;
  customerId: string | null;
  vehicleId: string | null;
  status: string;
  motorcycleType?: string | null;
}

const PACKAGE_TYPES: Array<{ value: string; label: string }> = [
  { value: 'MechanicalAndRunningGear', label: 'Mekanik ve Yürüyen Aksam' },
  { value: 'BodyAndFairing', label: 'Karenaj ve Kaporta' },
  { value: 'ObdAndElectrical', label: 'OBD Test ve Elektrik/Elektronik' },
  { value: 'Full', label: 'Full Ekspertiz' },
];

export default function InspectionEditPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const router = useRouter();
  const [form, setForm] = useState<InspectionFormState | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const { data, isLoading } = useQuery<MotorcycleInspectionDto>({
    queryKey: ['inspection', id],
    queryFn: async () => {
      const { data } = await apiClient.get<MotorcycleInspectionDto>(`/api/inspections/${id}`);
      return {
        ...data,
        packageType: inspectionPackageTypeFromApi(data.packageType),
        status: inspectionStatusFromApi(data.status),
      };
    },
  });

  useEffect(() => {
    if (data && !form) {
      setForm({
        customerName: data.customerName ?? '',
        phone: data.phone ?? '',
        plate: data.plate ?? '',
        brand: data.brand ?? '',
        model: data.model ?? '',
        year: data.year ? String(data.year) : '',
        mileage: data.mileage ? String(data.mileage) : '',
        chassisNumber: data.chassisNumber ?? '',
        engineNumber: data.engineNumber ?? '',
        query5664: data.query5664 ?? '',
        mileageQuery: data.mileageQuery ?? '',
        packageType: data.packageType ?? 'Full',
        generalNotes: data.generalNotes ?? '',
        testRideNotes: data.testRideNotes ?? '',
        cosmeticNotes: data.cosmeticNotes ?? '',
      });
    }
  }, [data, form]);

  function update(field: keyof InspectionFormState, value: string) {
    setForm((prev) => prev ? { ...prev, [field]: value } : prev);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!form) return;
    if (!form.customerName.trim()) { setError('Müşteri adı zorunludur.'); return; }
    if (!form.plate.trim()) { setError('Plaka zorunludur.'); return; }
    if (!form.packageType) { setError('Paket tipi seçiniz.'); return; }

    setSaving(true);
    setError('');
    try {
      await apiClient.put(`/api/inspections/${id}`, {
        customerId: data?.customerId ?? undefined,
        vehicleId: data?.vehicleId ?? undefined,
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
        packageType: inspectionPackageTypeToApi(form.packageType),
        generalNotes: form.generalNotes.trim() || undefined,
        testRideNotes: form.testRideNotes.trim() || undefined,
        cosmeticNotes: form.cosmeticNotes.trim() || undefined,
        motorcycleType: data?.motorcycleType ?? undefined,
      });
      router.push(`/inspections/${id}`);
    } catch (err) {
      setError(friendlyError(err, 'Ekspertiz güncellenemedi.'));
    } finally {
      setSaving(false);
    }
  }

  if (isLoading || !form) return <PageLoading />;

  const editable = data?.status === 'Draft' || data?.status === 'InProgress';
  if (!editable) {
    return (
      <div>
        <button onClick={() => router.back()} className="btn-ghost text-sm mb-4">
          <ArrowLeft size={14} />
          Geri
        </button>
        <div className="alert-error">Tamamlanmış veya iptal edilmiş ekspertizler düzenlenemez.</div>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.back()} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Geri
        </button>
      </div>
      <PageHeader
        title={`Ekspertiz Düzenle — ${data?.inspectionNo ?? ''}`}
        subtitle="Ekspertiz bilgilerini güncelleyin"
      />

      {error && <div className="alert-error mb-4">{error}</div>}

      <form onSubmit={handleSubmit} className="space-y-6 max-w-3xl">
        {/* Customer */}
        <div className="card p-6">
          <h2 className="text-base font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <User size={16} />
            Müşteri
          </h2>
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group">
              <label className="label">Müşteri Adı <span className="text-red-500">*</span></label>
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
          <div className="mb-5">
            <MotorcycleInspectionDiagram motorcycleType={data?.motorcycleType} zones={[]} />
          </div>
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group">
              <label className="label">Plaka <span className="text-red-500">*</span></label>
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
            <label className="label">Paket Tipi <span className="text-red-500">*</span></label>
            <select
              className="input"
              value={form.packageType}
              onChange={(e) => update('packageType', e.target.value)}
              required
            >
              {PACKAGE_TYPES.map((p) => (
                <option key={p.value} value={p.value}>{p.label}</option>
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
            {saving ? 'Kaydediliyor...' : 'Güncelle'}
          </button>
          <button type="button" onClick={() => router.back()} className="btn">
            İptal
          </button>
        </div>
      </form>
    </div>
  );
}
