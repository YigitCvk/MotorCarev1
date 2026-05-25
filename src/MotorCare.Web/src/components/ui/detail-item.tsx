import clsx from 'clsx';

export interface DetailItemProps {
  label: React.ReactNode;
  value: React.ReactNode;
  stacked?: boolean;
  className?: string;
}

export function DetailItem({ label, value, stacked, className }: DetailItemProps) {
  return (
    <div className={clsx(stacked ? 'space-y-1' : 'grid gap-1 sm:grid-cols-3 sm:gap-4', className)}>
      <dt className="text-sm font-medium text-slate-500">{label}</dt>
      <dd className={clsx('min-w-0 text-sm text-slate-900', !stacked && 'sm:col-span-2')}>{value ?? '-'}</dd>
    </div>
  );
}
