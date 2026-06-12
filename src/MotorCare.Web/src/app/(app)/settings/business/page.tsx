'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import { Building2, Save, AlertCircle } from 'lucide-react';
import apiClient from '@/core/api/client';
import { useAuth } from '@/core/auth/auth.context';
import { friendlyError } from '@/core/api/errors';
import { PageLoading } from '@/components/ui/loading';

interface TenantProfileDto {
  id: string;
  identifier: string;
  name: string;
  legalName: string | null;
  taxNumber: string | null;
  taxOffice: string | null;
  address: string | null;
  phone: string | null;
  email: string | null;
  website: string | null;
  logoUrl: string | null;
}

const businessSchema = z.object({
  name: z.string().min(2, 'İşletme adı en az 2 karakter olmalıdır'),
  legalName: z.string().optional(),
  taxNumber: z.string().optional(),
  taxOffice: z.string().optional(),
  address: z.string().optional(),
  phone: z.string().optional(),
  email: z.string().email('Geçerli bir e-posta girin').optional().or(z.literal('')),
  website: z.string().url('Geçerli bir URL girin').optional().or(z.literal('')),
});

type BusinessFormValues = z.infer<typeof businessSchema>;

function toBusinessFormValues(profile: TenantProfileDto): BusinessFormValues {
  return {
    name: profile.name ?? '',
    legalName: profile.legalName ?? '',
    taxNumber: profile.taxNumber ?? '',
    taxOffice: profile.taxOffice ?? '',
    phone: profile.phone ?? '',
    email: profile.email ?? '',
    address: profile.address ?? '',
    website: profile.website ?? '',
  };
}

function SettingsNav() {
  const pathname = usePathname();
  const { user } = useAuth();
  const tabs = [
    ...(user?.role === 'Owner'
      ? [{ label: 'İşletme Bilgileri', href: '/settings/business' }]
      : []),
    ...(user?.role === 'Owner' || user?.role === 'Admin'
      ? [{ label: 'Kullanıcılar', href: '/settings/users' }]
      : []),
    { label: 'Güvenlik', href: '/settings/security' },
  ];
  return (
    <div className="flex gap-1 border-b border-slate-200 mb-6">
      {tabs.map((tab) => {
        const active = pathname === tab.href;
        return (
          <Link
            key={tab.href}
            href={tab.href}
            className={[
              'px-4 py-2 text-sm font-medium rounded-t border-b-2 transition-colors',
              active
                ? 'border-blue-600 text-blue-600 bg-blue-50'
                : 'border-transparent text-slate-500 hover:text-slate-700 hover:bg-slate-50',
            ].join(' ')}
          >
            {tab.label}
          </Link>
        );
      })}
    </div>
  );
}

