'use client';

import { useRouter } from 'next/navigation';
import { ArrowLeft } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import apiClient from '@/core/api/client';
import { PageHeader } from '@/components/ui/page-header';
import { friendlyError } from '@/core/api/errors';

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

export default function InventoryCreatePage() {
  const router = useRouter();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
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

  async function onSubmit(values: InventoryFormValues) {
    try {
      const payload = {
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
      };
      const { data } = await apiClient.post<string | { id: string }>('/api/inventory', payload);
      const id = typeof data === 'string' ? data : data.id;
      toast.success('Stok kalemi kaydedildi');
      router.push(`/inventory/${id}`);
    } catch (err) {
      toast.error(friendlyError(err, 'Ürün kaydedilemedi.'));
    }
  }

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.push('/inventory')} className="btn btn-ghost text-sm">
          <ArrowLeft size={14} />
          Stok Yönetimi
        </button>
      </div>

      <PageHeader title="Yeni Ürün" subtitle="Stok ürünü bilgilerini girin" />

      <div className="card p-6 max-w-2xl">
        <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="space-y-4">
          <div className="grid sm:grid-cols-2 gap-4">
            <div className="form-group sm:col-span-2">
              <label className="label">
                Ürün Adı <span className="text-red-500">*</span>
              </label>
              <input
                className="input"
                placeholder="Örn: Motor Yağı 10W-40"
                {...register('name')}
              />
              {errors.name && <p className="error-text mt-1">{errors.name.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">SKU</label>
              <input
                className="input"
                placeholder="Stok kodu"
                {...register('sku')}
              />
            </div>

            <div className="form-group">
              <label className="label">Barkod</label>
              <input
                className="input"
                placeholder="Barkod numarası"
                {...register('barcode')}
              />
            </div>

            <div className="form-group">
              <label className="label">Kategori</label>
              <input
                className="input"
                placeholder="Örn: Yağlar, Filtreler"
                {...register('category')}
              />
            </div>

            <div className="form-group">
              <label className="label">Marka</label>
              <input
                className="input"
                placeholder="Örn: Castrol, Bosch"
                {...register('brand')}
              />
            </div>

            <div className="form-group">
              <label className="label">
                Birim <span className="text-red-500">*</span>
              </label>
              <input
                className="input"
                placeholder="Örn: adet, lt, kg"
                {...register('unit')}
              />
              {errors.unit && <p className="error-text mt-1">{errors.unit.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">Birim Maliyet (₺)</label>
              <input
                type="number"
                className="input"
                placeholder="0.00"
                min={0}
                step={0.01}
                {...register('unitCost', { valueAsNumber: true })}
              />
              {errors.unitCost && <p className="error-text mt-1">{errors.unitCost.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">
                Birim Fiyat (₺) <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                className="input"
                placeholder="0.00"
                min={0}
                step={0.01}
                {...register('unitPrice', { valueAsNumber: true })}
              />
              {errors.unitPrice && <p className="error-text mt-1">{errors.unitPrice.message}</p>}
            </div>

            <div className="form-group">
              <label className="label">
                Mevcut Stok <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                className="input"
                min={0}
                {...register('quantity', { valueAsNumber: true })}
              />
              {errors.quantity && <p className="error-text mt-1">{errors.quantity.message}</p>}
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
              <input
                type="checkbox"
                className="w-4 h-4"
                {...register('isActive')}
              />
              <span className="label mb-0">Aktif</span>
            </label>
          </div>

          <div className="flex gap-3 pt-2">
            <button type="submit" disabled={isSubmitting} className="btn btn-primary">
              {isSubmitting ? 'Kaydediliyor...' : 'Kaydet'}
            </button>
            <button type="button" onClick={() => router.push('/inventory')} className="btn btn-secondary">
              İptal
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
