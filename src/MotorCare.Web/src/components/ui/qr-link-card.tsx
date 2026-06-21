import clsx from 'clsx';
import { QRCodeSVG } from 'qrcode.react';
import { CopyButton } from './copy-button';

export interface QRLinkCardProps {
  href?: string | null;
  title: React.ReactNode;
  description?: React.ReactNode;
  emptyText?: React.ReactNode;
  className?: string;
  compact?: boolean;
}

export function QRLinkCard({
  href,
  title,
  description,
  emptyText = 'Paylaşım bağlantısı henüz oluşturulmamış.',
  className,
  compact,
}: QRLinkCardProps) {
  const value = href?.trim();

  return (
    <div className={clsx('rounded-lg border border-slate-200 bg-white p-4 shadow-sm', className)}>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div
          className={clsx(
            'flex shrink-0 items-center justify-center rounded-lg border border-slate-200 bg-white p-2',
            compact ? 'h-24 w-24' : 'h-32 w-32'
          )}
        >
          {value ? (
            <QRCodeSVG
              value={value}
              size={compact ? 80 : 112}
              level="M"
              marginSize={2}
              title={typeof title === 'string' ? title : 'Paylaşım QR kodu'}
            />
          ) : (
            <span className="px-2 text-center text-xs text-slate-400">QR yok</span>
          )}
        </div>
        <div className="min-w-0 flex-1">
          <h3 className="text-sm font-semibold text-slate-900">{title}</h3>
          {description && <p className="mt-1 text-sm text-slate-500">{description}</p>}
          {value ? (
            <>
              <a
                href={value}
                className="mt-2 block truncate text-sm font-medium text-brand-600 hover:text-brand-700"
                target="_blank"
                rel="noreferrer"
              >
                {value}
              </a>
              <CopyButton value={value} className="mt-3 print:hidden" />
            </>
          ) : (
            <p className="mt-2 text-sm text-slate-500">{emptyText}</p>
          )}
        </div>
      </div>
    </div>
  );
}
