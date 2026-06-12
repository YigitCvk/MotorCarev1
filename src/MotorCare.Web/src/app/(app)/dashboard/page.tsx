'use client';

import Link from 'next/link';
import { useQuery } from '@tanstack/react-query';
import { BarChart3, ClipboardList, TrendingUp, CreditCard, Users, Calendar, Wallet, AlertTriangle } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import apiClient from '@/core/api/client';
import { StatCard } from '@/components/ui/card';
import { PageHeader } from '@/components/ui/page-header';
import { PageLoading } from '@/components/ui/loading';
import { ErrorState } from '@/components/ui/error-state';
import { ServiceOrderStatusBadge } from '@/components/ui/badge';
import { money, dateText } from '@/shared/utils/format';
import { isRecord, normalizeApiArray, readNumber, readString } from '@/shared/utils/api-normalize';
import { useAuth } from '@/core/auth/auth.context';
import { appConfig } from '@/shared/config/env';
import {
  canCreateCustomer,
  canCreateServiceOrder,
  canManageInspection,
} from '@/shared/constants/permissions';

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
  const isoMonth = /^(\d{4})-(\d{2})$/.exec(month);
  if (isoMonth) {
    const monthNumber = Number(isoMonth[2]);
    const label = Object.values(TR_MONTHS)[monthNumber - 1];
    return label ?? month;
  }

  // Handles both "Jan", "January", "01", numeric strings
  const abbr = month.slice(0, 3);
  return TR_MONTHS[abbr] ?? month;
}

interface DailyPaymentSummary {
  date: string;
  total: number;
  cash: number;
  creditCard: number;
  bankTransfer: number;
  paymentCount: number;
}

interface PaymentSummary {
  totalCollected: number;
  cashTotal: number;
  creditCardTotal: number;
  bankTransferTotal: number;
  openBalance: number;
  totalOrdersInPeriod: number;
  paidOrdersCount: number;
  partiallyPaidOrdersCount: number;
  unpaidOrdersCount: number;
  dailyBreakdown: DailyPaymentSummary[];
}

interface OpenBalanceItem {
  serviceOrderId: string;
  orderNo: string;
  customerName: string | null;
  vehiclePlate: string | null;
  grandTotal: number;
  paidTotal: number;
  remainingTotal: number;
  status: string;
  openedAt: string;
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

function normalizeMonthlyRevenueStats(value: unknown): MonthlyRevenueStat[] {
  return normalizeApiArray<unknown>(value)
    .map((item) => {
      if (!isRecord(item)) return null;

      const month = readString(item.month ?? item.Month);
      if (!month) return null;

      return {
        month,
        revenue: readNumber(item.revenue ?? item.Revenue) ?? 0,
        orderCount: readNumber(item.orderCount ?? item.OrderCount ?? item.orders ?? item.Orders) ?? 0,
      };
    })
    .filter((item): item is MonthlyRevenueStat => item !== null);
}

export default function DashboardPage() {
  const { user } = useAuth();
  const { data, isLoading, error, refetch } = useQuery<DailyDashboard>({
    queryKey: ['dashboard', 'daily'],
    queryFn: async () => {
      const { data } = await apiClient.get<DailyDashboard>('/api/dashboard/daily');
      return data;
    },
    staleTime: 30_000,
  });

  const { data: paymentSummary, isLoading: isPaymentLoading, isError: paymentError, refetch: refetchPayment } = useQuery<PaymentSummary>({
    queryKey: ['dashboard', 'payment-summary'],
    queryFn: async () => {
      const to = new Date();
      const from = new Date(to.getFullYear(), to.getMonth(), 1);
      const params = new URLSearchParams({
        from: from.toISOString(),
        to: to.toISOString(),
      });
      const { data } = await apiClient.get<PaymentSummary>(`/api/dashboard/payment-summary?${params.toString()}`);
      return data;
    },
    staleTime: 30_000,
  });

  const { data: openBalances, isLoading: isBalancesLoading, isError: balancesError, refetch: refetchBalances } = useQuery<OpenBalanceItem[]>({
    queryKey: ['dashboard', 'open-balances'],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>('/api/dashboard/open-balances');
      return normalizeApiArray<OpenBalanceItem>(data);
    },
    staleTime: 30_000,
  });

