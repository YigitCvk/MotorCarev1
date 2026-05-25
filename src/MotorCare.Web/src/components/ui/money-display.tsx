// src/components/ui/money-display.tsx
import clsx from 'clsx';
import { money } from '@/shared/utils/format';

interface MoneyDisplayProps {
  value: number | null | undefined;
  className?: string;
  bold?: boolean;
  color?: 'default' | 'green' | 'red';
}

const colorClass: Record<string, string> = {
  default: 'text-slate-700',
  green: 'text-green-700',
  red: 'text-red-700',
};

export function MoneyDisplay({ value, className, bold, color = 'default' }: MoneyDisplayProps) {
  return (
    <span className={clsx(colorClass[color], bold && 'font-semibold', className)}>
      {money(value)}
    </span>
  );
}
