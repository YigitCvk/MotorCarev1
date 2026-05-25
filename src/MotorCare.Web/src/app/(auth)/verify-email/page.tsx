'use client';

import { Suspense, useEffect, useRef, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { toast } from 'sonner';
import { friendlyError } from '@/core/api/errors';
import { authService } from '@/core/auth/auth.service';

function VerifyEmailInner() {
  const router = useRouter();
  const params = useSearchParams();
  const tenant = params.get('tenant') ?? '';
  const email = params.get('email') ?? '';

  const [codes, setCodes] = useState(['', '', '', '', '', '']);
  const refs = useRef<(HTMLInputElement | null)[]>([]);
  const [loading, setLoading] = useState(false);
  const [resending, setResending] = useState(false);
  const [error, setError] = useState(!tenant || !email ? 'Doğrulama için işletme kodu ve e-posta bilgisi gerekli.' : '');
  const [success, setSuccess] = useState('');
  const [countdown, setCountdown] = useState(0);

  useEffect(() => {
    if (countdown > 0) {
      const t = setTimeout(() => setCountdown((c) => c - 1), 1000);
      return () => clearTimeout(t);
    }
  }, [countdown]);

  function handleCodeInput(idx: number, val: string) {
    const digit = val.replace(/\D/g, '').slice(-1);
    const newCodes = [...codes];
    newCodes[idx] = digit;
    setCodes(newCodes);
    if (digit && idx < 5) refs.current[idx + 1]?.focus();
    if (newCodes.every((c) => c)) {
      void submitCode(newCodes.join(''));
    }
  }

  function handleKeyDown(idx: number, e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Backspace' && !codes[idx] && idx > 0) {
      refs.current[idx - 1]?.focus();
    }
  }

  async function submitCode(code: string) {
    if (code.length < 6 || !tenant || !email) return;
    setLoading(true);
    setError('');
    try {
      await authService.verifyEmail({ tenantIdentifier: tenant, email, code });
      const message = 'E-posta doğrulandı. Giriş sayfasına yönlendiriliyorsunuz.';
      setSuccess(message);
      toast.success(message);
      setTimeout(() => router.push('/login'), 1200);
    } catch (err) {
      const message = friendlyError(err, 'Doğrulama kodu hatalı. Lütfen tekrar deneyin.');
      setError(message);
      toast.error(message);
      setCodes(['', '', '', '', '', '']);
      refs.current[0]?.focus();
    } finally {
      setLoading(false);
    }
  }

  async function resendCode() {
    if (!email || !tenant) return;
    setResending(true);
    setError('');
    try {
      await authService.resendVerificationCode({ email, tenantIdentifier: tenant });
      const message = 'Yeni doğrulama kodu gönderildi.';
      setSuccess(message);
      toast.success(message);
      setCountdown(60);
    } catch (err) {
      const message = friendlyError(err, 'Kod gönderilemedi.');
      setError(message);
      toast.error(message);
    } finally {
      setResending(false);
    }
  }

  return (
    <div className="card p-8 w-full max-w-md shadow-2xl text-center">
      <div className="h-16 w-16 rounded-full bg-brand-50 flex items-center justify-center mx-auto mb-6">
        <span className="text-3xl" aria-hidden>✉</span>
      </div>
      <h1 className="text-2xl font-bold text-slate-900 mb-2">E-posta Doğrulama</h1>
      <p className="text-sm text-slate-500 mb-2">
        <strong>{email || 'e-posta adresinize'}</strong> için gönderilen 6 haneli doğrulama kodunu girin.
      </p>
      <p className="text-xs text-slate-400 mb-6">Kodu bulamıyor musunuz? Spam klasörünü kontrol edin.</p>

      {error && <div className="alert-error mb-4 text-left">{error}</div>}
      {success && <div className="alert-success mb-4 text-left">{success}</div>}

      <div className="flex gap-2 justify-center mb-6">
        {codes.map((digit, idx) => (
          <input
            key={idx}
            ref={(el) => { refs.current[idx] = el; }}
            type="text"
            inputMode="numeric"
            maxLength={1}
            value={digit}
            onChange={(e) => handleCodeInput(idx, e.target.value)}
            onKeyDown={(e) => handleKeyDown(idx, e)}
            disabled={loading || !tenant || !email}
            className="w-11 h-14 text-center text-xl font-bold border-2 rounded-xl border-slate-200 focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500 disabled:opacity-50"
          />
        ))}
      </div>

      {loading && <p className="text-sm text-slate-500 mb-4">Doğrulanıyor...</p>}

      <button
        type="button"
        onClick={resendCode}
        disabled={resending || countdown > 0 || !tenant || !email}
        className="text-sm text-brand-600 hover:text-brand-700 disabled:opacity-50"
      >
        {resending ? 'Gönderiliyor...' : countdown > 0 ? `Tekrar gönder (${countdown}s)` : 'Kodu tekrar gönder'}
      </button>

      <p className="mt-4 text-sm">
        <Link href="/login" className="text-slate-500 hover:text-slate-700">Giriş sayfasına dön</Link>
      </p>
    </div>
  );
}

export default function VerifyEmailPage() {
  return (
    <Suspense>
      <VerifyEmailInner />
    </Suspense>
  );
}
