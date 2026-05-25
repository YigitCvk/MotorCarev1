// src/core/auth/schemas.ts
import { z } from 'zod';

export const loginSchema = z.object({
  tenantIdentifier: z.string().min(1, 'İşletme kodu zorunludur'),
  email: z.string().email('Geçerli bir e-posta adresi girin'),
  password: z.string().min(1, 'Şifre zorunludur'),
});
export type LoginFormData = z.infer<typeof loginSchema>;

export const registerSchema = z.object({
  tenantName: z.string().min(2, 'İşletme adı en az 2 karakter olmalıdır'),
  tenantIdentifier: z
    .string()
    .min(3, 'İşletme kodu en az 3 karakter olmalıdır')
    .max(50)
    .regex(/^[a-z0-9-]+$/, 'Küçük harf, rakam ve tire kullanabilirsiniz'),
  email: z.string().email('Geçerli bir e-posta adresi girin'),
  fullName: z.string().min(2, 'Ad soyad en az 2 karakter olmalıdır'),
  password: z.string().min(8, 'Şifre en az 8 karakter olmalıdır'),
  confirmPassword: z.string(),
}).refine((d) => d.password === d.confirmPassword, {
  message: 'Şifreler eşleşmiyor',
  path: ['confirmPassword'],
});
export type RegisterFormData = z.infer<typeof registerSchema>;

export const forgotPasswordSchema = z.object({
  tenantIdentifier: z.string().min(1, 'İşletme kodu zorunludur'),
  email: z.string().email('Geçerli bir e-posta adresi girin'),
});
export type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;

export const resetPasswordSchema = z.object({
  code: z.string().length(6, 'Kod 6 haneli olmalıdır').regex(/^\d+$/, 'Kod yalnızca rakamlardan oluşmalıdır'),
  newPassword: z.string().min(8, 'Şifre en az 8 karakter olmalıdır'),
  confirmPassword: z.string(),
}).refine((d) => d.newPassword === d.confirmPassword, {
  message: 'Şifreler eşleşmiyor',
  path: ['confirmPassword'],
});
export type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

export const acceptInviteSchema = z.object({
  fullName: z.string().min(2, 'Ad soyad en az 2 karakter olmalıdır'),
  password: z.string().min(8, 'Şifre en az 8 karakter olmalıdır'),
  confirmPassword: z.string(),
}).refine((d) => d.password === d.confirmPassword, {
  message: 'Şifreler eşleşmiyor',
  path: ['confirmPassword'],
});
export type AcceptInviteFormData = z.infer<typeof acceptInviteSchema>;
