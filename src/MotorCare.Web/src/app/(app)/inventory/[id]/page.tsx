'use client';

import { use, useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import { ArrowLeft, Package, CheckCircle, XCircle, AlertTriangle } from 'lucide-react';
import apiClient from '@/core/api/client';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { friendlyError } from '@/core/api/errors';
import { money } from '@/shared/utils/format';

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

const inventorySchema = z.object({
  name: z.string().min(2, 'Ürün adı en az 2 karakter olmalıdır'),
  sku: z.string().optional(),
  barcode: z.string().optional(),
  category: z.string().optional(),
  brand: z.string().optional(),
  unit: z.string().min(1, 'Birim zorunludur'),
  unitCost: z.number().min(0, 'Maliyet 0 veya daha büyük olmalıdır'),
  unitPrice: z.number().min(0, 'Fiyat 0 veya daha büyük olmalıdır'),
  quantity: z.number().int().min(0, 'Stok miktarı 0 veya daha büyük olmalıdır'),
  minStockLevel: z.number().int().min(0, 'Minimum stok 0 veya daha büyük olmalıdır').optional(),
  isActive: z.boolean(),
});

type InventoryFormValues = z.infer<typeof inventorySchema>;

const adjustSchema = z.object({
  delta: z.number().int().refine((v) => v !== 0, { message: 'Miktar sıfır olamaz' }),
  reason: z.string().min(1, 'Neden zorunludur'),
});

type AdjustFormValues = z.infer<typeof adjustSchema>;

export default function InventoryDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const qc = useQueryClient();

  const [toggling, setToggling] = useState(false);

  const { data, isLoading, error, refetch } = useQuery<InventoryItemDto>({
    queryKey: ['inventory-item', id],
    queryFn: async () => {
      const { data } = await apiClient.get<InventoryItemDto>(`/api/inventory/${id}`);
      return data;
    },
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting: saving },
  } = useForm<InventoryFormValues>({
    resolver: zodResolver(inventorySchema),
    defaultValues: {
      name: '',
      sku: '',
      barcode: '',
      category: '',
      brand: '',
      unit: '',
      unitCost: 0,
      unitPrice: 0,
      quantity: 0,
      minStockLevel: 0,
      isActive: true,
    },
  });

  const {
    register: registerAdj,
    handleSubmit: handleSubmitAdj,
    reset: resetAdj,
    formState: { errors: adjErrors, isSubmitting: adjusting },
  } = useForm<AdjustFormValues>({
    resolver: zodResolver(adjustSchema),
    defaultValues: { delta: 0, reason: '' },
  });

  useEffect(() => {
    if (data) {
      reset({
        name: data.name,
        sku: data.sku ?? '',
        barcode: data.barcode ?? '',
        category: data.category ?? '',
        brand: data.brand ?? '',
        unit: data.unit,
        unitCost: 0,
        unitPrice: data.unitPrice,
        quantity: data.stockQuantity,
        minStockLevel: data.minimumStockLevel,
        isActive: data.isActive,
      });
    }
  }, [data, reset]);

  async function onSave(values: InventoryFormValues) {
    try {
      await apiClient.put(`/api/inventory/${id}`, {
        name: values.name.trim(),
        sku: values.sku?.trim() || undefined,
        barcode: values.barcode?.trim() || undefined,
        category: values.category?.trim() || undefined,
        brand: values.brand?.trim() || undefined,
        unit: values.unit.trim(),
        unitPrice: values.unitPrice,
        stockQuantity: values.quantity,
        minimumStockLevel: values.minStockLevel ?? 0,
        isActive: values.isActive,
      });
      void qc.invalidateQueries({ queryKey: ['inventory-item', id] });
      void qc.invalidateQueries({ queryKey: ['inventory'] });
      toast.success('Stok kalemi kaydedildi');
    } catch (err) {
      toast.error(friendlyError(err, 'Ürün kaydedilemedi.'));
    }
  }

  async function handleToggleActive() {
    if (!data) return;
    setToggling(true);
    try {
      const endpoint = data.isActive
        ? `/api/inventory/${id}/deactivate`
        : `/api/inventory/${id}/activate`;
      await apiClient.put(endpoint);
      void qc.invalidateQueries({ queryKey: ['inventory-item', id] });
      void qc.invalidateQueries({ queryKey: ['inventory'] });
      toast.success(data.isActive ? 'Ürün pasif yapıldı' : 'Ürün aktif yapıldı');
    } catch (err) {
      toast.error(friendlyError(err, 'Durum değiştirilemedi.'));
    } finally {
      setToggling(false);
    }
  }

  async function onAdjust(values: AdjustFormValues) {
    try {
      await apiClient.post(`/api/inventory/${id}/adjust-stock`, {
        quantityDelta: values.delta,
        reason: values.reason.trim(),
      });
      void qc.invalidateQueries({ queryKey: ['inventory-item', id] });
      void qc.invalidateQueries({ queryKey: ['inventory'] });
      resetAdj({ delta: 0, reason: '' });
      toast.success('Stok güncellendi');
    } catch (err) {
      toast.error(friendlyError(err, 'Stok ayarlanamadı.'));
    }
  }

  if (isLoading) return <PageLoading />;
  if (error || !data) return <ErrorState message="Ürün bulunamadı." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.push('/inventory')} className="btn btn-ghost text-sm">
          <ArrowLeft size={14} />
          Stok Yönetimi
        </button>
      </div>

      <div className="flex items-start justify-between gap-4 flex-wrap mb-6">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-lg bg-slate-100 flex items-center justify-center shrink-0">
            <Package size={20} className="text-slate-500" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{data.name}</h1>
            {data.sku && <p className="text-sm text-slate-400">SKU: {data.sku}</p>}
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => void handleToggleActive()}
            disabled={toggling}
            className={`btn ${data.isActive ? 'btn-danger' : 'btn-primary'}`}
          >
            {data.isActive ? (
              <>
                <XCircle size={14} /> Pasif Yap
              </>
            ) : (
              <>
                <CheckCircle size={14} /> Aktif Yap
              </>
            )}
          </button>
        </div>
      </div>

      {/* Stock summary card */}
      <div className="grid sm:grid-cols-3 gap-4 mb-6">
        <div className={`card p-4 text-center${data.isLowStock ? ' border-yellow-300 bg-yellow-50' : ''}`}>
          <p className="text-xs text-slate-500 mb-1">Mevcut Stok</p>
          <p className={`text-4xl font-bold${data.isLowStock ? ' text-yellow-700' : ' text-slate-900'}`}>
            {data.stockQuantity}
          </p>
          <p className="text-sm text-slate-500 mt-1">{data.unit}</p>
          {data.isLowStock && (
            <div className="flex items-center justify-center gap-1 text-yellow-600 text-xs mt-2">
              <AlertTriangle size={12} />
              Düşük stok uyarısı
            </div>
          )}
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-slate-500 mb-1">Minimum Stok</p>
          <p className="text-4xl font-bold text-slate-700">{data.minimumStockLevel}</p>
          <p className="text-sm text-slate-500 mt-1">{data.unit}</p>
        </div>
        <div className="card p-4 text-center">
          <p className="text-xs text-slate-500 mb-1">Birim Fiyat</p>
          <p className="text-3xl font-bold text-slate-900">{money(data.unitPrice)}</p>
          <p className="text-sm text-slate-500 mt-1">/ {data.unit}</p>
        </div>
      </div>

      {/* Edit form */}
      <div className="card p-6 max-w-2xl mb-6">
        <h2 className="text-base font-semibold text-slate-800 mb-4">Ürün Bilgileri</h2>
        <form onSubmit={(e) => void handleSubmit(onSave)(e)} className="space-y-4">
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group sm:col-span-2">
              <label className="label">
                Ürün Adı <span className="text-red-500">*</span>
              </label>
              <input className="input" {...register('name')} />
              {errors.name && <p className="error-text mt-1">{errors.name.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">SKU</label>
              <input className="input" {...register('sku')} />
            </div>

            <div className="form-group">
              <label className="label">Barkod</label>
              <input className="input" {...register('barcode')} />
            </div>

            <div className="form-group">
              <label className="label">Kategori</label>
              <input className="input" {...register('category')} />
            </div>

            <div className="form-group">
              <label className="label">Marka</label>
              <input className="input" {...register('brand')} />
            </div>

            <div className="form-group">
              <label className="label">
                Birim <span className="text-red-500">*</span>
              </label>
              <input className="input" {...register('unit')} />
              {errors.unit && <p className="error-text mt-1">{errors.unit.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">
                Birim Fiyat (₺) <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                className="input"
                min={0}
                step={0.01}
                {...register('unitPrice', { valueAsNumber: true })}
              />
              {errors.unitPrice && <p className="error-text mt-1">{errors.unitPrice.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">Minimum Stok Seviyesi</label>
              <input
                type="number"
                className="input"
                min={0}
                {...register('minStockLevel', { valueAsNumber: true })}
              />
              {errors.minStockLevel && <p className="error-text mt-1">{errors.minStockLevel.message}</p>}
            </div>
          </div>

          <div className="form-group">
            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" className="w-4 h-4" {...register('isActive')} />
              <span className="label mb-0">Aktif</span>
            </label>
          </div>

          <div className="flex gap-3 pt-2">
            <button type="submit" disabled={saving} className="btn btn-primary">
              {saving ? 'Kaydediliyor...' : 'Kaydet'}
            </button>
          </div>
        </form>
      </div>

      {/* Stock adjustment */}
      <div className="card p-6 max-w-2xl">
        <h2 className="text-base font-semibold text-slate-800 mb-4">Stok Ayarla</h2>
        <form onSubmit={(e) => void handleSubmitAdj(onAdjust)(e)} className="space-y-4">
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group">
              <label className="label">
                Miktar Değişimi <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                className="input"
                placeholder="Örn: +10 veya -5"
                {...registerAdj('delta', { valueAsNumber: true })}
              />
              <p className="text-xs text-slate-400 mt-1">
                Pozitif: stok ekle, Negatif: stoktan düş
              </p>
              {adjErrors.delta && <p className="error-text mt-1">{adjErrors.delta.message}</p>}
            </div>
            <div className="form-group">
              <label className="label">
                Neden <span className="text-red-500">*</span>
              </label>
              <input
                className="input"
                placeholder="Örn: Satış, Sayım düzeltmesi"
                {...registerAdj('reason')}
              />
              {adjErrors.reason && <p className="error-text mt-1">{adjErrors.reason.message}</p>}
            </div>
          </div>
          <button type="submit" disabled={adjusting} className="btn btn-secondary">
            {adjusting ? 'Güncelleniyor...' : 'Stok Güncelle'}
          </button>
        </form>
      </div>
    </div>
  );
}
