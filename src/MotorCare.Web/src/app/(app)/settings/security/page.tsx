'use client';

import { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ShieldCheck, ShieldOff, KeyRound, AlertCircle } from 'lucide-react';
import apiClient from '@/core/api/client';
import { useAuth } from '@/core/auth/auth.context';
import { friendlyError } from '@/core/api/errors';
import { PageLoading } from '@/components/ui/loading';

interface SecurityStatus {
  email: string;
  isEmailVerified: boolean;
  twoFactorEnabled: boolean;
  twoFactorProvider: string | null;
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

type TwoFactorStep = 'idle' | 'code-sent';

export default function SecuritySettingsPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  const [enableStep, setEnableStep] = useState<TwoFactorStep>('idle');
  const [disableStep, setDisableStep] = useState<TwoFactorStep>('idle');
  const [enableCode, setEnableCode] = useState('');
  const [disableCode, setDisableCode] = useState('');

  const { data: status, isLoading, error, refetch } = useQuery<SecurityStatus>({
    queryKey: ['security-status'],
    queryFn: async () => {
      const { data } = await apiClient.get<SecurityStatus>('/api/auth/security-status');
      return data;
    },
    enabled: !!user,
  });

  const sendEnableCode = useMutation({
    mutationFn: () => apiClient.post('/api/auth/2fa/enable/send-code'),
    onSuccess: () => {
      setEnableStep('code-sent');
      setEnableCode('');
      toast.success('Doğrulama kodu e-postanıza gönderildi.');
    },
    onError: (err: unknown) => toast.error(friendlyError(err)),
  });

  const confirmEnable = useMutation({
    mutationFn: () => apiClient.post('/api/auth/2fa/enable/confirm', { code: enableCode }),
    onSuccess: () => {
      setEnableStep('idle');
      setEnableCode('');
      queryClient.setQueryData<SecurityStatus>(['security-status'], (current) => (
        current
          ? { ...current, twoFactorEnabled: true, twoFactorProvider: 'Email' }
          : current
      ));
      toast.success('İki faktörlü doğrulama etkinleştirildi.');
    },
    onError: (err: unknown) => toast.error(friendlyError(err)),
  });

  const sendDisableCode = useMutation({
    mutationFn: () => apiClient.post('/api/auth/2fa/disable/send-code'),
    onSuccess: () => {
      setDisableStep('code-sent');
      setDisableCode('');
      toast.success('Doğrulama kodu e-postanıza gönderildi.');
    },
    onError: (err: unknown) => toast.error(friendlyError(err)),
  });

  const confirmDisable = useMutation({
    mutationFn: () => apiClient.post('/api/auth/2fa/disable/confirm', { code: disableCode }),
    onSuccess: () => {
      setDisableStep('idle');
      setDisableCode('');
      queryClient.setQueryData<SecurityStatus>(['security-status'], (current) => (
        current
          ? { ...current, twoFactorEnabled: false, twoFactorProvider: null }
          : current
      ));
      toast.success('İki faktörlü doğrulama devre dışı bırakıldı.');
    },
    onError: (err: unknown) => toast.error(friendlyError(err)),
  });

  if (!user) return null;

