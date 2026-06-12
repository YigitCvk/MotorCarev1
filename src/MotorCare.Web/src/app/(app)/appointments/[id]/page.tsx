'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useParams, useRouter } from 'next/navigation';
import { ArrowLeft, CalendarClock, Car, CheckCircle2, Pencil, UserRound, Wrench } from 'lucide-react';
import { toast } from 'sonner';
import { friendlyError } from '@/core/api/errors';
import { ErrorState } from '@/components/ui/error-state';
import { PageLoading } from '@/components/ui/loading';
import { dateTimeText } from '@/shared/utils/format';
import {
  APPOINTMENT_STATUS_OPTIONS,
  appointmentStatusLabel,
  appointmentTypeLabel,
  type AppointmentDto,
  type AppointmentStatus,
} from '@/features/appointments/types';
import {
  useAppointment,
  useCancelAppointment,
  useConvertAppointment,
  useUpdateAppointmentStatus,
} from '@/features/appointments/hooks';
import { useAuth } from '@/core/auth/auth.context';
import { canCreateServiceOrder, canEditAppointment } from '@/shared/constants/permissions';

function StatusBadge({ appointment }: { appointment: AppointmentDto }): React.ReactElement {
  const label = appointmentStatusLabel(appointment.status, appointment.statusText);
  const className =
    appointment.status === 'Cancelled' || appointment.status === 'NoShow'
      ? 'badge badge-red'
      : appointment.status === 'ConvertedToOrder' || appointment.status === 'Completed'
        ? 'badge badge-green'
        : appointment.status === 'CheckedIn'
          ? 'badge badge-yellow'
          : 'badge badge-blue';

  return <span className={className}>{label}</span>;
}

function DetailItem({ label, value }: { label: string; value: React.ReactNode }): React.ReactElement {
  return (
    <div>
      <p className="text-xs font-medium uppercase tracking-wide text-slate-400">{label}</p>
      <div className="mt-1 text-sm font-medium text-slate-800">{value}</div>
    </div>
  );
}

