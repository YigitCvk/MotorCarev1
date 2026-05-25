import clsx from 'clsx';

export interface FormFieldProps {
  label: React.ReactNode;
  htmlFor?: string;
  children: React.ReactNode;
  hint?: React.ReactNode;
  error?: React.ReactNode;
  required?: boolean;
  className?: string;
}

export function FormField({ label, htmlFor, children, hint, error, required, className }: FormFieldProps) {
  return (
    <div className={clsx('space-y-1.5', className)}>
      <label htmlFor={htmlFor} className="block text-sm font-medium text-slate-700">
        {label}
        {required && <span className="ml-1 text-red-600">*</span>}
      </label>
      {children}
      {error ? <p className="text-xs font-medium text-red-600">{error}</p> : hint ? <p className="text-xs text-slate-500">{hint}</p> : null}
    </div>
  );
}
