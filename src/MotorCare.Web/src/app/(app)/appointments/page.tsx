'use client';

import { useEffect, useRef, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { CalendarPlus, CalendarRange, Search } from 'lucide-react';
import { EmptyState } from '@/components/ui/empty-state';
import { ErrorState } from '@/components/ui/error-state';
import { PageLoading } from '@/components/ui/loading';
import { PageHeader } from '@/components/ui/page-header';
import { dateTimeText } from '@/shared/utils/format';
import {
  APPOINTMENT_STATUS_OPTIONS,
  APPOINTMENT_TYPE_OPTIONS,
  appointmentStatusLabel,
  appointmentTypeLabel,
  type AppointmentDto,
} from '@/features/appointments/types';
import { useAppointments } from '@/features/appointments/hooks';
import { useAuth } from '@/core/auth/auth.context';
import { canCreateAppointment } from '@/shared/constants/permissions';

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

export default function AppointmentsPage(): React.ReactElement {
  const router = useRouter();
  const { user } = useAuth();
  const canCreate = canCreateAppointment(user?.role);
  const [inputValue, setInputValue] = useState<string>('');
  const [search, setSearch] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [typeFilter, setTypeFilter] = useState<string>('');
  const [page, setPage] = useState<number>(1);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      setSearch(inputValue.trim());
      setPage(1);
    }, 300);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [inputValue]);

  const { data, isLoading, error, refetch } = useAppointments({
    q: search,
    status: statusFilter,
    type: typeFilter,
    pageNumber: page,
    pageSize: 20,
  });

  const totalPages = data?.totalPages ?? Math.ceil((data?.totalCount ?? 0) / 20);

  return (
    <div>
      <PageHeader
        title="Randevular"
        subtitle={data ? `${data.totalCount} kayıt` : ''}
        actions={canCreate ? (
          <Link href="/appointments/new" className="btn-primary">
            <CalendarPlus size={16} />
            Yeni Randevu
          </Link>
        ) : undefined}
      />

      <div className="mb-4 flex flex-wrap gap-2">
        <div className="relative min-w-[220px] flex-1 sm:max-w-sm">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            className="input pl-9"
            placeholder="Müşteri, telefon veya plaka ara..."
            value={inputValue}
            onChange={(event) => setInputValue(event.target.value)}
          />
        </div>
        <select
          className="input w-full sm:w-auto"
          value={statusFilter}
          onChange={(event) => {
            setStatusFilter(event.target.value);
            setPage(1);
          }}
        >
          <option value="">Tüm Durumlar</option>
          {APPOINTMENT_STATUS_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        <select
          className="input w-full sm:w-auto"
          value={typeFilter}
          onChange={(event) => {
            setTypeFilter(event.target.value);
            setPage(1);
          }}
        >
          <option value="">Tüm Tipler</option>
          {APPOINTMENT_TYPE_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <PageLoading />}
      {error && <ErrorState message="Randevular yüklenemedi." onRetry={() => void refetch()} />}

      {!isLoading && !error && (
        <>
          {!data?.items || data.items.length === 0 ? (
            <EmptyState
              title="Randevu bulunamadı"
              description="Filtreleri değiştirin veya yeni bir randevu oluşturun."
              icon={<CalendarRange size={48} />}
              action={canCreate ? { label: 'Yeni Randevu', onClick: () => router.push('/appointments/new') } : undefined}
            />
          ) : (
            <>
              <div className="hidden overflow-hidden rounded-lg border border-slate-200 bg-white md:block">
                <div className="overflow-x-auto">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Müşteri</th>
                      <th>Plaka</th>
                      <th>Tip</th>
                      <th>Başlangıç</th>
                      <th>Bitiş</th>
                      <th>Durum</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.items.map((appointment) => (
                      <tr
                        key={appointment.id}
                        className="cursor-pointer"
                        onClick={() => router.push(`/appointments/${appointment.id}`)}
                      >
                        <td className="max-w-[180px]">
                          <p className="font-medium text-slate-900 truncate">{appointment.customerName}</p>
                          <p className="text-xs text-slate-400 truncate">{appointment.phone}</p>
                        </td>
                        <td className="font-medium text-slate-700">{appointment.plate || '-'}</td>
                        <td>{appointmentTypeLabel(appointment.type, appointment.typeText)}</td>
                        <td className="text-sm text-slate-600">{dateTimeText(appointment.startAt)}</td>
                        <td className="text-sm text-slate-600">{dateTimeText(appointment.endAt)}</td>
                        <td>
                          <StatusBadge appointment={appointment} />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                </div>
              </div>

              <div className="space-y-3 md:hidden">
                {data.items.map((appointment) => (
                  <button
                    key={appointment.id}
                    type="button"
                    onClick={() => router.push(`/appointments/${appointment.id}`)}
                    className="w-full rounded-lg border border-slate-200 bg-white p-4 text-left shadow-sm"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="font-semibold text-slate-900">{appointment.customerName}</p>
                        <p className="text-sm text-slate-500">{appointment.phone}</p>
                      </div>
                      <StatusBadge appointment={appointment} />
                    </div>
                    <div className="mt-3 grid grid-cols-2 gap-2 text-sm">
                      <div>
                        <p className="text-xs text-slate-400">Plaka</p>
                        <p className="font-medium text-slate-700">{appointment.plate || '-'}</p>
                      </div>
                      <div>
                        <p className="text-xs text-slate-400">Tip</p>
                        <p className="font-medium text-slate-700">
                          {appointmentTypeLabel(appointment.type, appointment.typeText)}
                        </p>
                      </div>
                    </div>
                    <p className="mt-3 text-sm text-slate-600">{dateTimeText(appointment.startAt)}</p>
                  </button>
                ))}
              </div>
            </>
          )}

          {totalPages > 1 && (
            <div className="mt-4 flex items-center justify-center gap-3">
              <button disabled={page === 1} onClick={() => setPage((value) => value - 1)} className="btn-secondary">
                Önceki
              </button>
              <span className="text-sm text-slate-600">
                {page} / {totalPages}
              </span>
              <button disabled={page >= totalPages} onClick={() => setPage((value) => value + 1)} className="btn-secondary">
                Sonraki
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