export default function AppointmentDetailPage(): React.ReactElement {
  const params = useParams();
  const router = useRouter();
  const { user } = useAuth();
  const canEdit = canEditAppointment(user?.role);
  const id = params.id as string;
  const { data, isLoading, error, refetch } = useAppointment(id);
  const updateStatus = useUpdateAppointmentStatus(id);
  const cancelAppointment = useCancelAppointment(id);
  const convertAppointment = useConvertAppointment(id);

  const [statusValue, setStatusValue] = useState<AppointmentStatus>('Scheduled');
  const [vehicleKm, setVehicleKm] = useState<string>('');
  const [actionError, setActionError] = useState<string>('');

  useEffect(() => {
    if (data) setStatusValue(data.status);
  }, [data]);

  if (isLoading) return <PageLoading />;
  if (error || !data) return <ErrorState message="Randevu yüklenemedi." onRetry={() => void refetch()} />;

  const appointment = data;
  const terminalStatus = data.status === 'Cancelled' || data.status === 'ConvertedToOrder' || data.status === 'Completed';
  const canConvert =
    canCreateServiceOrder(user?.role) &&
    !terminalStatus &&
    Boolean(data.customerId && data.vehicleId);

  function handleStatusUpdate(): void {
    if (!data || statusValue === data.status) return;
    setActionError('');
    updateStatus.mutate(statusValue, {
      onSuccess: () => toast.success('Durum güncellendi'),
      onError: (err) => {
        const message = friendlyError(err, 'Durum güncellenemedi.');
        setActionError(message);
        toast.error(message);
      },
    });
  }

  function handleCancel(): void {
    if (!window.confirm('Randevu iptal edilsin mi?')) return;
    setActionError('');
    cancelAppointment.mutate(undefined, {
      onSuccess: () => toast.success('Randevu iptal edildi'),
      onError: (err) => {
        const message = friendlyError(err, 'Randevu iptal edilemedi.');
        setActionError(message);
        toast.error(message);
      },
    });
  }

  function handleConvert(): void {
    if (!appointment.customerId || !appointment.vehicleId) {
      setActionError('Servis emrine dönüştürmek için müşteri ve araç seçilmelidir.');
      return;
    }

    const km = Number(vehicleKm);
    if (Number.isNaN(km) || km < 0) {
      setActionError('Geçerli bir araç KM değeri girin.');
      return;
    }

    setActionError('');
    convertAppointment.mutate(km, {
      onSuccess: (result) => {
        toast.success('Servis emri oluşturuldu');
        router.push(`/service-orders/${result.serviceOrderId}`);
      },
      onError: (err) => {
        const message = friendlyError(err, 'Servis emrine dönüştürülemedi.');
        setActionError(message);
        toast.error(message);
      },
    });
  }

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
        <button onClick={() => router.push('/appointments')} className="btn-ghost text-sm">
          <ArrowLeft size={14} />
          Randevular
        </button>
        <div className="flex flex-wrap items-center gap-2">
          {canEdit && (
            <Link href={`/appointments/${id}/edit`} className="btn-secondary text-sm">
              <Pencil size={14} />
              Düzenle
            </Link>
          )}
          {data.serviceOrderId && (
            <Link href={`/service-orders/${data.serviceOrderId}`} className="btn-secondary text-sm">
              <Wrench size={14} />
              Servis Emri
            </Link>
          )}
        </div>
      </div>

      {actionError && (
        <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {actionError}
        </div>
      )}

      <div className="mb-4 rounded-lg border border-slate-200 bg-white p-4 sm:p-5">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <div className="mb-2 flex items-center gap-2">
              <CalendarClock size={20} className="text-brand-600" />
              <h1 className="text-xl font-bold text-slate-900">Randevu Detayı</h1>
            </div>
            <p className="text-sm text-slate-500">
              {dateTimeText(data.startAt)} - {dateTimeText(data.endAt)}
            </p>
          </div>
          <StatusBadge appointment={data} />
        </div>
      </div>

      <div className="grid gap-4 lg:grid-cols-[1fr_320px]">
        <div className="space-y-4">
          <section className="rounded-lg border border-slate-200 bg-white p-4 sm:p-5">
            <div className="mb-4 flex items-center gap-2">
              <UserRound size={18} className="text-slate-400" />
              <h2 className="text-sm font-semibold text-slate-800">Müşteri</h2>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <DetailItem label="Müşteri" value={data.customerName} />
              <DetailItem label="Telefon" value={data.phone} />
            </div>
          </section>

          <section className="rounded-lg border border-slate-200 bg-white p-4 sm:p-5">
            <div className="mb-4 flex items-center gap-2">
              <Car size={18} className="text-slate-400" />
              <h2 className="text-sm font-semibold text-slate-800">Araç ve Randevu</h2>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <DetailItem label="Plaka" value={data.plate || '-'} />
              <DetailItem label="Tip" value={appointmentTypeLabel(data.type, data.typeText)} />
              <DetailItem label="Başlangıç" value={dateTimeText(data.startAt)} />
              <DetailItem label="Bitiş" value={dateTimeText(data.endAt)} />
            </div>
          </section>

          <section className="rounded-lg border border-slate-200 bg-white p-4 sm:p-5">
            <h2 className="mb-3 text-sm font-semibold text-slate-800">Notlar</h2>
            <div className="space-y-4">
              <DetailItem label="Şikayet" value={data.complaint || '-'} />
              <DetailItem label="Not" value={data.note || '-'} />
            </div>
          </section>
        </div>

        <aside className="space-y-4">
          {canEdit && (
            <section className="rounded-lg border border-slate-200 bg-white p-4">
              <h2 className="mb-3 text-sm font-semibold text-slate-800">Durum Güncelle</h2>
              <div className="space-y-3">
                <select
                  className="input"
                  value={statusValue}
                  disabled={updateStatus.isPending || data.status === 'ConvertedToOrder'}
                  onChange={(event) => setStatusValue(event.target.value as AppointmentStatus)}
                >
                  {APPOINTMENT_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
                <button
                  type="button"
                  className="btn-primary w-full"
                  disabled={updateStatus.isPending || statusValue === data.status || data.status === 'ConvertedToOrder'}
                  onClick={handleStatusUpdate}
                >
                  <CheckCircle2 size={16} />
                  {updateStatus.isPending ? 'Güncelleniyor...' : 'Durumu Kaydet'}
                </button>
                {!terminalStatus && (
                  <button
                    type="button"
                    className="btn-danger w-full"
                    disabled={cancelAppointment.isPending}
                    onClick={handleCancel}
                  >
                    {cancelAppointment.isPending ? 'İptal ediliyor...' : 'Randevuyu İptal Et'}
                  </button>
                )}
              </div>
            </section>
          )}

          {canConvert && (
            <section className="rounded-lg border border-slate-200 bg-white p-4">
              <h2 className="mb-3 text-sm font-semibold text-slate-800">Servis Emrine Dönüştür</h2>
              <div className="space-y-3">
                <div className="form-group">
                  <label className="label">Araç KM *</label>
                  <input
                    type="number"
                    min={0}
                    className="input"
                    value={vehicleKm}
                    onChange={(event) => setVehicleKm(event.target.value)}
                    placeholder="Ör. 85000"
                  />
                </div>
                <button
                  type="button"
                  className="btn-primary w-full"
                  disabled={convertAppointment.isPending}
                  onClick={handleConvert}
                >
                  <Wrench size={16} />
                  {convertAppointment.isPending ? 'Dönüştürülüyor...' : 'Servis Emri Oluştur'}
                </button>
              </div>
            </section>
          )}
        </aside>
      </div>
    </div>
  );
}
