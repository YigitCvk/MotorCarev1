'use client';

import { Suspense, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { friendlyError } from '@/core/api/errors';
import { authService } from '@/core/auth/auth.service';
import { resetPasswordSchema, type ResetPasswordFormData } from '@/core/auth/schemas';
import { appConfig } from '@/shared/config/env';

function ResetPasswordInner() {
  const router = useRouter();
  const params = useSearchParams();
  const tenant = params.get('tenant') ?? '';
  const email = params.get('email') ?? '';

  const [error, setError] = useState(!tenant || !email ? 'Şifre sıfırlamak için işletme kodu ve e-posta bilgisi gerekli.' : '');
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  });

  async function onSubmit(data: ResetPasswordFormData) {
    if (!tenant || !email) return;
    setError('');
    try {
      await authService.resetPassword({
        tenantIdentifier: tenant,
        email,
        code: data.code,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      });
      setSuccess(true);
      toast.success('Şifreniz başarıyla güncellendi.');
    } catch (err) {
      const message = friendlyError(err, 'Şifre sıfırlanamadı. Kodu kontrol edip tekrar deneyin.');
      setError(message);
      toast.error(message);
    }
  }

  if (success) {
    return (
      <div className="card p-8 w-full max-w-md shadow-2xl text-center">
        <div className="flex items-center justify-center gap-2 mb-6">
          <div className="h-9 w-9 rounded-xl bg-brand-600 flex items-center justify-center shadow">
            <span className="text-white font-bold">B</span>
          </div>
          <span className="font-bold text-xl text-slate-900">{appConfig.appName}</span>
        </div>
        <div className="text-5xl mb-4" aria-hidden>✓</div>
        <h2 className="text-xl font-bold text-slate-900 mb-2">Şifre Güncellendi</h2>
        <p className="text-sm text-slate-600 mb-6">Yeni şifrenizle giriş yapabilirsiniz.</p>
        <button type="button" onClick={() => router.push('/login')} className="btn-primary w-full">Giriş Yap</button>
      </div>
    );
  }

  return (
    <div className="card p-8 w-full max-w-md shadow-2xl">
      <div className="flex items-center gap-2 mb-8">
        <div className="h-9 w-9 rounded-xl bg-brand-600 flex items-center justify-center shadow">
          <span className="text-white font-bold">B</span>
        </div>
        <span className="font-bold text-xl text-slate-900">{appConfig.appName}</span>
      </div>
      <h1 className="text-2xl font-bold text-slate-900 mb-1">Yeni Şifre Belirle</h1>
      <p className="text-sm text-slate-500 mb-6">E-postanıza gönderilen kodu ve yeni şifrenizi girin.</p>

      {error && <div className="alert-error mb-4">{error}</div>}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="form-group">
          <label className="label">Sıfırlama Kodu</label>
          <input
            className="input"
            placeholder="6 haneli kod"
            inputMode="numeric"
            maxLength={6}
            {...register('code')}
          />
          {errors.code && (
            <p className="mt-1 text-xs text-red-500">{errors.code.message}</p>
          )}
        </div>
        <div className="form-group">
          <label className="label">Yeni Şifre</label>
          <input
            type="password"
            className="input"
            placeholder="En az 8 karakter"
            autoComplete="new-password"
            {...register('newPassword')}
          />
          {errors.newPassword && (
            <p className="mt-1 text-xs text-red-500">{errors.newPassword.message}</p>
          )}
        </div>
        <div className="form-group">
          <label className="label">Şifre Tekrar</label>
          <input
            type="password"
            className="input"
            placeholder="Şifreyi tekrar girin"
            autoComplete="new-password"
            {...register('confirmPassword')}
          />
          {errors.confirmPassword && (
            <p className="mt-1 text-xs text-red-500">{errors.confirmPassword.message}</p>
          )}
        </div>
        <button type="submit" disabled={isSubmitting || !tenant || !email} className="btn-primary w-full py-2.5">
          {isSubmitting ? 'Kaydediliyor...' : 'Şifreyi Güncelle'}
        </button>
      </form>
      <p className="mt-4 text-center text-sm">
        <Link href="/login" className="text-brand-600 hover:text-brand-700">Giriş sayfasına dön</Link>
      </p>
    </div>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense>
      <ResetPasswordInner />
    </Suspense>
  );
}
