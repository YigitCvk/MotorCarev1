'use client';

// src/components/ui/confirm-dialog.tsx
import clsx from 'clsx';
import { Modal } from './modal';

interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: 'danger' | 'default';
  loading?: boolean;
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  description,
  confirmLabel = 'Onayla',
  cancelLabel = 'İptal',
  variant = 'default',
  loading = false,
}: ConfirmDialogProps) {
  return (
    <Modal open={open} onClose={onClose} title={title} size="sm">
      <p className="text-sm text-slate-600 mb-6">{description}</p>
      <div className="flex justify-end gap-2">
        <button
          onClick={onClose}
          className="btn btn-secondary text-sm px-4 py-2"
          disabled={loading}
        >
          {cancelLabel}
        </button>
        <button
          onClick={onConfirm}
          disabled={loading}
          className={clsx('btn text-sm px-4 py-2', variant === 'danger' ? 'btn-danger' : 'btn-primary')}
        >
          {loading ? 'İşleniyor...' : confirmLabel}
        </button>
      </div>
    </Modal>
  );
}
