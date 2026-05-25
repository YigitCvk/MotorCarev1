// src/components/ui/card.tsx
import clsx from 'clsx';

interface CardProps {
  children: React.ReactNode;
  className?: string;
  padding?: boolean;
}

export function Card({ children, className, padding = true }: CardProps) {
  return (
    <div className={clsx('card', padding && 'p-5', className)}>
      {children}
    </div>
  );
}

export interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: React.ReactNode;
  trend?: { value: number; label: string };
  color?: 'default' | 'green' | 'blue' | 'orange' | 'red';
}

const statColorClass: Record<string, string> = {
  default: 'text-brand-600',
  green: 'text-green-600',
  blue: 'text-blue-600',
  orange: 'text-orange-600',
  red: 'text-red-600',
};

const statIconBgClass: Record<string, string> = {
  default: 'bg-brand-50 text-brand-500',
  green: 'bg-green-50 text-green-500',
  blue: 'bg-blue-50 text-blue-500',
  orange: 'bg-orange-50 text-orange-500',
  red: 'bg-red-50 text-red-500',
};

export function StatCard({ title, value, subtitle, icon, trend, color = 'default' }: StatCardProps) {
  return (
    <div className="stat-card">
      <div className="flex items-start justify-between">
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-slate-500">{title}</p>
          <p className={clsx('mt-1 text-2xl font-bold truncate', statColorClass[color])}>{value}</p>
          {subtitle && <p className="mt-0.5 text-xs text-slate-400">{subtitle}</p>}
          {trend && (
            <p className={clsx('mt-1 text-xs font-medium', trend.value >= 0 ? 'text-green-600' : 'text-red-600')}>
              {trend.value >= 0 ? '+' : ''}{trend.value}% {trend.label}
            </p>
          )}
        </div>
        {icon && (
          <div className={clsx('rounded-lg p-2', statIconBgClass[color])}>
            {icon}
          </div>
        )}
      </div>
    </div>
  );
}
