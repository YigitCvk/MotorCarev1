'use client';

// src/components/ui/pagination.tsx
import clsx from 'clsx';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ currentPage, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center justify-center gap-1 py-3">
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="p-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:pointer-events-none"
      >
        <ChevronLeft size={16} />
      </button>

      {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
        const page = i + 1;
        return (
          <button
            key={page}
            onClick={() => onPageChange(page)}
            className={clsx(
              'min-w-[36px] h-9 rounded-lg border text-sm font-medium',
              currentPage === page
                ? 'border-brand-600 bg-brand-600 text-white'
                : 'border-slate-200 text-slate-600 hover:bg-slate-50'
            )}
          >
            {page}
          </button>
        );
      })}

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="p-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:pointer-events-none"
      >
        <ChevronRight size={16} />
      </button>
    </div>
  );
}
