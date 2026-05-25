// src/components/ui/badge.tsx
import clsx from 'clsx';

export type BadgeColor = 'green' | 'yellow' | 'red' | 'blue' | 'gray' | 'orange' | 'purple';

interface BadgeProps {
  color?: BadgeColor;
  children: React.ReactNode;
  className?: string;
}

const colorClass: Record<BadgeColor, string> = {
  green: 'badge-green',
  yellow: 'badge-yellow',
  red: 'badge-red',
  blue: 'badge-blue',
  gray: 'badge-gray',
  orange: 'bg-orange-100 text-orange-800',
  purple: 'bg-purple-100 text-purple-800',
};

export function Badge({ color = 'gray', children, className }: BadgeProps) {
  return <span className={clsx('badge', colorClass[color], className)}>{children}</span>;
}

// Status badge for ServiceOrder status
export type ServiceOrderStatus = 'Open' | 'InProgress' | 'Completed' | 'Cancelled' | 'WaitingForParts';

const STATUS_CONFIG: Record<ServiceOrderStatus, { label: string; color: BadgeColor }> = {
  Open: { label: 'Açık', color: 'blue' },
  InProgress: { label: 'Devam Ediyor', color: 'yellow' },
  WaitingForParts: { label: 'Parça Bekleniyor', color: 'orange' },
  Completed: { label: 'Tamamlandı', color: 'green' },
  Cancelled: { label: 'İptal Edildi', color: 'red' },
};

export function ServiceOrderStatusBadge({ status }: { status: string }) {
  const config = STATUS_CONFIG[status as ServiceOrderStatus] ?? { label: status, color: 'gray' as BadgeColor };
  return <Badge color={config.color}>{config.label}</Badge>;
}
