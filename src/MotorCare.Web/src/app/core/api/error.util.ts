import { HttpErrorResponse } from '@angular/common/http';
import { ApiProblem } from '../models/api.models';

export function friendlyError(error: unknown, fallback = 'İşlem şu anda tamamlanamadı. Lütfen tekrar deneyin.'): string {
  if (error instanceof HttpErrorResponse) {
    const problem = parseProblem(error.error);
    if (problem.message) {
      return problem.message;
    }

    if (problem.errors) {
      const first = Object.values(problem.errors).flat().find(Boolean);
      if (first) {
        return first;
      }
    }

    if (error.status === 401) return 'Oturumunuzun süresi dolmuş olabilir. Lütfen tekrar giriş yapın.';
    if (error.status === 403) return 'Bu işlem için yetkiniz bulunmuyor.';
    if (error.status === 404) return 'İstenen kayıt bulunamadı.';
    if (error.status === 422) return 'Gönderilen bilgilerde bir hata var. Lütfen formu kontrol edin.';
    if (error.status === 429) return 'Çok sık deneme yapıldı. Kısa süre sonra tekrar deneyin.';
  }

  return fallback;
}

export function parseProblem(value: unknown): ApiProblem {
  if (isRecord(value)) {
    return {
      code: asString(value['code']),
      message: asString(value['message']) ?? asString(value['detail']) ?? asString(value['title']),
      traceId: asString(value['traceId']),
      errors: isErrorMap(value['errors']) ? value['errors'] : undefined
    };
  }

  return {};
}

function asString(value: unknown): string | undefined {
  return typeof value === 'string' && value.trim() ? value : undefined;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function isErrorMap(value: unknown): value is Record<string, string[]> {
  if (!isRecord(value)) return false;
  return Object.values(value).every((entry) => Array.isArray(entry));
}
