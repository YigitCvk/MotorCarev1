'use client';

import Link from 'next/link';
import { AlertTriangle, RotateCcw } from 'lucide-react';

export default function InspectionsError({
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="min-h-[60vh] flex items-center justify-center px-4">
      <div className="w-full max-w-lg rounded-xl border border-red-100 bg-white p-6 text-center shadow-sm">
        <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-red-50 text-red-600">
          <AlertTriangle size={22} />
        </div>
        <h1 className="text-lg font-semibold text-slate-900">
          Expertiz ekranı yüklenirken bir sorun oluştu.
        </h1>
        <p className="mt-2 text-sm text-slate-500">Lütfen tekrar deneyin.</p>
        <div className="mt-5 flex flex-col gap-2 sm:flex-row sm:justify-center">
          <button type="button" onClick={reset} className="btn-primary">
            <RotateCcw size={16} />
            Tekrar Dene
          </button>
          <Link href="/inspections" className="btn-secondary justify-center">
            Expertiz Listesine Dön
          </Link>
        </div>
      </div>
    </div>
  );
}