export default function SettingsBusinessPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<BusinessFormValues>({
    resolver: zodResolver(businessSchema),
    defaultValues: {
      name: '',
      legalName: '',
      taxNumber: '',
      taxOffice: '',
      phone: '',
      email: '',
      address: '',
      website: '',
    },
  });

  const { data, isLoading, error: loadError } = useQuery<TenantProfileDto>({
    queryKey: ['tenant-profile'],
    queryFn: async () => {
      const { data } = await apiClient.get<TenantProfileDto>('/api/tenants/current/profile');
      return data;
    },
    enabled: user?.role === 'Owner',
  });

  useEffect(() => {
    if (data) {
      reset(toBusinessFormValues(data));
    }
  }, [data, reset]);

  const mutation = useMutation({
    mutationFn: async (payload: BusinessFormValues) => {
      const response = await apiClient.put<TenantProfileDto>('/api/tenants/current/profile', {
        ...payload,
        logoUrl: data?.logoUrl ?? null,
      });
      return response.data;
    },
    onSuccess: (updated) => {
      queryClient.setQueryData(['tenant-profile'], updated);
      reset(toBusinessFormValues(updated));
      toast.success('İşletme profili güncellendi');
    },
    onError: (err: unknown) => {
      toast.error(friendlyError(err));
    },
  });

  // Access guard
  if (user && user.role !== 'Owner') {
    return (
      <div>
        <SettingsNav />
        <div className="alert-error flex items-center gap-2 max-w-xl">
          <AlertCircle size={16} className="shrink-0" />
          Bu sayfayı görüntülemek için yetkiniz yok.
        </div>
      </div>
    );
  }

  return (
    <div>
      <SettingsNav />

      <div className="page-header mb-6">
        <div className="flex items-center gap-2">
          <Building2 size={20} className="text-slate-500" />
          <div>
            <h1 className="page-title">İşletme Bilgileri</h1>
            <p className="page-subtitle">Müşterilere ve kayıtlarda görünen işletme bilgilerinizi yönetin.</p>
          </div>
        </div>
      </div>

      {loadError && (
        <div className="alert-error mb-4 flex items-center gap-2">
          <AlertCircle size={16} className="shrink-0" />
          İşletme bilgileri yüklenemedi.
        </div>
      )}

      <div className="card p-6 max-w-2xl">
        {isLoading ? (
          <PageLoading />
        ) : (
          <form onSubmit={handleSubmit((values) => mutation.mutate(values))} className="space-y-5">
            {/* Business Identity */}
            <div className="grid sm:grid-cols-2 gap-4">
              <div className="form-group sm:col-span-2">
                <label className="label" htmlFor="name">
                  İşletme Adı <span className="text-red-500">*</span>
                </label>
                <input
                  id="name"
                  type="text"
                  className="input"
                  placeholder="Müşterilere gösterilen işletme adı"
                  {...register('name')}
                />
                {errors.name && (
                  <p className="error-text mt-1">{errors.name.message}</p>
                )}
              </div>

              <div className="form-group">
                <label className="label" htmlFor="legalName">Yasal Ünvan</label>
                <input
                  id="legalName"
                  type="text"
                  className="input"
                  placeholder="Resmi ticaret ünvanı"
                  {...register('legalName')}
                />
              </div>

              <div className="form-group">
                <label className="label" htmlFor="phone">Telefon</label>
                <input
                  id="phone"
                  type="tel"
                  className="input"
                  placeholder="0212 000 0000"
                  {...register('phone')}
                />
              </div>

              <div className="form-group">
                <label className="label" htmlFor="taxNumber">Vergi No</label>
                <input
                  id="taxNumber"
                  type="text"
                  className="input"
                  placeholder="1234567890"
                  {...register('taxNumber')}
                />
              </div>

              <div className="form-group">
                <label className="label" htmlFor="taxOffice">Vergi Dairesi</label>
                <input
                  id="taxOffice"
                  type="text"
                  className="input"
                  placeholder="Kadıköy V.D."
                  {...register('taxOffice')}
                />
              </div>

              <div className="form-group">
                <label className="label" htmlFor="email">E-posta</label>
                <input
                  id="email"
                  type="email"
                  className="input"
                  placeholder="info@isletme.com"
                  {...register('email')}
                />
                {errors.email && (
                  <p className="error-text mt-1">{errors.email.message}</p>
                )}
              </div>

              <div className="form-group">
                <label className="label" htmlFor="website">Web Sitesi</label>
                <input
                  id="website"
                  type="url"
                  className="input"
                  placeholder="https://isletme.com"
                  {...register('website')}
                />
                {errors.website && (
                  <p className="error-text mt-1">{errors.website.message}</p>
                )}
              </div>

              <div className="form-group sm:col-span-2">
                <label className="label" htmlFor="address">Adres</label>
                <textarea
                  id="address"
                  className="input"
                  rows={2}
                  placeholder="Cadde, sokak, ilçe, şehir"
                  {...register('address')}
                />
              </div>

            </div>

            <div className="flex gap-3 pt-2">
              <button
                type="submit"
                disabled={mutation.isPending}
                className="btn btn-primary flex items-center gap-2"
              >
                <Save size={15} />
                {mutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
