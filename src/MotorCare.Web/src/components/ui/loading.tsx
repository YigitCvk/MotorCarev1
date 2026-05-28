// src/components/ui/loading.tsx
export function LoadingSpinner({ size = 24 }: { size?: number }) {
  return (
    <svg
      className="animate-spin text-brand-600"
      style={{ width: size, height: size }}
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
    </svg>
  );
}

export function PageLoading() {
  return (
    <div className="flex items-center justify-center py-20">
      <LoadingSpinner size={32} />
    </div>
  );
}

export function InlineLoading({ text = 'Yükleniyor...' }: { text?: string }) {
  return (
    <div className="flex items-center gap-2 py-8 justify-center text-slate-500">
      <LoadingSpinner size={18} />
      <span className="text-sm">{text}</span>
    </div>
  );
}
