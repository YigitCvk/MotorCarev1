'use client';

import Link from 'next/link';
import { useQuery } from '@tanstack/react-query';
import { BarChart3, ClipboardList, TrendingUp, CreditCard, Users, Calendar } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import apiClient from '@/core/api/client';
import { StatCard } from '@/components/ui/card';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { ServiceOrderStatusBadge } from '@/components/ui/badge';
import { money, dateText } from '@/shared/utils/format';

interface MonthlyRevenueStat {
  month: string;
  revenue: number;
  orderCount: number;
}

const TR_MONTHS: Record<string, string> = {
  Jan: 'Oca', Feb: 'Şub', Mar: 'Mar', Apr: 'Nis', May: 'May', Jun: 'Haz',
  Jul: 'Tem', Aug: 'Ağu', Sep: 'Eyl', Oct: 'Eki', Nov: 'Kas', Dec: 'Ara',
};

function toTrMonth(month: string): string {
  // Handles both "Jan", "January", "2024-01", "01", numeric strings
  const abbr = month.slice(0, 3);
  return TR_MONTHS[abbr] ?? month;
}

interface DailyDashboard {
  totalRevenue?: number;
  activeServiceOrders?: number;
  completedToday?: number;
  totalCustomers?: number;
  todayAppointments?: number;
  pendingPayment?: number;
  recentServiceOrders?: Record<string, unknown>[];
  todayAppointmentsList?: Record<string, unknown>[];
  criticalInspections?: Record<string, unknown>[];
  [key: string]: unknown;
}

function safeNumber(v: unknown): number {
  return typeof v === 'number' ? v : 0;
}

function safeStr(v: unknown, fallback = '-'): string {
  return typeof v === 'string' && v ? v : fallback;
}

function safeArr(v: unknown): Record<string, unknown>[] {
  return Array.isArray(v) ? (v as Record<string, unknown>[]) : [];
}

