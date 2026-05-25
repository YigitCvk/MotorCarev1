'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { authService } from '@/core/auth/auth.service';
import { friendlyError } from '@/core/api/errors';
import { registerSchema, type RegisterFormData } from '@/core/auth/schemas';

export default function RegisterPage() {
  const router = useRouter();
  const [error, setError] = useState('');

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  async function onSubmit(data: RegisterFormData) {
    setError('');
    try {
      await authService.register({
        tenantIdentifier: data.tenantIdentifier,
        tenantName: data.tenantName,
        ownerFullName: data.fullName,
        ownerEmail: data.email,
        ownerPassword: data.password,
      });
      toast.success('Hesap oluşturuldu! E-postanızı doğrulayın.');
      router.push(
        `/verify-email?tenant=${encodeURIComponent(data.tenantIdentifier)}&email=${encodeURIComponent(data.email)}`,
      );
    } catch (err) {
      const message = friendlyError(err, 'İşletme oluşturulamadı. Lütfen tekrar deneyin.');
      setError(message);
      toast.error(message);
    }
  }

  return (
    <div className="card p-8 w-full max-w-md shadow-2xl">
      <div className="flex items-center gap-2 mb-8">
        <div className="h-9 w-9 rounded-xl bg-brand-600 flex items-center justify-center shadow">
          <span className="text-white font-bold">B</span>
        </div>
        <span className="font-bold text-xl text-slate-900">BakımSuite</span>
      </div>

      <h1 className="text-2xl font-bold text-slate-900 mb-1">İşletme Oluştur</h1>
      <p className="text-sm text-slate-500 mb-6">Yeni bir servis hesabı açın.</p>

      {error && <div className="alert-error mb-5">{error}</div>}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="form-group">
          <label className="label">İşletme Kodu <span className="text-red-500">*</span></label>
          <input
            className="input"
            placeholder="kucuk-harf-tire-izin-veriliyor"
            {...register('tenantIdentifier')}
          />
          <p className="text-xs text-slate-400 mt-1">Giriş yaparken kullanılır. Sonradan değiştirilemez.</p>
          {errors.tenantIdentifier && (
            <p className="mt-1 text-xs text-red-500">{errors.tenantIdentifier.message}</p>
          )}
        </div>

        <div className="form-group">
          <label className="label">İşletme Adı <span className="text-red-500">*</span></label>
          <input
            className="input"
            placeholder="Örnek Oto Servis"
            {...register('tenantName')}
          />
          {errors.tenantName && (
            <p className="mt-1 text-xs text-red-500">{errors.tenantName.message}</p>
          )}
        </div>

        <div className="form-group">
          <label className="label">Yetkili Adı Soyadı <span className="text-red-500">*</span></label>
          <input
            className="input"
            placeholder="Ahmet Yılmaz"
            {...register('fullName')}
          />
          {errors.fullName && (
            <p className="mt-1 text-xs text-red-500">{errors.fullName.message}</p>
          )}
        </div>

        <div className="form-group">
          <label className="label">E-posta <span className="text-red-500">*</span></label>
          <input
            type="email"
            className="input"
            placeholder="ahmet@ornekservis.com"
            {...register('email')}
          />
          {errors.email && (
            <p className="mt-1 text-xs text-red-500">{errors.email.message}</p>
          )}
        </div>

        <div className="form-group">
          <label className="label">Şifre <span className="text-red-500">*</span></label>
          <input
            type="password"
            className="input"
            placeholder="En az 8 karakter"
            {...register('password')}
          />
          {errors.password && (
            <p className="mt-1 text-xs text-red-500">{errors.password.message}</p>
          )}
        </div>

        <div className="form-group">
          <label className="label">Şifre Tekrar <span className="text-red-500">*</span></label>
          <input
            type="password"
            className="input"
            placeholder="Şifreyi tekrar girin"
            {...register('confirmPassword')}
          />
          {errors.confirmPassword && (
            <p className="mt-1 text-xs text-red-500">{errors.confirmPassword.message}</p>
          )}
        </div>

        <button type="submit" disabled={isSubmitting} className="btn-primary w-full py-2.5 text-base">
          {isSubmitting ? 'Oluşturuluyor...' : 'Hesap Oluştur'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-slate-500">
        Hesabınız var mı?{' '}
        <Link href="/login" className="text-brand-600 font-medium hover:text-brand-700">Giriş yapın</Link>
      </p>
    </div>
  );
}
