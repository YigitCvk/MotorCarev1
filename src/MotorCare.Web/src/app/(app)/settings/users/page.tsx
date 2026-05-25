'use client';

import { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import {
  Users,
  UserPlus,
  X,
  AlertCircle,
  ShieldCheck,
  UserX,
} from 'lucide-react';
import apiClient from '@/core/api/client';
import { useAuth } from '@/core/auth/auth.context';
import { friendlyError } from '@/core/api/errors';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';

interface UserDto {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

const ROLE_LABELS: Record<string, string> = {
  Owner: 'Sahip',
  Admin: 'Yönetici',
  Manager: 'Müdür',
  Technician: 'Teknisyen',
  Inspector: 'Eksper',
  Accountant: 'Muhasebe',
  ReadOnly: 'Salt Okuma',
};

const ASSIGNABLE_ROLES = ['Admin', 'Manager', 'Technician', 'Inspector', 'Accountant', 'ReadOnly'];

function roleLabel(role: string): string {
  return ROLE_LABELS[role] ?? role;
}

function SettingsNav() {
  const pathname = usePathname();
  const tabs = [
    { label: 'İşletme Bilgileri', href: '/settings/business' },
    { label: 'Kullanıcılar', href: '/settings/users' },
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

const inviteSchema = z.object({
  email: z.string().email('Geçerli bir e-posta girin'),
  role: z.string().min(1, 'Rol seçiniz'),
  fullName: z.string().min(2, 'Ad soyad zorunludur').optional().or(z.literal('')),
});

type InviteFormValues = z.infer<typeof inviteSchema>;

export default function SettingsUsersPage() {
  const { user: currentUser } = useAuth();
  const queryClient = useQueryClient();

  const canManage =
    currentUser?.role === 'Owner' || currentUser?.role === 'Admin';

  // Invite panel
  const [showInvite, setShowInvite] = useState(false);

  // Confirm deactivate dialog
  const [confirmDeactivate, setConfirmDeactivate] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset: resetInviteForm,
    formState: { errors: inviteErrors },
  } = useForm<InviteFormValues>({
    resolver: zodResolver(inviteSchema),
    defaultValues: { fullName: '', email: '', role: 'Technician' },
  });

  // Load users
  const { data: users, isLoading, error: loadError, refetch } = useQuery<UserDto[]>({
    queryKey: ['users'],
    queryFn: async () => {
      const { data } = await apiClient.get<UserDto[]>('/api/users');
      return data;
    },
  });

  // Role mutation
  const roleMutation = useMutation({
    mutationFn: async ({ id, role }: { id: string; role: string }) => {
      await apiClient.put(`/api/users/${id}/role`, { role });
    },
    onSuccess: () => {
      toast.success('Rol güncellendi');
      void queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (err: unknown) => {
      toast.error(friendlyError(err));
    },
  });

  // Deactivate mutation
  const deactivateMutation = useMutation({
    mutationFn: async (id: string) => {
      await apiClient.patch(`/api/users/${id}/deactivate`, {});
    },
    onSuccess: () => {
      toast.success('Kullanıcı devre dışı bırakıldı');
      setConfirmDeactivate(null);
      void queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (err: unknown) => {
      toast.error(friendlyError(err));
      setConfirmDeactivate(null);
    },
  });

  // Invite mutation
  const inviteMutation = useMutation({
    mutationFn: async (payload: InviteFormValues) => {
      await apiClient.post('/api/users/invite', payload);
    },
    onSuccess: () => {
      toast.success('Kullanıcı davet edildi');
      resetInviteForm();
      setShowInvite(false);
      void queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (err: unknown) => {
      toast.error(friendlyError(err));
    },
  });

  function handleRoleChange(userId: string, role: string) {
    roleMutation.mutate({ id: userId, role });
  }

  function handleDeactivateConfirm() {
    if (confirmDeactivate) {
      deactivateMutation.mutate(confirmDeactivate);
    }
  }

  return (
    <div>
      <SettingsNav />

      <div className="page-header mb-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Users size={20} className="text-slate-500" />
            <div>
              <h1 className="page-title">Kullanıcı Yönetimi</h1>
              <p className="page-subtitle">İşletme kullanıcılarını ve rollerini yönetin.</p>
            </div>
          </div>
          {canManage && (
            <button
              className="btn btn-primary flex items-center gap-2"
              onClick={() => {
                setShowInvite((v) => !v);
                if (showInvite) resetInviteForm();
              }}
            >
              {showInvite ? <X size={15} /> : <UserPlus size={15} />}
              {showInvite ? 'İptal' : 'Kullanıcı Davet Et'}
            </button>
          )}
        </div>
      </div>

      {/* Invite form panel */}
      {showInvite && canManage && (
        <div className="card p-5 mb-6 max-w-xl border border-blue-100 bg-blue-50/30">
          <h2 className="text-sm font-semibold text-slate-700 mb-4 flex items-center gap-2">
            <UserPlus size={15} />
            Yeni Kullanıcı Davet Et
          </h2>

          <form onSubmit={handleSubmit((values) => inviteMutation.mutate(values))} className="space-y-3">
            <div className="form-group">
              <label className="label" htmlFor="invite-fullName">Ad Soyad</label>
              <input
                id="invite-fullName"
                type="text"
                className="input"
                placeholder="Kullanıcının adı ve soyadı"
                {...register('fullName')}
              />
              {inviteErrors.fullName && (
                <p className="error-text mt-1">{inviteErrors.fullName.message}</p>
              )}
            </div>
            <div className="form-group">
              <label className="label" htmlFor="invite-email">
                E-posta <span className="text-red-500">*</span>
              </label>
              <input
                id="invite-email"
                type="email"
                className="input"
                placeholder="kullanici@sirket.com"
                {...register('email')}
              />
              {inviteErrors.email && (
                <p className="error-text mt-1">{inviteErrors.email.message}</p>
              )}
            </div>
            <div className="form-group">
              <label className="label" htmlFor="invite-role">Rol</label>
              <select
                id="invite-role"
                className="input"
                {...register('role')}
              >
                {ASSIGNABLE_ROLES.map((r) => (
                  <option key={r} value={r}>
                    {roleLabel(r)}
                  </option>
                ))}
              </select>
              {inviteErrors.role && (
                <p className="error-text mt-1">{inviteErrors.role.message}</p>
              )}
            </div>
            <div className="flex gap-3 pt-1">
              <button
                type="submit"
                disabled={inviteMutation.isPending}
                className="btn btn-primary"
              >
                {inviteMutation.isPending ? 'Gönderiliyor...' : 'Davet Gönder'}
              </button>
              <button
                type="button"
                onClick={() => {
                  setShowInvite(false);
                  resetInviteForm();
                }}
                className="btn btn-ghost"
              >
                İptal
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Load error */}
      {loadError && (
        <div className="alert-error mb-4 flex items-center gap-2">
          <AlertCircle size={16} className="shrink-0" />
          Kullanıcılar yüklenemedi.
          <button onClick={() => void refetch()} className="underline text-sm ml-1">
            Yeniden dene
          </button>
        </div>
      )}

      {isLoading && (
        <div className="py-10 text-center text-slate-400 text-sm">Yükleniyor...</div>
      )}

      {!isLoading && !loadError && users && (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Ad Soyad</th>
                <th>E-posta</th>
                <th>Rol</th>
                <th>Durum</th>
                {canManage && <th>İşlemler</th>}
              </tr>
            </thead>
            <tbody>
              {users.map((u) => {
                const isSelf = u.id === currentUser?.id;
                const isOwner = u.role === 'Owner';

                return (
                  <tr
                    key={u.id}
                    className={isSelf ? 'bg-blue-50/40' : undefined}
                  >
                    <td>
                      <span className="font-medium text-slate-900">{u.fullName}</span>
                      {isSelf && (
                        <span className="ml-2 text-xs text-blue-500 font-normal">(siz)</span>
                      )}
                    </td>
                    <td className="text-slate-500">{u.email}</td>
                    <td>
                      <span className="flex items-center gap-1 text-slate-700">
                        {isOwner && <ShieldCheck size={13} className="text-amber-500" />}
                        {roleLabel(u.role)}
                      </span>
                    </td>
                    <td>
                      {u.isActive ? (
                        <span className="badge-green">Aktif</span>
                      ) : (
                        <span className="badge-gray">Pasif</span>
                      )}
                    </td>
                    {canManage && (
                      <td>
                        {!isOwner ? (
                          <div className="flex items-center gap-2 flex-wrap">
                            <label htmlFor={`role-${u.id}`} className="sr-only">
                              {u.fullName} kullanıcısının rolünü değiştir
                            </label>
                            <select
                              id={`role-${u.id}`}
                              className="input py-1 text-xs h-auto"
                              value={u.role}
                              disabled={roleMutation.isPending}
                              onChange={(e) => handleRoleChange(u.id, e.target.value)}
                              aria-label={`${u.fullName} rolünü değiştir`}
                            >
                              {ASSIGNABLE_ROLES.map((r) => (
                                <option key={r} value={r}>
                                  {roleLabel(r)}
                                </option>
                              ))}
                            </select>
                            {u.isActive && !isSelf && (
                              <button
                                className="btn btn-danger py-1 px-2 text-xs flex items-center gap-1"
                                disabled={deactivateMutation.isPending}
                                onClick={() => setConfirmDeactivate(u.id)}
                                title="Pasif et"
                              >
                                <UserX size={13} />
                                Pasif Et
                              </button>
                            )}
                          </div>
                        ) : (
                          <span className="text-xs text-slate-400 italic">—</span>
                        )}
                      </td>
                    )}
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {!isLoading && !loadError && users?.length === 0 && (
        <div className="py-10 text-center text-slate-400 text-sm">
          Henüz kullanıcı bulunmuyor.
        </div>
      )}

      {/* Deactivate confirmation dialog */}
      <ConfirmDialog
        open={confirmDeactivate !== null}
        onClose={() => setConfirmDeactivate(null)}
        onConfirm={handleDeactivateConfirm}
        title="Kullanıcıyı Pasif Et"
        description="Bu kullanıcıyı pasif etmek istediğinize emin misiniz? Kullanıcı sisteme giriş yapamayacaktır."
        confirmLabel="Pasif Et"
        variant="danger"
        loading={deactivateMutation.isPending}
      />
    </div>
  );
}
