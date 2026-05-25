'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { Menu, X } from 'lucide-react';

const MENU_ITEMS = [
  { href: '#features', label: 'Özellikler' },
  { href: '#how-it-works', label: 'Nasıl Çalışır' },
  { href: '#pricing', label: 'Fiyatlar' },
  { href: '#faq', label: 'SSS' },
];

export function MobileMenuButton() {
  const [open, setOpen] = useState(false);

  useEffect(() => {
    function closeOnEscape(event: KeyboardEvent) {
      if (event.key === 'Escape') setOpen(false);
    }

    document.addEventListener('keydown', closeOnEscape);
    return () => document.removeEventListener('keydown', closeOnEscape);
  }, []);

  return (
    <>
      <button
        type="button"
        aria-label={open ? 'Menüyü kapat' : 'Menüyü aç'}
        aria-expanded={open}
        aria-controls="mobile-navigation"
        onClick={() => setOpen((v) => !v)}
        className="md:hidden inline-flex h-10 w-10 items-center justify-center rounded-lg text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition-colors"
      >
        {open ? <X size={22} /> : <Menu size={22} />}
      </button>

      {open && (
        <div id="mobile-navigation" className="md:hidden absolute top-16 left-0 right-0 z-40 bg-white border-b border-slate-200 shadow-lg">
          <nav className="flex flex-col px-4 py-4 gap-1">
            {MENU_ITEMS.map((item) => (
              <a
                key={item.href}
                href={item.href}
                onClick={() => setOpen(false)}
                className="rounded-lg px-3 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-slate-900 transition-colors"
              >
                {item.label}
              </a>
            ))}
            <div className="mt-3 pt-3 border-t border-slate-100 flex flex-col gap-2">
              <Link
                href="/login"
                onClick={() => setOpen(false)}
                className="rounded-lg px-3 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-slate-900 transition-colors text-center"
              >
                Giriş Yap
              </Link>
              <Link
                href="/register"
                onClick={() => setOpen(false)}
                className="rounded-xl bg-brand-600 px-4 py-2.5 text-sm font-semibold text-white text-center hover:bg-brand-700 transition-colors"
              >
                Ücretsiz Başla
              </Link>
            </div>
          </nav>
        </div>
      )}
    </>
  );
}
