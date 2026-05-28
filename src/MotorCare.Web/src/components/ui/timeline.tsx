import clsx from 'clsx';

export interface TimelineItem {
  id: React.Key;
  title: React.ReactNode;
  description?: React.ReactNode;
  meta?: React.ReactNode;
  icon?: React.ReactNode;
}

export interface TimelineProps {
  items: TimelineItem[];
  emptyState?: React.ReactNode;
  className?: string;
}

export function Timeline({ items, emptyState, className }: TimelineProps) {
  if (items.length === 0) return <div className={clsx('text-sm text-slate-500', className)}>{emptyState ?? 'Kayit bulunamadi.'}</div>;

  return (
    <ol className={clsx('relative space-y-4 border-l border-slate-200 pl-5', className)}>
      {items.map((item) => (
        <li key={item.id} className="relative">
          <span className="absolute -left-[1.65rem] flex h-6 w-6 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-500">
            {item.icon ?? <span className="h-2 w-2 rounded-full bg-brand-600" />}
          </span>
          <div className="min-w-0">
            <div className="flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between">
              <h3 className="text-sm font-semibold text-slate-900">{item.title}</h3>
              {item.meta && <div className="text-xs text-slate-500">{item.meta}</div>}
            </div>
            {item.description && <div className="mt-1 text-sm text-slate-600">{item.description}</div>}
          </div>
        </li>
      ))}
    </ol>
  );
}
