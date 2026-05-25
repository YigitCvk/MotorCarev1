// src/shared/utils/format.ts

export function money(value: number | null | undefined): string {
  if (value === null || value === undefined) return '₺0,00';
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

export function dateText(value: string | Date | null | undefined): string {
  if (!value) return '-';
  const date = typeof value === 'string' ? new Date(value) : value;
  return new Intl.DateTimeFormat('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(date);
}

export function dateTimeText(value: string | Date | null | undefined): string {
  if (!value) return '-';
  const date = typeof value === 'string' ? new Date(value) : value;
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
