'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import { friendlyError } from '@/core/api/errors';
import { ErrorState } from '@/components/ui/error-state';
import { PageLoading } from '@/components/ui/loading';
import { PageHeader } from '@/components/ui/page-header';
import { AppointmentForm } from '@/features/appointments/components/AppointmentForm';
import { useAppointment, useUpdateAppointment } from '@/features/appointments/hooks';
import type { AppointmentUpsertRequest } from '@/features/appointments/types';

export default function EditAppointmentPage(): React.ReactElement {
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;
  const { data, isLoading, error, refetch } = useAppointment(id);
  const updateAppointment = useUpdateAppointment(id);
  const [formError, setFormError] = useState<string>('');

  function handleSubmit(body: AppointmentUpsertRequest): void {
    setFormError('');
    updateAppointment.mutate(body, {
      onSuccess: (appointment) => {
        toast.success('Randevu guncellendi');
        router.push(`/appointments/${appointment.id}`);
      },
      onError: (err) => {
        const message = friendlyError(err, 'Randevu guncellenemedi.');
        setFormError(message);
        toast.error(message);
      },
    });
  }

  if (isLoading) return <PageLoading />;
  if (error || !data) return <ErrorState message="Randevu yuklenemedi." onRetry={() => void refetch()} />;

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.push(`/appointments/${id}`)} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Randevu Detayi
        </button>
      </div>

      <PageHeader title="Randevu Duzenle" subtitle={data.customerName} />

      <AppointmentForm
        appointment={data}
        submitLabel="Degisiklikleri Kaydet"
        isSubmitting={updateAppointment.isPending}
        error={formError}
        onSubmit={handleSubmit}
        onCancel={() => router.push(`/appointments/${id}`)}
      />
    </div>
  );
}
