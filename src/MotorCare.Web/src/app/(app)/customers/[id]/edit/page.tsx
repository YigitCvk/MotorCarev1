'use client';

import { use } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import apiClient from '@/core/api/client';
import { friendlyError } from '@/core/api/errors';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { CustomerForm, type CustomerFormValues } from '@/features/customers/components';
import type { Customer } from '@/features/customers/types';

export default function CustomerEditPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const qc = useQueryClient();

  const { data, isLoading, error, refetch } = useQuery<Customer>({
    queryKey: ['customer', id],
    queryFn: async () => {
      const { data } = await apiClient.get<Customer>(`/api/customers/${id}`);
      return data;
    },
  });

  async function onSubmit(values: CustomerFormValues) {
    try {
      await apiClient.put(`/api/customers/${id}`, values);
      await qc.invalidateQueries({ queryKey: ['customer', id] });
      await qc.invalidateQueries({ queryKey: ['customers'] });
      toast.success('Müşteri güncellendi');
      router.push(`/customers/${id}`);
    } catch (err) {
      toast.error(friendlyError(err, 'Müşteri kaydedilemedi.'));
    }
  }

  if (isLoading) return <PageLoading />;
  if (error || !data) return <ErrorState message="Müşteri bulunamadı." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.back()} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Geri
        </button>
      </div>
      <PageHeader title="Müşteriyi Düzenle" subtitle={data.fullName} />
      <div className="card p-4 sm:p-6 max-w-2xl">
        <CustomerForm
          initialValues={data}
          submitLabel="Kaydet"
          submittingLabel="Kaydediliyor..."
          onCancel={() => router.back()}
          onSubmit={onSubmit}
        />
      </div>
    </div>
  );
}
