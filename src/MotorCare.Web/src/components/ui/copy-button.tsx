'use client';

import { Check, Copy } from 'lucide-react';
import { useState } from 'react';
import { Button, type ButtonSize, type ButtonVariant } from './button';

export interface CopyButtonProps {
  value: string;
  label?: string;
  copiedLabel?: string;
  variant?: ButtonVariant;
  size?: ButtonSize;
  className?: string;
}

export function CopyButton({
  value,
  label = 'Kopyala',
  copiedLabel = 'Kopyalandı',
  variant = 'secondary',
  size = 'sm',
  className,
}: CopyButtonProps) {
  const [copied, setCopied] = useState(false);

  const copy = async () => {
    await navigator.clipboard.writeText(value);
    setCopied(true);
    window.setTimeout(() => setCopied(false), 1600);
  };

  return (
    <Button type="button" variant={variant} size={size} onClick={() => void copy()} className={className} icon={copied ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}>
      {copied ? copiedLabel : label}
    </Button>
  );
}
