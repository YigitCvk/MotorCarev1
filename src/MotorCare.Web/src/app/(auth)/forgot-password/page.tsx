'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { friendlyError } from '@/core/api/errors';
import { authService } from '@/core/auth/auth.service';
import { forgotPasswordSchema, type ForgotPasswordFormData } from '@/core/auth/schemas';
import { appConfig } from '@/shared/config/env';

export default function ForgotPasswordPage() {
  const router = useRouter();
  const [error, setError] = useState('');
  const [sent, setSent] = useState(false);
  const [submittedData, setSubmittedData] = useState<ForgotPasswordFormData | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  async function onSubmit(data: ForgotPasswordFormData) {
    setError('');
    try {
      await authService.forgotPassword({ email: data.email, tenantIdentifier: data.tenantIdentifier });
      setSubmittedData(data);
      setSent(true);
      toast.success('Sıfırlama kodu e-postanıza gönderildi.');
    } catch (err) {
      const message = friendlyError(err, 'İstek gönderilemedi. Lütfen tekrar deneyin.');
      setError(message);
      toast.error(message);
    }
  }

  if (sent && submittedData) {
    return (
      <div className="card p-8 w-full max-w-md shadow-2xl text-center">
        <div className="flex items-center justify-center gap-2 mb-6">
          <div className="h-9 w-9 rounded-xl bg-brand-600 flex items-center justify-center shadow">
            <span className="text-white font-bold">B</span>
          </div>
          <span className="font-bold text-xl text-slate-900">{appConfig.appName}</span>
        </div>
        <div className="text-5xl mb-4" aria-hidden>✉</div>
        <h2 className="text-xl font-bold text-slate-900 mb-2">E-posta Gönderildi</h2>
        <p className="text-sm text-slate-600 mb-2">
          Şifre sıfırlama kodu <strong>{submittedData.email}</strong> adresine gönderildi.
        </p>
        <p className="text-xs text-slate-400 mb-6">Spam klasörünü de kontrol edin.</p>
        <button
          type="button"
          onClick={() =>
            router.push(
              `/reset-password?tenant=${encodeURIComponent(submittedData.tenantIdentifier)}&email=${encodeURIComponent(submittedData.email)}`,
            )
          }
          className="btn-primary w-full"
        >
          Kodu Gir
        </button>
        <button
          type="button"
          onClick={() => setSent(false)}
          className="mt-3 text-sm text-slate-500 hover:text-slate-700 w-full"
        >
          Farklı bir adres deneyin
        </button>
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
      <h1 className="text-2xl font-bold text-slate-900 mb-1">Şifremi Unuttum</h1>
      <p className="text-sm text-slate-500 mb-6">E-posta adresinize sıfırlama kodu göndereceğiz.</p>

      {error && <div className="alert-error mb-4">{error}</div>}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="form-group">
          <label className="label">İşletme Kodu</label>
          <input
            className="input"
            placeholder="isletme-kodunuz"
            autoComplete="organization"
            {...register('tenantIdentifier')}
          />
          {errors.tenantIdentifier && (
            <p className="mt-1 text-xs text-red-500">{errors.tenantIdentifier.message}</p>
          )}
        </div>
        <div className="form-group">
          <label className="label">E-posta</label>
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
        <button type="submit" disabled={isSubmitting} className="btn-primary w-full py-2.5">
          {isSubmitting ? 'Gönderiliyor...' : 'Sıfırlama Kodu Gönder'}
        </button>
      </form>
      <p className="mt-4 text-center text-sm">
        <Link href="/login" className="text-brand-600 hover:text-brand-700">Giriş sayfasına dön</Link>
      </p>
    </div>
  );
}
