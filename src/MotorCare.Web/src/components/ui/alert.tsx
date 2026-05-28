'use client';

// src/components/ui/alert.tsx
import clsx from 'clsx';
import { CheckCircle, AlertCircle, Info, XCircle, X } from 'lucide-react';
import { useState } from 'react';

export type AlertType = 'success' | 'error' | 'info' | 'warning';

interface AlertProps {
  type: AlertType;
  message: string;
  dismissible?: boolean;
  className?: string;
}

const config: Record<AlertType, { className: string; Icon: React.ComponentType<{ size: number; className?: string }> }> = {
  success: { className: 'alert-success', Icon: CheckCircle },
  error: { className: 'alert-error', Icon: XCircle },
  info: { className: 'bg-blue-50 border border-blue-200 text-blue-700 rounded-lg px-4 py-3 text-sm', Icon: Info },
  warning: { className: 'bg-yellow-50 border border-yellow-200 text-yellow-700 rounded-lg px-4 py-3 text-sm', Icon: AlertCircle },
};

export function Alert({ type, message, dismissible, className }: AlertProps) {
  const [dismissed, setDismissed] = useState(false);
  if (dismissed || !message) return null;

  const { className: typeClass, Icon } = config[type];

  return (
    <div className={clsx(typeClass, 'flex items-start gap-2', className)}>
      <Icon size={16} className="mt-0.5 shrink-0" />
      <span className="flex-1">{message}</span>
      {dismissible && (
        <button onClick={() => setDismissed(true)} className="shrink-0 opacity-60 hover:opacity-100">
          <X size={14} />
        </button>
      )}
    </div>
  );
}
