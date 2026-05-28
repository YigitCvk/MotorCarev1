import Link from 'next/link';

export default function NotFound() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-slate-200">404</h1>
        <p className="mt-4 text-slate-600">Sayfa bulunamadı.</p>
        <Link href="/" className="mt-6 inline-block btn-primary">Ana Sayfaya Dön</Link>
      </div>
    </div>
  );
}
