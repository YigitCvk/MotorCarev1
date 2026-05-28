import Link from 'next/link';
import { ChevronRight, Home } from 'lucide-react';

export interface BreadcrumbItem {
  label: string;
  href?: string;
}

interface BreadcrumbsProps {
  items: BreadcrumbItem[];
  className?: string;
  homeHref?: string;
}

export function Breadcrumbs({ items, className = '', homeHref = '/dashboard' }: BreadcrumbsProps) {
  if (items.length === 0) return null;

  return (
    <nav className={`flex items-center gap-1 text-sm ${className}`} aria-label="Breadcrumb">
      <Link href={homeHref} className="text-slate-400 hover:text-slate-600 flex-shrink-0">
        <Home size={14} />
      </Link>
      {items.map((item, idx) => {
        const isLast = idx === items.length - 1;
        return (
          <span key={idx} className="flex items-center gap-1">
            <ChevronRight size={14} className="text-slate-300 flex-shrink-0" />
            {isLast || !item.href ? (
              <span className="text-slate-700 font-medium truncate max-w-[200px]">{item.label}</span>
            ) : (
              <Link href={item.href} className="text-slate-500 hover:text-slate-700 truncate max-w-[160px]">
                {item.label}
              </Link>
            )}
          </span>
        );
      })}
    </nav>
  );
}
