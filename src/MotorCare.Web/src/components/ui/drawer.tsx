'use client';

import clsx from 'clsx';
import { X } from 'lucide-react';
import { useEffect } from 'react';

export interface DrawerProps {
  open: boolean;
  title?: React.ReactNode;
  children: React.ReactNode;
  footer?: React.ReactNode;
  side?: 'left' | 'right';
  onClose: () => void;
  className?: string;
}

export function Drawer({ open, title, children, footer, side = 'right', onClose, className }: DrawerProps) {
  useEffect(() => {
    if (!open) return;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [onClose, open]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50">
      <button type="button" aria-label="Kapat" className="absolute inset-0 bg-slate-900/40" onClick={onClose} />
      <aside
        className={clsx(
          'absolute top-0 flex h-full w-full max-w-md flex-col bg-white shadow-xl',
          side === 'right' ? 'right-0' : 'left-0',
          className
        )}
      >
        <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
          <div className="min-w-0 text-base font-semibold text-slate-900">{title}</div>
          <button type="button" onClick={onClose} className="btn-ghost p-2" aria-label="Kapat">
            <X className="h-4 w-4" />
          </button>
        </div>
        <div className="flex-1 overflow-y-auto px-5 py-4">{children}</div>
        {footer && <div className="border-t border-slate-200 px-5 py-4">{footer}</div>}
      </aside>
    </div>
  );
}
