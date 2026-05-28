import { RefreshCw } from 'lucide-react';
import { Button } from './button';

export interface RetryStateProps {
  title?: string;
  message?: string;
  retryLabel?: string;
  onRetry: () => void;
  loading?: boolean;
}

export function RetryState({
  title = 'Bir sorun oluştu',
  message = 'Veriler alınamadı. Tekrar deneyin.',
  retryLabel = 'Tekrar Dene',
  onRetry,
  loading,
}: RetryStateProps) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-6 text-center">
      <div className="mx-auto flex h-10 w-10 items-center justify-center rounded-full bg-slate-100 text-slate-500">
        <RefreshCw className="h-5 w-5" />
      </div>
      <h3 className="mt-3 text-sm font-semibold text-slate-900">{title}</h3>
      <p className="mt-1 text-sm text-slate-500">{message}</p>
      <Button type="button" className="mt-4" variant="secondary" size="sm" onClick={onRetry} loading={loading}>
        {retryLabel}
      </Button>
    </div>
  );
}
