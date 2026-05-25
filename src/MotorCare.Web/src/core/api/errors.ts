// src/core/api/errors.ts
import type { AxiosError } from 'axios';
import type { ApiProblem } from '@/shared/types/api.types';

const ERROR_CODE_MESSAGES: Record<string, string> = {
  EMAIL_VERIFICATION_CODE_INVALID: 'Doğrulama kodu hatalı veya süresi dolmuş.',
  EMAIL_VERIFICATION_CODE_EXPIRED: 'Doğrulama kodunun süresi dolmuş. Yeni kod isteyin.',
  EMAIL_NOT_VERIFIED: 'E-posta adresiniz henüz doğrulanmamış.',
  INVALID_CREDENTIALS: 'İşletme kodu, e-posta veya şifre hatalı.',
  ACCOUNT_LOCKED: 'Hesap geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.',
  INVITE_TOKEN_INVALID: 'Bu davet bağlantısı geçersiz veya süresi dolmuş.',
  INVITE_TOKEN_EXPIRED: 'Davet bağlantısının süresi dolmuş.',
  RESET_CODE_INVALID: 'Şifre sıfırlama kodu hatalı veya süresi dolmuş.',
  TENANT_NOT_FOUND: 'İşletme bulunamadı.',
  USER_ALREADY_EXISTS: 'Bu e-posta adresiyle zaten bir hesap mevcut.',
  INSUFFICIENT_PERMISSIONS: 'Bu işlemi yapmak için yetkiniz bulunmuyor.',
};

export function friendlyError(error: unknown, fallback = 'Bir hata oluştu. Lütfen tekrar deneyin.'): string {
  const axiosErr = error as AxiosError<ApiProblem>;

  if (!axiosErr?.response) {
    return 'Sunucuya bağlanılamadı. İnternet bağlantınızı kontrol edin.';
  }

  const { status, data } = axiosErr.response;

  if (status === 401) return 'Oturumunuzun süresi dolmuş. Lütfen tekrar giriş yapın.';
  if (status === 403) return 'Bu işlemi yapmak için yetkiniz bulunmuyor.';
  if (status === 429) return 'Çok fazla istek gönderildi. Lütfen bekleyin.';
  if (status >= 500) return 'Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.';

  if (data?.code && ERROR_CODE_MESSAGES[data.code]) {
    return ERROR_CODE_MESSAGES[data.code];
  }

  if (data?.message && isUserFriendlyMessage(data.message)) {
    return data.message;
  }

  return fallback;
}

function isUserFriendlyMessage(message: string): boolean {
  const techPatterns = [/exception/i, /stack/i, /null reference/i, /sql/i, /database/i, /inner exception/i];
  return !techPatterns.some((pattern) => pattern.test(message));
}

export function extractValidationErrors(error: unknown): Record<string, string> {
  const axiosErr = error as AxiosError<ApiProblem>;
  const errors = axiosErr?.response?.data?.errors;
  if (!errors) return {};
  return Object.fromEntries(
    Object.entries(errors).map(([key, messages]) => [
      key.charAt(0).toLowerCase() + key.slice(1),
      Array.isArray(messages) ? messages[0] : String(messages),
    ])
  );
}
