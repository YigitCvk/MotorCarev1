'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import { friendlyError } from '@/core/api/errors';
import { PageHeader } from '@/components/ui/page-header';
import { AppointmentForm } from '@/features/appointments/components/AppointmentForm';
import { useCreateAppointment } from '@/features/appointments/hooks';
import type { AppointmentUpsertRequest } from '@/features/appointments/types';

export default function NewAppointmentPage(): React.ReactElement {
  const router = useRouter();
  const createAppointment = useCreateAppointment();
  const [formError, setFormError] = useState<string>('');

  function handleSubmit(body: AppointmentUpsertRequest): void {
    setFormError('');
    createAppointment.mutate(body, {
      onSuccess: (appointment) => {
        toast.success('Randevu oluşturuldu');
        router.push(`/appointments/${appointment.id}`);
      },
      onError: (error) => {
        const message = friendlyError(error, 'Randevu oluşturulamadı.');
        setFormError(message);
        toast.error(message);
      },
    });
  }

  return (
    <div>
      <div className="mb-4">
        <button onClick={() => router.push('/appointments')} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Randevular
        </button>
      </div>

      <PageHeader
        title="Yeni Randevu"
        subtitle="Müşteri, araç ve zaman bilgisini girerek randevu oluşturun."
      />

      <AppointmentForm
        submitLabel="Randevu Oluştur"
        isSubmitting={createAppointment.isPending}
        error={formError}
        onSubmit={handleSubmit}
        onCancel={() => router.push('/appointments')}
      />
    </div>
  );
}
