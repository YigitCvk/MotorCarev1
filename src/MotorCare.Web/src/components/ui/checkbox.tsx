import clsx from 'clsx';
import { type InputHTMLAttributes, forwardRef } from 'react';

export interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label?: React.ReactNode;
  description?: React.ReactNode;
  error?: string;
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ label, description, error, className, id, disabled, ...props }, ref) => {
    const control = (
      <input
        ref={ref}
        id={id}
        type="checkbox"
        disabled={disabled}
        aria-invalid={error ? true : undefined}
        className={clsx(
          'mt-0.5 h-4 w-4 rounded border-slate-300 text-brand-600 focus:ring-2 focus:ring-brand-500 disabled:cursor-not-allowed disabled:opacity-50',
          className
        )}
        {...props}
      />
    );

    if (!label && !description && !error) return control;

    return (
      <label className={clsx('flex items-start gap-3 text-sm', disabled && 'cursor-not-allowed opacity-70')}>
        {control}
        <span className="min-w-0">
          {label && <span className="block font-medium text-slate-700">{label}</span>}
          {description && <span className="block text-xs text-slate-500">{description}</span>}
          {error && <span className="block text-xs font-medium text-red-600">{error}</span>}
        </span>
      </label>
    );
  }
);
Checkbox.displayName = 'Checkbox';
