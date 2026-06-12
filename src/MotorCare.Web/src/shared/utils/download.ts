import type { AxiosRequestConfig } from 'axios';
import apiClient from '@/core/api/client';

const DEFAULT_FALLBACK_FILE_NAME = 'download.bin';

export interface DownloadFileOptions {
  fallbackFileName?: string;
  requestConfig?: AxiosRequestConfig;
}

export async function downloadFile(
  url: string,
  options: DownloadFileOptions = {},
): Promise<string> {
  const response = await apiClient.get<Blob>(url, {
    ...options.requestConfig,
    responseType: 'blob',
  });

  const fileName = fileNameFromContentDisposition(
    response.headers['content-disposition'],
    options.fallbackFileName,
  );

  triggerBrowserDownload(response.data, fileName);
  return fileName;
}

export function fileNameFromContentDisposition(
  contentDisposition?: string,
  fallbackFileName = DEFAULT_FALLBACK_FILE_NAME,
): string {
  const extendedFileName = contentDisposition
    ? parseExtendedFileName(contentDisposition)
    : null;
  const regularFileName = contentDisposition
    ? parseRegularFileName(contentDisposition)
    : null;

  return sanitizeFileName(extendedFileName ?? regularFileName ?? fallbackFileName);
}

function parseExtendedFileName(contentDisposition: string): string | null {
  const match = contentDisposition.match(/filename\*\s*=\s*("[^"]*"|[^;]+)/i);
  if (!match) return null;

  const rawValue = stripQuotes(match[1].trim());
  const rfc5987Match = rawValue.match(/^([^']*)'[^']*'(.*)$/);
  const charset = rfc5987Match?.[1]?.toLowerCase();
  const encodedValue = rfc5987Match?.[2] ?? rawValue;

  if (charset && charset !== 'utf-8' && charset !== 'us-ascii') {
    return null;
  }

  try {
    return decodeURIComponent(encodedValue);
  } catch {
    return encodedValue;
  }
}

function parseRegularFileName(contentDisposition: string): string | null {
  const match = contentDisposition.match(/filename\s*=\s*(?:"((?:\\.|[^"])*)"|([^;]+))/i);
  const value = match?.[1] ?? match?.[2];
  if (!value) return null;

  return stripQuotes(value.trim()).replace(/\\"/g, '"').replace(/\\\\/g, '\\');
}

function stripQuotes(value: string): string {
  return value.startsWith('"') && value.endsWith('"') ? value.slice(1, -1) : value;
}

function sanitizeFileName(value: string): string {
  const sanitized = value
    .replace(/[<>:"/\\|?*\u0000-\u001f\u007f]/g, '_')
    .trim()
    .slice(0, 200)
    .replace(/[. ]+$/g, '')
    .trim();

  if (sanitized) {
    return /^(con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\..*)?$/i.test(sanitized)
      ? `_${sanitized}`
      : sanitized;
  }

  const fallback = DEFAULT_FALLBACK_FILE_NAME
    .replace(/[<>:"/\\|?*\u0000-\u001f\u007f]/g, '_')
    .trim();
  return fallback || 'download.bin';
}

function triggerBrowserDownload(blob: Blob, fileName: string): void {
  if (typeof document === 'undefined') {
    throw new Error('Dosya indirme yalnızca tarayıcıda kullanılabilir.');
  }

  const objectUrl = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = objectUrl;
  anchor.download = fileName;
  anchor.style.display = 'none';
  document.body.appendChild(anchor);

  try {
    anchor.click();
  } finally {
    anchor.remove();
    window.setTimeout(() => URL.revokeObjectURL(objectUrl), 1_000);
  }
}
