import clsx from 'clsx';

export interface FilterBarProps {
  children: React.ReactNode;
  actions?: React.ReactNode;
  className?: string;
}

export function FilterBar({ children, actions, className }: FilterBarProps) {
  return (
    <div className={clsx('rounded-lg border border-slate-200 bg-white p-4 shadow-sm', className)}>
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">{children}</div>
      {actions && <div className="mt-4 flex flex-wrap items-center justify-end gap-2">{actions}</div>}
    </div>
  );
}