  return (
    <div>
      <SettingsNav />

      <div className="page-header mb-6">
        <div className="flex items-center gap-2">
          <ShieldCheck size={20} className="text-slate-500" />
          <div>
            <h1 className="page-title">Güvenlik</h1>
            <p className="page-subtitle">Hesap güvenliği ve iki faktörlü doğrulama ayarları.</p>
          </div>
        </div>
      </div>

      {isLoading && <PageLoading />}

      {error && (
        <div className="alert-error flex items-center gap-2 max-w-xl mb-4">
          <AlertCircle size={16} className="shrink-0" />
          Güvenlik bilgileri yüklenemedi.
          <button type="button" onClick={() => void refetch()} className="underline text-sm ml-1">
            Yeniden dene
          </button>
        </div>
      )}

      {status && (
        <div className="card p-6 max-w-xl space-y-6">
          {/* 2FA Status Banner */}
          <div className={[
            'flex items-center gap-3 rounded-lg px-4 py-3',
            status.twoFactorEnabled ? 'bg-green-50 text-green-800' : 'bg-slate-50 text-slate-700',
          ].join(' ')}>
            {status.twoFactorEnabled
              ? <ShieldCheck size={20} className="text-green-600 shrink-0" />
              : <ShieldOff size={20} className="text-slate-400 shrink-0" />}
            <div>
              <p className="text-sm font-medium">
                {status.twoFactorEnabled
                  ? 'İki Faktörlü Doğrulama Etkin'
                  : 'İki Faktörlü Doğrulama Devre Dışı'}
              </p>
              <p className="text-xs text-slate-500 mt-0.5">{status.email}</p>
            </div>
          </div>

          {/* Enable 2FA Flow */}
          {!status.twoFactorEnabled && (
            <div className="space-y-3">
              <div>
                <h2 className="font-medium text-slate-900">2FA Etkinleştir</h2>
                <p className="text-sm text-slate-500 mt-1">
                  Giriş yaparken e-postanıza tek kullanımlık kod gönderilir.
                </p>
              </div>

              {enableStep === 'idle' && (
                <button
                  onClick={() => sendEnableCode.mutate()}
                  disabled={sendEnableCode.isPending}
                  className="btn btn-primary flex items-center gap-2"
                >
                  <KeyRound size={15} />
                  {sendEnableCode.isPending ? 'Gönderiliyor...' : 'Etkinleştir'}
                </button>
              )}

              {enableStep === 'code-sent' && (
                <div className="space-y-3">
                  <p className="text-sm text-slate-600">
                    <strong>{status.email}</strong> adresine doğrulama kodu gönderildi.
                  </p>
                  <div className="flex gap-2">
                    <input
                      type="text"
                      inputMode="numeric"
                      autoComplete="one-time-code"
                      maxLength={6}
                      value={enableCode}
                      onChange={(e) => setEnableCode(e.target.value.replace(/\D/g, ''))}
                      placeholder="6 haneli kod"
                      aria-label="2FA etkinleştirme doğrulama kodu"
                      className="input w-36 text-center tracking-widest font-mono"
                    />
                    <button
                      onClick={() => confirmEnable.mutate()}
                      disabled={enableCode.length !== 6 || confirmEnable.isPending}
                      className="btn btn-primary"
                    >
                      {confirmEnable.isPending ? 'Doğrulanıyor...' : 'Onayla'}
                    </button>
                  </div>
                  <button
                    onClick={() => sendEnableCode.mutate()}
                    disabled={sendEnableCode.isPending}
                    className="text-xs text-brand-600 hover:underline"
                  >
                    Kodu tekrar gönder
                  </button>
                </div>
              )}
            </div>
          )}

          {/* Disable 2FA Flow */}
          {status.twoFactorEnabled && (
            <div className="space-y-3">
              <div>
                <h2 className="font-medium text-slate-900">2FA Devre Dışı Bırak</h2>
                <p className="text-sm text-slate-500 mt-1">
                  Devre dışı bırakmak için e-postanıza doğrulama kodu gönderilir.
                </p>
              </div>

              {disableStep === 'idle' && (
                <button
                  onClick={() => sendDisableCode.mutate()}
                  disabled={sendDisableCode.isPending}
                  className="btn btn-secondary flex items-center gap-2 text-rose-600 border-rose-200 hover:bg-rose-50"
                >
                  <ShieldOff size={15} />
                  {sendDisableCode.isPending ? 'Gönderiliyor...' : 'Devre Dışı Bırak'}
                </button>
              )}

              {disableStep === 'code-sent' && (
                <div className="space-y-3">
                  <p className="text-sm text-slate-600">
                    <strong>{status.email}</strong> adresine doğrulama kodu gönderildi.
                  </p>
                  <div className="flex gap-2">
                    <input
                      type="text"
                      inputMode="numeric"
                      autoComplete="one-time-code"
                      maxLength={6}
                      value={disableCode}
                      onChange={(e) => setDisableCode(e.target.value.replace(/\D/g, ''))}
                      placeholder="6 haneli kod"
                      aria-label="2FA devre dışı bırakma doğrulama kodu"
                      className="input w-36 text-center tracking-widest font-mono"
                    />
                    <button
                      onClick={() => confirmDisable.mutate()}
                      disabled={disableCode.length !== 6 || confirmDisable.isPending}
                      className="btn btn-primary bg-rose-600 hover:bg-rose-700 border-rose-600"
                    >
                      {confirmDisable.isPending ? 'Doğrulanıyor...' : 'Onayla'}
                    </button>
                  </div>
                  <button
                    onClick={() => sendDisableCode.mutate()}
                    disabled={sendDisableCode.isPending}
                    className="text-xs text-brand-600 hover:underline"
                  >
                    Kodu tekrar gönder
                  </button>
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
