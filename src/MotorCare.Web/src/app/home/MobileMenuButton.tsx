'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Menu, X } from 'lucide-react';

export function MobileMenuButton() {
  const [open, setOpen] = useState(false);

  return (
    <>
      {/* Hamburger trigger — only shown on mobile */}
      <button
        type="button"
        aria-label={open ? 'Menüyü kapat' : 'Menüyü aç'}
        aria-expanded={open}
        onClick={() => setOpen((v) => !v)}
        className="md:hidden inline-flex items-center justify-center rounded-lg p-2 text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition-colors"
      >
        {open ? <X size={22} /> : <Menu size={22} />}
      </button>

      {/* Slide-down mobile menu */}
      {open && (
        <div className="md:hidden absolute top-16 left-0 right-0 z-40 bg-white border-b border-slate-200 shadow-lg">
          <nav className="flex flex-col px-4 py-4 gap-1">
            <a
              href="#features"
              onClick={() => setOpen(false)}
              className="rounded-lg px-3 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-slate-900 transition-colors"
            >
              Özellikler
            </a>
            <a
              href="#how-it-works"
              onClick={() => setOpen(false)}
              className="rounded-lg px-3 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-slate-900 transition-colors"
            >
              Nasıl Çalışır
            </a>
            <a
              href="#pricing"
              onClick={() => setOpen(false)}
              className="rounded-lg px-3 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-slate-900 transition-colors"
            >
              Fiyatlar
            </a>
            <a
              href="#faq"
              onClick={() => setOpen(false)}
              className="rounded-lg px-3 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-slate-900 transition-colors"
            >
              SSS
            </a>
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
