import type { PagedResult } from '@/shared/types/api.types';

const ARRAY_KEYS = ['items', 'data', 'results', 'value'] as const;

export function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

export function normalizeApiArray<T = unknown>(value: unknown): T[] {
  if (Array.isArray(value)) return value as T[];
  if (!isRecord(value)) return [];

  for (const key of ARRAY_KEYS) {
    const candidate = value[key];
    if (Array.isArray(candidate)) return candidate as T[];
  }

  return [];
}

export function normalizePagedResult<T>(
  value: unknown,
  fallbackPageNumber = 1,
  fallbackPageSize = 10
): PagedResult<T> {
  const record = isRecord(value) ? value : {};
  const items = normalizeApiArray<T>(value);
  const totalCount =
    readNumber(record.totalCount) ?? readNumber(record.total) ?? readNumber(record.count) ?? items.length;
  const pageSize = readNumber(record.pageSize) ?? fallbackPageSize;
  const pageNumber = readNumber(record.pageNumber) ?? readNumber(record.page) ?? fallbackPageNumber;
  const totalPages =
    readNumber(record.totalPages) ?? Math.max(1, Math.ceil(totalCount / Math.max(pageSize, 1)));

  return {
    items,
    totalCount,
    pageNumber,
    pageSize,
    totalPages,
  };
}

export function readString(value: unknown, fallback = ''): string {
  return typeof value === 'string' ? value : fallback;
}

export function readNumber(value: unknown): number | undefined {
  if (typeof value === 'number' && Number.isFinite(value)) return value;
  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value);
    if (Number.isFinite(parsed)) return parsed;
  }
  return undefined;
}
