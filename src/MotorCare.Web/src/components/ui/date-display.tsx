import { dateText, dateTimeText } from '@/shared/utils/format';

export interface DateDisplayProps {
  value: string | Date | null | undefined;
  withTime?: boolean;
  fallback?: string;
}

export function DateDisplay({ value, withTime, fallback = '-' }: DateDisplayProps) {
  if (!value) return <span>{fallback}</span>;
  const label = withTime ? dateTimeText(value) : dateText(value);
  if (label === '-') return <span>{fallback}</span>;
  const dateTime = typeof value === 'string' ? value : value.toISOString();
  return <time dateTime={dateTime}>{label}</time>;
}
