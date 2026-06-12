'use client';

import { Suspense, useMemo, useState } from 'react';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { zodResolver } from '@hookform/resolvers/zod';
import { ShieldCheck } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { z } from 'zod';
import { useAuth } from '@/core/auth/auth.context';
import { authService, sanitizeAuthRedirect } from '@/core/auth/auth.service';
import { friendlyError } from '@/core/api/errors';

const twoFactorSchema = z.object({
  code: z
    .string()
    .trim()
    .length(6, 'Kod 6 haneli olmalıdır.')
    .regex(/^\d+$/, 'Kod yalnızca rakamlardan oluşmalıdır.'),
});

type TwoFactorFormData = z.infer<typeof twoFactorSchema>;

function TwoFactorForm(): React.ReactElement {
  const { refreshUser } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const ticket = searchParams.get('ticket') ?? '';
  const redirectTo = sanitizeAuthRedirect(searchParams.get('from'));
  const [resending, setResending] = useState(false);
  const [formError, setFormError] = useState('');

  const hasTicket = useMemo(() => ticket.trim().length > 0, [ticket]);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<TwoFactorFormData>({
    resolver: zodResolver(twoFactorSchema),
    defaultValues: { code: '' },
  });

  async function onSubmit(values: TwoFactorFormData): Promise<void> {
    if (!hasTicket) {
      setFormError('Doğrulama oturumu bulunamadı. Lütfen tekrar giriş yapın.');
      return;
    }

    setFormError('');
    try {
      const user = await authService.verifyTwoFactor({ ticket, code: values.code });
      await refreshUser();
      toast.success('Giriş doğrulandı');
      router.replace(redirectTo ?? authService.roleLanding(user?.role));
    } catch (err) {
      const message = friendlyError(err, 'Doğrulama kodu onaylanamadı. Lütfen tekrar deneyin.');
      setFormError(message);
      toast.error(message);
    }
  }

  async function resendCode(): Promise<void> {
    if (!hasTicket) {
      setFormError('Doğrulama oturumu bulunamadı. Lütfen tekrar giriş yapın.');
      return;
    }

    setResending(true);
    setFormError('');
    try {
      await authService.resendTwoFactorCode({ ticket });
      toast.success('Yeni doğrulama kodu gönderildi');
    } catch (err) {
      const message = friendlyError(err, 'Yeni doğrulama kodu gönderilemedi.');
      setFormError(message);
      toast.error(message);
    } finally {
      setResending(false);
    }
  }

  return (
    <div className="card w-full max-w-md p-8 shadow-2xl">
      <div className="mb-6 flex h-12 w-12 items-center justify-center rounded-xl bg-brand-50 text-brand-600">
        <ShieldCheck size={24} />
      </div>

      <h1 className="mb-1 text-2xl font-bold text-slate-900">İki Adımlı Doğrulama</h1>
      <p className="mb-6 text-sm text-slate-500">
        E-postanıza gönderilen 6 haneli güvenlik kodunu girin.
      </p>

      {!hasTicket && (
        <div className="mb-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          Doğrulama oturumu bulunamadı. Giriş ekranından tekrar deneyin.
        </div>
      )}

      {formError && (
        <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {formError}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="form-group">
          <label className="label">
            Doğrulama Kodu <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            inputMode="numeric"
            autoComplete="one-time-code"
            maxLength={6}
            className={`input text-center text-lg tracking-[0.4em] ${errors.code ? 'input-error' : ''}`}
            {...register('code')}
          />
          {errors.code && <p className="error-text">{errors.code.message}</p>}
        </div>

        <button type="submit" disabled={isSubmitting || !hasTicket} className="btn-primary w-full py-2.5 text-base">
          {isSubmitting ? 'Doğrulanıyor...' : 'Doğrula ve Giriş Yap'}
        </button>
      </form>

      <div className="mt-6 flex flex-col gap-3 text-center text-sm">
        <button
          type="button"
          onClick={resendCode}
          disabled={resending || !hasTicket}
          className="font-medium text-brand-600 hover:text-brand-700 disabled:cursor-not-allowed disabled:text-slate-400"
        >
          {resending ? 'Kod gönderiliyor...' : 'Kodu tekrar gönder'}
        </button>
        <Link
          href={redirectTo ? `/login?from=${encodeURIComponent(redirectTo)}` : '/login'}
          className="text-slate-500 hover:text-slate-700"
        >
          Giriş ekranına dön
        </Link>
      </div>
    </div>
  );
}

export default function TwoFactorPage(): React.ReactElement {
  return (
    <Suspense>
      <TwoFactorForm />
    </Suspense>
  );
}
