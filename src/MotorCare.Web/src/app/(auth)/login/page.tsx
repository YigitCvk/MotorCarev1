'use client';

import { Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { useAuth } from '@/core/auth/auth.context';
import { friendlyError } from '@/core/api/errors';
import { loginSchema, type LoginFormData } from '@/core/auth/schemas';

function LoginForm() {
  const { login } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectTo = searchParams.get('from') ?? undefined;

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  async function onSubmit(data: LoginFormData) {
    try {
      const response = await login(data);
      if (response.requiresTwoFactor) {
        router.push(`/two-factor?ticket=${encodeURIComponent(response.twoFactorToken ?? '')}`);
        return;
      }
      const landing = (() => {
        switch (response.role) {
          case 'Technician': return '/service-orders';
          case 'Inspector': return '/inspections';
          default: return '/dashboard';
        }
      })();
      router.replace(redirectTo ?? landing);
    } catch (err) {
      const message = friendlyError(err, 'Giriş yapılamadı. Bilgileri kontrol edip tekrar deneyin.');
      toast.error(message);
    }
  }

  return (
    <div className="card p-8 w-full max-w-md shadow-2xl">
      {/* Logo */}
      <div className="flex items-center gap-2 mb-8">
        <div className="h-9 w-9 rounded-xl bg-brand-600 flex items-center justify-center shadow">
          <span className="text-white font-bold">B</span>
        </div>
        <span className="font-bold text-xl text-slate-900">BakımSuite</span>
      </div>

      <h1 className="text-2xl font-bold text-slate-900 mb-1">Giriş Yap</h1>
      <p className="text-sm text-slate-500 mb-6">İşletme hesabınıza giriş yapın.</p>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="form-group">
          <label className="label">İşletme Kodu <span className="text-red-500">*</span></label>
          <input
            type="text"
            className="input"
            placeholder="ornek-garaj"
            autoComplete="organization"
            {...register('tenantIdentifier')}
          />
          {errors.tenantIdentifier && (
            <p className="mt-1 text-xs text-red-500">{errors.tenantIdentifier.message}</p>
          )}
        </div>

        <div className="form-group">
          <label className="label">E-posta <span className="text-red-500">*</span></label>
          <input
            type="email"
            className="input"
            placeholder="isim@sirket.com"
            autoComplete="email"
            {...register('email')}
          />
          {errors.email && (
            <p className="mt-1 text-xs text-red-500">{errors.email.message}</p>
          )}
        </div>

        <div className="form-group">
          <div className="flex items-center justify-between mb-1">
            <label className="label mb-0">Şifre <span className="text-red-500">*</span></label>
            <Link href="/forgot-password" className="text-xs text-brand-600 hover:text-brand-700">
              Şifremi unuttum
            </Link>
          </div>
          <input
            type="password"
            className="input"
            autoComplete="current-password"
            {...register('password')}
          />
          {errors.password && (
            <p className="mt-1 text-xs text-red-500">{errors.password.message}</p>
          )}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="btn-primary w-full py-2.5 text-base"
        >
          {isSubmitting ? 'Giriş yapılıyor...' : 'Giriş Yap'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-slate-500">
        Hesabınız yok mu?{' '}
        <Link href="/register" className="text-brand-600 font-medium hover:text-brand-700">
          İşletme oluşturun
        </Link>
      </p>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense>
      <LoginForm />
    </Suspense>
  );
}