export default function DashboardPage() {
  const { data, isLoading, error, refetch } = useQuery<DailyDashboard>({
    queryKey: ['dashboard', 'daily'],
    queryFn: async () => {
      const { data } = await apiClient.get<DailyDashboard>('/api/dashboard/daily');
      return data;
    },
    staleTime: 30_000,
  });

  const { data: monthlyData, isError: monthlyError } = useQuery<MonthlyRevenueStat[]>({
    queryKey: ['dashboard', 'monthly'],
    queryFn: async () => {
      const { data } = await apiClient.get<MonthlyRevenueStat[]>('/api/dashboard/monthly');
      return data;
    },
    staleTime: 60_000,
    retry: false,
  });

  if (isLoading) return <PageLoading />;

  if (error) {
    return (
      <div>
        <PageHeader title="Dashboard" subtitle="Günlük operasyon özeti" />
        <ErrorState message="Dashboard bilgileri şu anda yüklenemedi. Lütfen tekrar deneyin." onRetry={() => void refetch()} />
      </div>
    );
  }

  const metrics = [
    {
      title: 'Toplam Gelir (Bugün)',
      value: money(safeNumber(data?.totalRevenue)),
      icon: <TrendingUp size={22} />,
      color: 'green' as const,
    },
    {
      title: 'Aktif Servis',
      value: String(safeNumber(data?.activeServiceOrders ?? data?.openServiceOrders)),
      icon: <ClipboardList size={22} />,
      color: 'blue' as const,
    },
    {
      title: 'Bugün Tamamlanan',
      value: String(safeNumber(data?.completedToday ?? data?.completedServiceOrders)),
      icon: <BarChart3 size={22} />,
      color: 'default' as const,
    },
    {
      title: 'Ödeme Bekliyor',
      value: money(safeNumber(data?.pendingPayment ?? data?.pendingPayments)),
      icon: <CreditCard size={22} />,
      color: 'orange' as const,
    },
    {
      title: 'Toplam Müşteri',
      value: String(safeNumber(data?.totalCustomers)),
      icon: <Users size={22} />,
      color: 'default' as const,
    },
    {
      title: 'Bugünkü Randevu',
      value: String(safeNumber(data?.todayAppointments)),
      icon: <Calendar size={22} />,
      color: 'default' as const,
    },
  ];

  const recentOrders = safeArr(data?.recentServiceOrders ?? data?.serviceOrders ?? data?.orders);
  const appointments = safeArr(data?.todayAppointmentsList ?? data?.appointments ?? data?.todayAppointments as unknown);
  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <PageHeader title="Dashboard" subtitle="Günlük operasyon özeti" />
        <button onClick={() => void refetch()} className="btn-secondary text-sm px-3 py-1.5">
          Yenile
        </button>
      </div>

      {/* Metrics grid */}
      <div className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-6">
        {metrics.map((m) => (
          <StatCard key={m.title} title={m.title} value={m.value} icon={m.icon} color={m.color} />
        ))}
      </div>

      {/* Monthly Revenue Chart */}
      <div className="card p-5 mb-6">
        <h2 className="font-semibold text-slate-900 mb-4">Aylık Gelir</h2>
        {monthlyError || !monthlyData || monthlyData.length === 0 ? (
          <p className="text-sm text-slate-400 text-center py-8">Grafik verisi henüz mevcut değil</p>
        ) : (
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={monthlyData.map((d) => ({ ...d, monthLabel: toTrMonth(d.month) }))} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
              <XAxis dataKey="monthLabel" tick={{ fontSize: 12 }} tickLine={false} axisLine={false} />
              <YAxis yAxisId="revenue" orientation="left" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} tickFormatter={(v: number) => `₺${(v / 1000).toFixed(0)}k`} />
              <YAxis yAxisId="orders" orientation="right" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} />
              <Tooltip
                formatter={(value, name) => {
                  const nameStr = String(name ?? '');
                  return nameStr === 'Gelir'
                    ? [`₺${Number(value).toLocaleString('tr-TR', { minimumFractionDigits: 2 })}`, nameStr]
                    : [value, nameStr];
                }}
                labelFormatter={(label) => String(label)}
              />
              <Legend />
              <Bar yAxisId="revenue" dataKey="revenue" name="Gelir" fill="#2563eb" radius={[4, 4, 0, 0]} />
              <Bar yAxisId="orders" dataKey="orderCount" name="Sipariş" fill="#94a3b8" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>

      {/* Content grid */}
      <div className="grid lg:grid-cols-2 gap-6">
        {/* Recent service orders */}
        <div className="card p-0 overflow-hidden">
          <div className="px-5 py-4 border-b border-slate-100 flex items-center justify-between">
            <h2 className="font-semibold text-slate-900">Son Servis Kayıtları</h2>
            <Link href="/service-orders" className="text-xs text-brand-600 hover:text-brand-700">Tümünü gör →</Link>
          </div>
          {recentOrders.length === 0 ? (
            <div className="px-5 py-8 text-center text-sm text-slate-400">Henüz servis kaydı yok.</div>
          ) : (
            <ul className="divide-y divide-slate-100">
              {recentOrders.slice(0, 5).map((order, idx) => (
                <li key={safeStr(order['id'], String(idx))}>
                  <Link
                    href={`/service-orders/${safeStr(order['id'], '#')}`}
                    className="flex items-center justify-between px-5 py-3 hover:bg-slate-50 transition-colors"
                  >
                    <div>
                      <p className="text-sm font-medium text-slate-900">
                        {safeStr(order['orderNo'] ?? order['serviceOrderNo'] ?? order['vehiclePlate'])}
                      </p>
                      <p className="text-xs text-slate-500">
                        {safeStr(order['customerName'] ?? order['vehiclePlate'])}
                      </p>
                    </div>
                    <div className="flex items-center gap-3">
                      {!!order['status'] && (
                        <ServiceOrderStatusBadge status={safeStr(order['status'])} />
                      )}
                      <span className="text-xs text-slate-400">{dateText(order['openedAt'] as string)}</span>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </div>

        {/* Appointments or Inspections */}
        <div className="card p-0 overflow-hidden">
          <div className="px-5 py-4 border-b border-slate-100 flex items-center justify-between">
            <h2 className="font-semibold text-slate-900">Bugünkü Randevular</h2>
            <Link href="/appointments" className="text-xs text-brand-600 hover:text-brand-700">Tümünü gör →</Link>
          </div>
          {appointments.length === 0 ? (
            <div className="px-5 py-8 text-center text-sm text-slate-400">Bugün randevu yok.</div>
          ) : (
            <ul className="divide-y divide-slate-100">
              {appointments.slice(0, 5).map((apt, idx) => (
                <li key={safeStr(apt['id'], String(idx))} className="flex items-center justify-between px-5 py-3">
                  <div>
                    <p className="text-sm font-medium text-slate-900">
                      {safeStr(apt['customerName'] ?? apt['vehiclePlate'] ?? apt['title'])}
                    </p>
                    <p className="text-xs text-slate-500">{safeStr(apt['vehiclePlate'] ?? apt['time'])}</p>
                  </div>
                  <span className="text-xs text-slate-400">{safeStr(apt['time'] ?? apt['scheduledAt'])}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>

      {/* Quick actions */}
      <div className="mt-6 flex flex-wrap gap-3">
        <Link href="/service-orders/new" className="btn-secondary">Yeni Servis Emri</Link>
        <Link href="/customers/create" className="btn-secondary">Yeni Müşteri</Link>
        <Link href="/inspections/new" className="btn-secondary">Yeni Expertiz</Link>
      </div>
    </div>
  );
}
