import clsx from 'clsx';

export interface PrintLayoutProps {
  title: React.ReactNode;
  children: React.ReactNode;
  subtitle?: React.ReactNode;
  actions?: React.ReactNode;
  footer?: React.ReactNode;
  className?: string;
}

export function PrintLayout({ title, subtitle, actions, footer, children, className }: PrintLayoutProps) {
  return (
    <main className={clsx('mx-auto max-w-4xl bg-white p-4 text-slate-900 print:max-w-none print:p-0', className)}>
      <header className="mb-6 flex flex-col gap-3 border-b border-slate-200 pb-4 print:mb-4">
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0">
            <h1 className="text-2xl font-bold text-slate-950 print:text-xl">{title}</h1>
            {subtitle && <p className="mt-1 text-sm text-slate-500">{subtitle}</p>}
          </div>
          {actions && <div className="flex shrink-0 flex-wrap justify-end gap-2 print:hidden">{actions}</div>}
        </div>
      </header>
      {children}
      {footer && <footer className="mt-8 border-t border-slate-200 pt-4 text-xs text-slate-500">{footer}</footer>}
    </main>
  );
}
