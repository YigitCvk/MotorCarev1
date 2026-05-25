'use client';

import { Suspense, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { friendlyError } from '@/core/api/errors';
import { authService } from '@/core/auth/auth.service';
import { acceptInviteSchema, type AcceptInviteFormData } from '@/core/auth/schemas';
import { appConfig } from '@/shared/config/env';

function AcceptInviteInner() {
  const router = useRouter();
  const params = useSearchParams();
  const token = params.get('token') ?? '';

  const [inviteInfo, setInviteInfo] = useState<{ email: string; fullName?: string; role: string } | null>(null);
  const [validating, setValidating] = useState(true);
  const [tokenError, setTokenError] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<AcceptInviteFormData>({
    resolver: zodResolver(acceptInviteSchema),
  });

  useEffect(() => {
    if (!token) {
      setTokenError('Geçersiz davet bağlantısı.');
      setValidating(false);
      return;
    }

    authService.validateInvite(token)
      .then((info) => {
        if (!info.isValid) {
          setTokenError('Bu davet bağlantısı geçersiz veya süresi dolmuş.');
          return;
        }
        setInviteInfo(info);
        if (info.fullName) setValue('fullName', info.fullName);
      })
      .catch((err) => setTokenError(friendlyError(err, 'Davet bağlantısı doğrulanamadı.')))
      .finally(() => setValidating(false));
  }, [token, setValue]);

  async function onSubmit(data: AcceptInviteFormData) {
    setError('');
    try {
      await authService.acceptInvite({
        token,
        fullName: data.fullName,
        password: data.password,
        confirmPassword: data.confirmPassword,
      });
      setSuccess(true);
      toast.success('Hesabınız oluşturuldu. Giriş yapabilirsiniz.');
    } catch (err) {
      const message = friendlyError(err, 'Davet kabul edilemedi. Lütfen tekrar deneyin.');
      setError(message);
      toast.error(message);
    }
  }

  if (validating) {
    return (
      <div className="card p-8 w-full max-w-md shadow-2xl text-center">
        <p className="text-slate-500">Davet doğrulanıyor...</p>
      </div>
    );
  }

  if (tokenError) {
    return (
      <div className="card p-8 w-full max-w-md shadow-2xl text-center">
        <div className="text-4xl mb-4" aria-hidden>!</div>
        <h2 className="text-lg font-bold text-slate-900 mb-2">Geçersiz Davet</h2>
        <p className="text-sm text-slate-600 mb-6">{tokenError}</p>
        <Link href="/login" className="text-sm text-brand-600 hover:text-brand-700">Giriş sayfasına dön</Link>
      </div>
    );
  }

  if (success) {
    return (
      <div className="card p-8 w-full max-w-md shadow-2xl text-center">
        <div className="text-4xl mb-4" aria-hidden>✓</div>
        <h2 className="text-xl font-bold text-slate-900 mb-2">Hesabınız Oluşturuldu</h2>
        <p className="text-sm text-slate-600 mb-6">Giriş yaparak başlayabilirsiniz.</p>
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
      <h1 className="text-2xl font-bold text-slate-900 mb-1">Daveti Kabul Et</h1>
      {inviteInfo && (
        <p className="text-sm text-slate-500 mb-6">
          <strong>{inviteInfo.email}</strong> için hesap oluşturun.
        </p>
      )}

      {error && <div className="alert-error mb-4">{error}</div>}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="form-group">
          <label className="label">Ad Soyad</label>
          <input
            className="input"
            placeholder="Adınız Soyadınız"
            autoComplete="name"
            {...register('fullName')}
          />
          {errors.fullName && (
            <p className="mt-1 text-xs text-red-500">{errors.fullName.message}</p>
          )}
        </div>
        <div className="form-group">
          <label className="label">Şifre</label>
          <input
            type="password"
            className="input"
            placeholder="En az 8 karakter"
            autoComplete="new-password"
            {...register('password')}
          />
          {errors.password && (
            <p className="mt-1 text-xs text-red-500">{errors.password.message}</p>
          )}
        </div>
        <div className="form-group">
          <label className="label">Şifre Tekrar</label>
          <input
            type="password"
            className="input"
            placeholder="Şifrenizi tekrar girin"
            autoComplete="new-password"
            {...register('confirmPassword')}
          />
          {errors.confirmPassword && (
            <p className="mt-1 text-xs text-red-500">{errors.confirmPassword.message}</p>
          )}
        </div>
        <button type="submit" disabled={isSubmitting} className="btn-primary w-full py-2.5">
          {isSubmitting ? 'Oluşturuluyor...' : 'Hesabı Oluştur'}
        </button>
      </form>
    </div>
  );
}

export default function AcceptInvitePage() {
  return (
    <Suspense>
      <AcceptInviteInner />
    </Suspense>
  );
}
