// src/shared/utils/format.ts

const tryFormatter = new Intl.NumberFormat('tr-TR', {
  style: 'currency',
  currency: 'TRY',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export function money(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) return tryFormatter.format(0);
  return tryFormatter.format(value);
}

function toValidDate(value: string | Date | null | undefined): Date | null {
  if (!value) return null;
  const date = typeof value === 'string' ? new Date(value) : value;
  return Number.isNaN(date.getTime()) ? null : date;
}

export function dateText(value: string | Date | null | undefined): string {
  const date = toValidDate(value);
  if (!date) return '-';
  return new Intl.DateTimeFormat('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(date);
}

export function dateTimeText(value: string | Date | null | undefined): string {
  const date = toValidDate(value);
  if (!date) return '-';
  return new Intl.DateTimeFormat('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date);
}

export function truncate(value: string | null | undefined, length = 50): string {
  if (!value) return '-';
  return value.length > length ? `${value.slice(0, length)}...` : value;
}

export function todayInputValue(): string {
  return new Date().toISOString().slice(0, 10);
}

export function lineTotal(quantity: number, unitPrice: number, discount = 0): number {
  return Math.max(0, quantity * unitPrice - discount);
}

export const formatMoney = money;
export const formatDate = dateText;
export const formatDateTime = dateTimeText;
