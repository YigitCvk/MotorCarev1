'use client';

import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import apiClient from '@/core/api/client';
import { friendlyError } from '@/core/api/errors';
import { PageHeader } from '@/components/ui/page-header';
import { CustomerForm, type CustomerFormValues } from '@/features/customers/components';

export default function CustomerCreatePage() {
  const router = useRouter();

  async function onSubmit(values: CustomerFormValues) {
    try {
      await apiClient.post('/api/customers', values);
      toast.success('Müşteri oluşturuldu');
      router.push('/customers');
    } catch (err) {
      toast.error(friendlyError(err, 'Müşteri kaydedilemedi.'));
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
      <PageHeader title="Yeni Müşteri" subtitle="Müşteri bilgilerini girin" />
      <div className="card p-4 sm:p-6 max-w-2xl">
        <CustomerForm
          submitLabel="Kaydet"
          submittingLabel="Kaydediliyor..."
          onCancel={() => router.back()}
          onSubmit={onSubmit}
        />
      </div>
    </div>
  );
}
