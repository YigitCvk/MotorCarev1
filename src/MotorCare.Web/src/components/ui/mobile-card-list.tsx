import clsx from 'clsx';

export interface MobileCardListProps<T> {
  items: T[];
  renderItem: (item: T, index: number) => React.ReactNode;
  getItemKey?: (item: T, index: number) => React.Key;
  emptyState?: React.ReactNode;
  className?: string;
}

export function MobileCardList<T>({
  items,
  renderItem,
  getItemKey,
  emptyState,
  className,
}: MobileCardListProps<T>) {
  if (items.length === 0) {
    return <div className={clsx('rounded-lg border border-slate-200 bg-white p-5 text-center text-sm text-slate-500', className)}>{emptyState ?? 'Kayit bulunamadi.'}</div>;
  }

  return (
    <div className={clsx('grid gap-3 sm:hidden', className)}>
      {items.map((item, index) => (
        <div key={getItemKey?.(item, index) ?? index} className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
          {renderItem(item, index)}
        </div>
      ))}
    </div>
  );
}
