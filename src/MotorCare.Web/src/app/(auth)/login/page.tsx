'use client';

import { type ChangeEvent, type FormEvent, Suspense, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { toast } from 'sonner';
import { friendlyLoginError } from '@/core/api/errors';
import { useAuth } from '@/core/auth/auth.context';
import { authService, sanitizeAuthRedirect } from '@/core/auth/auth.service';
import { loginSchema, type LoginFormData } from '@/core/auth/schemas';
import { appConfig } from '@/shared/config/env';

function LoginForm() {
  const { login } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectTo = sanitizeAuthRedirect(searchParams.get('from'));
  const [formData, setFormData] = useState<LoginFormData>({
    tenantIdentifier: '',
    email: '',
    password: '',
  });
  const [errors, setErrors] = useState<Partial<Record<keyof LoginFormData, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  function onFieldChange(event: ChangeEvent<HTMLInputElement>) {
    const { name, value } = event.target;
    setFormData((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const submittedData: LoginFormData = {
      tenantIdentifier: formValue(event.currentTarget, 'tenantIdentifier'),
      email: formValue(event.currentTarget, 'email'),
      password: formValue(event.currentTarget, 'password'),
    };
    setFormData(submittedData);

    const parsed = loginSchema.safeParse(submittedData);
    if (!parsed.success) {
      const nextErrors: Partial<Record<keyof LoginFormData, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof LoginFormData | undefined;
        if (field && !nextErrors[field]) nextErrors[field] = issue.message;
      }
      setErrors(nextErrors);
      return;
    }

    setIsSubmitting(true);
    try {
      const response = await login(parsed.data);
      if (response.requiresTwoFactor) {
        const ticket = response.twoFactorToken?.trim();
        if (!ticket) {
          toast.error('İki faktörlü doğrulama oturumu başlatılamadı. Lütfen tekrar deneyin.');
          return;
        }

        const params = new URLSearchParams({ ticket });
        if (redirectTo) params.set('from', redirectTo);
        router.push(`/two-factor?${params.toString()}`);
        return;
      }
      router.replace(redirectTo ?? authService.roleLanding(response.role));
    } catch (err) {
      toast.error(friendlyLoginError(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  function formValue(form: HTMLFormElement, name: keyof LoginFormData): string {
    const value = new FormData(form).get(name);
    return typeof value === 'string' ? value : '';
  }

  return (
    <div className="card p-8 w-full max-w-md shadow-2xl">
      <div className="flex items-center gap-2 mb-8">
        <div className="h-9 w-9 rounded-xl bg-brand-600 flex items-center justify-center shadow">
          <span className="text-white font-bold">B</span>
        </div>
        <span className="font-bold text-xl text-slate-900">{appConfig.appName}</span>
      </div>

      <h1 className="text-2xl font-bold text-slate-900 mb-1">Giriş Yap</h1>
      <p className="text-sm text-slate-500 mb-6">İşletme hesabınıza giriş yapın.</p>

      <form onSubmit={onSubmit} className="space-y-4">
        <div className="form-group">
          <label className="label">İşletme Kodu <span className="text-red-500">*</span></label>
          <input
            type="text"
            className="input"
            placeholder="ornek-garaj"
            autoComplete="organization"
            name="tenantIdentifier"
            value={formData.tenantIdentifier}
            onChange={onFieldChange}
          />
          {errors.tenantIdentifier && (
            <p className="mt-1 text-xs text-red-500">{errors.tenantIdentifier}</p>
          )}
        </div>

        <div className="form-group">
          <label className="label">E-posta <span className="text-red-500">*</span></label>
          <input
            type="email"
            className="input"
            placeholder="isim@sirket.com"
            autoComplete="email"
            name="email"
            value={formData.email}
            onChange={onFieldChange}
          />
          {errors.email && (
            <p className="mt-1 text-xs text-red-500">{errors.email}</p>
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
            name="password"
            value={formData.password}
            onChange={onFieldChange}
          />
          {errors.password && (
            <p className="mt-1 text-xs text-red-500">{errors.password}</p>
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
