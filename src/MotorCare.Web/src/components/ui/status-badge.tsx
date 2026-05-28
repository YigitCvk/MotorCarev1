import { Badge, type BadgeColor } from './badge';

export type StatusBadgeTone = BadgeColor | 'success' | 'warning' | 'danger' | 'info' | 'neutral';

export interface StatusBadgeProps {
  status: React.ReactNode;
  tone?: StatusBadgeTone;
  className?: string;
}

const toneColor: Record<StatusBadgeTone, BadgeColor> = {
  green: 'green',
  yellow: 'yellow',
  red: 'red',
  blue: 'blue',
  gray: 'gray',
  orange: 'orange',
  purple: 'purple',
  success: 'green',
  warning: 'yellow',
  danger: 'red',
  info: 'blue',
  neutral: 'gray',
};

export function StatusBadge({ status, tone = 'neutral', className }: StatusBadgeProps) {
  return (
    <Badge color={toneColor[tone]} className={className}>
      {status}
    </Badge>
  );
}
