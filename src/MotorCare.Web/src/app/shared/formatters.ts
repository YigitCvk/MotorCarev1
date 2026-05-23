export const tryCurrency = new Intl.NumberFormat('tr-TR', {
  style: 'currency',
  currency: 'TRY',
  minimumFractionDigits: 2
});

export function money(value: unknown): string {
  const number = typeof value === 'number' ? value : Number(value ?? 0);
  return tryCurrency.format(Number.isFinite(number) ? number : 0);
}

export function dateText(value: unknown): string {
  if (!value) return '-';
  const date = new Date(String(value));
  return Number.isNaN(date.getTime()) ? '-' : date.toLocaleString('tr-TR');
}

export function field<T>(record: Record<string, unknown> | null | undefined, key: string, fallback = '-'): T | string {
  const value = record?.[key];
  return value === null || value === undefined || value === '' ? fallback : (value as T);
}

export function maskPublic(value: unknown): string {
  const text = String(value ?? '').trim();
  if (!text) return '-';
  if (text.includes('@')) {
    const [name, domain] = text.split('@');
    return `${name.slice(0, 2)}***@${domain ?? '***'}`;
  }
  if (/^\+?\d/.test(text)) {
    return `${text.slice(0, 3)}***${text.slice(-2)}`;
  }
  return `${text.slice(0, 2)}***`;
}