  const {
    data: monthlyData,
    isLoading: isMonthlyLoading,
    isError: monthlyError,
  } = useQuery<MonthlyRevenueStat[]>({
    queryKey: ['dashboard', 'monthly'],
    queryFn: async () => {
      const { data } = await apiClient.get<unknown>('/api/dashboard/monthly');
      return normalizeMonthlyRevenueStats(data);
    },
    enabled: appConfig.monthlyDashboardEnabled,
    staleTime: 60_000,
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
  const monthlyRows = monthlyData ?? [];
  const balanceRows = openBalances ?? [];

  function refreshDashboard(): void {
    void refetch();
    void refetchPayment();
    void refetchBalances();
  }

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-2 mb-6">
        <PageHeader title="Dashboard" subtitle="Günlük operasyon özeti" />
        <button onClick={refreshDashboard} className="btn-secondary text-sm px-3 py-1.5 shrink-0">
          Yenile
        </button>
      </div>

      {/* Metrics grid */}
      <div className="grid grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-6">
        {metrics.map((m) => (
          <StatCard key={m.title} title={m.title} value={m.value} icon={m.icon} color={m.color} />
        ))}
      </div>

      {/* Payment overview */}
      <div className="grid xl:grid-cols-3 gap-6 mb-6">
        <div className="card p-5">
          <div className="flex items-center gap-2 mb-4">
            <Wallet size={18} className="text-emerald-600" />
            <div>
              <h2 className="font-semibold text-slate-900">Bu Ay Tahsilat</h2>
              <p className="text-xs text-slate-500">Ay başından bugüne</p>
            </div>
          </div>

          {isPaymentLoading ? (
            <p className="text-sm text-slate-400 py-6 text-center">Ödeme özeti yükleniyor...</p>
          ) : paymentError ? (
            <div className="py-4 text-center">
              <p className="text-sm text-rose-500">Ödeme özeti yüklenemedi.</p>
              <button onClick={() => void refetchPayment()} className="mt-2 text-xs text-brand-600 hover:underline">
                Yeniden dene
              </button>
            </div>
          ) : paymentSummary ? (
            <div className="space-y-4">
              <div>
                <p className="text-2xl font-bold text-slate-900">{money(paymentSummary.totalCollected)}</p>
                <p className="text-xs text-slate-500">{paymentSummary.totalOrdersInPeriod} servis emri</p>
              </div>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between gap-3">
                  <span className="text-slate-500">Nakit</span>
                  <span className="font-medium text-slate-800">{money(paymentSummary.cashTotal)}</span>
                </div>
                <div className="flex justify-between gap-3">
                  <span className="text-slate-500">Kredi Kartı</span>
                  <span className="font-medium text-slate-800">{money(paymentSummary.creditCardTotal)}</span>
                </div>
                <div className="flex justify-between gap-3">
                  <span className="text-slate-500">Banka Transferi</span>
                  <span className="font-medium text-slate-800">{money(paymentSummary.bankTransferTotal)}</span>
                </div>
              </div>
              <div className="pt-3 border-t border-slate-100 flex justify-between gap-3 text-sm">
                <span className="text-slate-500">Dönem açık bakiyesi</span>
                <span className="font-semibold text-amber-700">{money(paymentSummary.openBalance)}</span>
              </div>
            </div>
          ) : null}
        </div>

        <div className="card p-0 overflow-hidden xl:col-span-2">
          <div className="px-5 py-4 border-b border-slate-100 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <AlertTriangle size={18} className="text-amber-500" />
              <h2 className="font-semibold text-slate-900">Açık Bakiyeler</h2>
            </div>
            <Link href="/service-orders" className="text-xs text-brand-600 hover:text-brand-700">
              Tümünü gör →
            </Link>
          </div>

          {isBalancesLoading ? (
            <p className="text-sm text-slate-400 py-10 text-center">Açık bakiyeler yükleniyor...</p>
          ) : balancesError ? (
            <div className="py-8 text-center">
              <p className="text-sm text-rose-500">Açık bakiye listesi yüklenemedi.</p>
              <button onClick={() => void refetchBalances()} className="mt-2 text-xs text-brand-600 hover:underline">
                Yeniden dene
              </button>
            </div>
          ) : balanceRows.length === 0 ? (
            <p className="text-sm text-slate-400 py-10 text-center">Açık bakiyesi olan servis emri yok.</p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {balanceRows.slice(0, 5).map((item) => (
                <li key={item.serviceOrderId}>
                  <Link
                    href={`/service-orders/${item.serviceOrderId}`}
                    className="flex items-center justify-between gap-4 px-5 py-3 hover:bg-slate-50 transition-colors"
                  >
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-slate-900 truncate">
                        {item.orderNo} · {item.vehiclePlate ?? 'Plaka yok'}
                      </p>
                      <p className="text-xs text-slate-500 truncate">
                        {item.customerName ?? 'Müşteri bilgisi yok'} · {dateText(item.openedAt)}
                      </p>
                    </div>
                    <div className="text-right shrink-0">
                      <p className="text-sm font-semibold text-amber-700">{money(item.remainingTotal)}</p>
                      <p className="text-xs text-slate-400">{money(item.paidTotal)} ödendi</p>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>

      {/* Monthly Revenue Chart */}
      <div className="card p-5 mb-6">
        <h2 className="font-semibold text-slate-900 mb-4">Aylık Gelir</h2>
        {!appConfig.monthlyDashboardEnabled ? (
          <p className="text-sm text-slate-400 text-center py-8">
            Aylık grafik backend desteği etkinleştirildiğinde burada gösterilecek.
          </p>
        ) : isMonthlyLoading ? (
          <p className="text-sm text-slate-400 text-center py-8">Aylık grafik yükleniyor...</p>
        ) : monthlyError ? (
          <p className="text-sm text-rose-500 text-center py-8">Aylık grafik bilgileri şu anda yüklenemedi.</p>
        ) : monthlyRows.length === 0 ? (
          <p className="text-sm text-slate-400 text-center py-8">Grafik verisi henüz mevcut değil</p>
        ) : (
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={monthlyRows.map((d) => ({ ...d, monthLabel: toTrMonth(d.month) }))} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
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
              {recentOrders.slice(0, 5).map((order, idx) => {
                const orderId = safeStr(order['id'], '');
                const content = (
                  <>
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
                  </>
                );

                return (
                  <li key={orderId || String(idx)}>
                    {orderId ? (
                      <Link
                        href={`/service-orders/${orderId}`}
                        className="flex items-center justify-between px-5 py-3 hover:bg-slate-50 transition-colors"
                      >
                        {content}
                      </Link>
                    ) : (
                      <div className="flex items-center justify-between px-5 py-3">{content}</div>
                    )}
                  </li>
                );
              })}
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
        {canCreateServiceOrder(user?.role) && (
          <Link href="/service-orders/new" className="btn-secondary">Yeni Servis Emri</Link>
        )}
        {canCreateCustomer(user?.role) && (
          <Link href="/customers/create" className="btn-secondary">Yeni Müşteri</Link>
        )}
        {canManageInspection(user?.role) && (
          <Link href="/inspections/new" className="btn-secondary">Yeni Expertiz</Link>
        )}
      </div>
    </div>
  );
}
