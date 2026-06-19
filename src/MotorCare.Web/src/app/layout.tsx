import type { Metadata } from 'next';
import './globals.css';
import { Providers } from '@/components/providers';
import { appConfig } from '@/shared/config/env';

export const dynamic = 'force-dynamic';
export const revalidate = 0;

export const metadata: Metadata = {
  metadataBase: new URL(appConfig.publicAppUrl),
  title: {
    default: `${appConfig.appName} - Servis Yönetim Platformu`,
    template: `%s | ${appConfig.appName}`,
  },
  description: 'Oto servis, araç ve expertiz yönetimini tek panelden yönetin.',
  keywords: ['oto servis yazılımı', 'araç bakım yönetimi', 'expertiz raporu', 'servis emri', 'stok takibi'],
  openGraph: {
    title: `${appConfig.appName} - Servis Yönetim Platformu`,
    description: 'Servis emri, müşteri, araç, stok, ödeme ve expertiz süreçlerini tek panelden yönetin.',
    url: appConfig.publicAppUrl,
    siteName: appConfig.appName,
    locale: 'tr_TR',
    type: 'website',
  },
  alternates: {
    canonical: '/',
  },
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="tr">
      <body>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
