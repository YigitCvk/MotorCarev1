import { appConfig } from '@/shared/config/env';

function trimTrailingSlash(value: string): string {
  return value.replace(/\/+$/, '');
}

export function publicAppBaseUrl(): string {
  return trimTrailingSlash(appConfig.publicAppUrl || 'http://localhost:3000');
}

export function publicServiceRecordUrl(slug?: string | null): string {
  return `${publicAppBaseUrl()}/public/service-record/${encodeURIComponent(slug ?? '')}`;
}

export function publicInspectionReportUrl(slug?: string | null): string {
  return `${publicAppBaseUrl()}/public/inspection-report/${encodeURIComponent(slug ?? '')}`;
}
