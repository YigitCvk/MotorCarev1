import clsx from 'clsx';

export type ResponsiveGridColumns = 1 | 2 | 3 | 4;

export interface ResponsiveGridProps {
  children: React.ReactNode;
  columns?: ResponsiveGridColumns;
  className?: string;
}

const columnClass: Record<ResponsiveGridColumns, string> = {
  1: 'grid-cols-1',
  2: 'grid-cols-1 md:grid-cols-2',
  3: 'grid-cols-1 md:grid-cols-2 xl:grid-cols-3',
  4: 'grid-cols-1 sm:grid-cols-2 xl:grid-cols-4',
};

export function ResponsiveGrid({ children, columns = 3, className }: ResponsiveGridProps) {
  return <div className={clsx('grid gap-4', columnClass[columns], className)}>{children}</div>;
}
