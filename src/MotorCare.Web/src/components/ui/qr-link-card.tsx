import clsx from 'clsx';
import { CopyButton } from './copy-button';

export interface QRLinkCardProps {
  href: string;
  title: React.ReactNode;
  description?: React.ReactNode;
  qrImageSrc?: string;
  className?: string;
}

export function QRLinkCard({ href, title, description, qrImageSrc, className }: QRLinkCardProps) {
  const src = qrImageSrc ?? `https://api.qrserver.com/v1/create-qr-code/?size=160x160&data=${encodeURIComponent(href)}`;

  return (
    <div className={clsx('rounded-lg border border-slate-200 bg-white p-4 shadow-sm', className)}>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={src} alt="" className="h-32 w-32 rounded border border-slate-200 bg-white p-2" />
        <div className="min-w-0 flex-1">
          <h3 className="text-sm font-semibold text-slate-900">{title}</h3>
          {description && <p className="mt-1 text-sm text-slate-500">{description}</p>}
          <a href={href} className="mt-2 block truncate text-sm font-medium text-brand-600 hover:text-brand-700" target="_blank" rel="noreferrer">
            {href}
          </a>
          <CopyButton value={href} className="mt-3" />
        </div>
      </div>
    </div>
  );
}
