import clsx from 'clsx';

export interface DataTableColumn<T> {
  key: string;
  header: React.ReactNode;
  render?: (row: T) => React.ReactNode;
  className?: string;
  headerClassName?: string;
  hideOnMobile?: boolean;
}

export interface DataTableProps<T> {
  columns: Array<DataTableColumn<T>>;
  data: T[];
  getRowKey?: (row: T, index: number) => React.Key;
  onRowClick?: (row: T) => void;
  emptyState?: React.ReactNode;
  className?: string;
  tableClassName?: string;
}

function defaultCell<T>(row: T, key: string): React.ReactNode {
  const value = (row as Record<string, unknown>)[key];
  if (value === null || value === undefined || value === '') return '-';
  return String(value);
}

export function DataTable<T>({
  columns,
  data,
  getRowKey,
  onRowClick,
  emptyState,
  className,
  tableClassName,
}: DataTableProps<T>) {
  return (
    <div className={clsx('table-container', className)}>
      <div className="overflow-x-auto">
        <table className={clsx('table', tableClassName)}>
          <thead>
            <tr>
              {columns.map((column) => (
                <th
                  key={column.key}
                  className={clsx(column.hideOnMobile && 'hidden sm:table-cell', column.headerClassName)}
                >
                  {column.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="py-8 text-center text-sm text-slate-500">
                  {emptyState ?? 'Kayit bulunamadi.'}
                </td>
              </tr>
            ) : (
              data.map((row, index) => (
                <tr
                  key={getRowKey?.(row, index) ?? index}
                  onClick={() => onRowClick?.(row)}
                  className={clsx(onRowClick && 'cursor-pointer hover:bg-slate-50')}
                >
                  {columns.map((column) => (
                    <td key={column.key} className={clsx(column.hideOnMobile && 'hidden sm:table-cell', column.className)}>
                      {column.render ? column.render(row) : defaultCell(row, column.key)}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
