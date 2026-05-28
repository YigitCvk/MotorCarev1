import Link from 'next/link';
import type { Metadata } from 'next';
import {
  ArrowRight,
  BarChart3,
  CalendarCheck,
  Car,
  CheckCircle,
  ClipboardList,
  FileSearch,
  Package,
  QrCode,
  Shield,
  Star,
  TrendingUp,
  Users,
  Wrench,
  Zap,
} from 'lucide-react';
import { appConfig } from '@/shared/config/env';
import { FaqItem } from './FaqItem';
import { MobileMenuButton } from './MobileMenuButton';

export const metadata: Metadata = {
  title: 'Oto Servis, Araç ve Expertiz Yönetimi',
  description: 'Servis emri, müşteri, araç, stok, ödeme ve expertiz süreçlerini tek panelden yönetin.',
  alternates: { canonical: '/home' },
  openGraph: {
    title: `${appConfig.appName} - Oto Servis Yönetim Platformu`,
    description: 'Oto servis, motosiklet servisleri ve expertiz firmaları için modern işletme paneli.',
    url: `${appConfig.publicAppUrl}/home`,
    type: 'website',
  },
};

const FEATURES = [
  {
    icon: ClipboardList,
    title: 'Servis Emri Yönetimi',
    description: 'İşçilik, parça ve sarf ürünlerini tek ekranda takip edin. Maliyet ve durum bilgisi anlık güncellenir.',
  },
  {
    icon: Car,
    title: 'Müşteri ve Araç Takibi',
    description: 'Plakadan araç geçmişine, müşteri notlarına ve önceki işlemlere saniyeler içinde ulaşın.',
  },
  {
    icon: Package,
    title: 'Stok ve Parça Yönetimi',
    description: 'Kritik stok uyarıları, parça kullanım takibi ve servis emrine bağlı otomatik düşümler.',
  },
  {
    icon: FileSearch,
    title: 'Dijital Expertiz Raporu',
    description: 'Kontrol listeleri, fotoğraflar ve paylaşılabilir raporlarla expertiz sürecini standartlaştırın.',
  },
  {
    icon: QrCode,
    title: 'QR ile Paylaşım',
    description: 'Servis kaydı ve expertiz raporunu müşteriye güvenli bağlantıyla hızlıca iletin.',
  },
  {
    icon: Users,
    title: 'Ekip ve Rol Yönetimi',
    description: 'Yönetici, teknisyen, eksper ve muhasebe rolleriyle erişimleri doğru seviyede tutun.',
  },
];

const STEPS = [
  { step: '01', title: 'İşletme hesabı oluşturun', description: 'Dakikalar içinde kayıt olun ve işletme kodunuzu belirleyin.' },
  { step: '02', title: 'Müşteri ve araç ekleyin', description: 'Plaka, iletişim ve geçmiş bilgilerini tek profilde toplayın.' },
  { step: '03', title: 'Servis emri açın', description: 'Araç kabulünde şikayet, işçilik ve parça kalemlerini kaydedin.' },
  { step: '04', title: 'İşi yönetin', description: 'Teknisyen, stok, ödeme ve durum bilgisini aynı akışta takip edin.' },
  { step: '05', title: 'Kaydı paylaşın', description: 'Tamamlanan işi QR bağlantısıyla müşteriye dijital olarak gönderin.' },
];

const FAQS = [
  {
    q: 'Ücretsiz deneme var mı?',
    a: 'Evet. Pilot dönemde seçili işletmelerle ücretsiz çalışıyoruz. İşletme hesabı oluşturduktan sonra ekibimiz sizinle iletişime geçer.',
  },
  {
    q: 'Hangi servis tipleri için uygun?',
    a: 'Oto servisler, motosiklet servisleri, özel servis zincirleri ve expertiz firmaları için uygundur.',
  },
  {
    q: 'Kaç kullanıcı ekleyebilirim?',
    a: 'Teknisyen, eksper, muhasebe ve yönetici dahil birden fazla kullanıcı ekleyebilirsiniz. Yetkiler rol bazlı yönetilir.',
  },
  {
    q: 'Mobilde kullanılabilir mi?',
    a: 'Web uygulaması mobil uyumludur. Ekipler servis kabul, takip ve rapor görüntüleme işlerini telefondan yapabilir.',
  },
];

const PREVIEW_STATS = [
  { icon: TrendingUp, label: 'Aylık Gelir', value: '₺128.450', color: 'text-brand-600', bg: 'bg-brand-50' },
  { icon: Wrench, label: 'Aktif Servis', value: '18', color: 'text-amber-600', bg: 'bg-amber-50' },
  { icon: CalendarCheck, label: 'Bugün Tamamlanan', value: '7', color: 'text-green-600', bg: 'bg-green-50' },
];

export default function LandingPage() {
  return (
    <div className="bg-white">
      <header className="sticky top-0 z-50 bg-white/90 backdrop-blur border-b border-slate-100">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center justify-between">
            <Link href="/home" className="flex items-center gap-2" aria-label={`${appConfig.appName} ana sayfa`}>
              <div className="h-8 w-8 rounded-lg bg-brand-600 flex items-center justify-center">
                <span className="text-white font-bold text-xs">GP</span>
              </div>
              <span className="font-bold text-lg text-slate-900">{appConfig.appName}</span>
            </Link>

            <nav className="hidden md:flex items-center gap-6">
              <a href="#features" className="text-sm text-slate-600 hover:text-slate-900">Özellikler</a>
              <a href="#how-it-works" className="text-sm text-slate-600 hover:text-slate-900">Nasıl Çalışır</a>
              <a href="#pricing" className="text-sm text-slate-600 hover:text-slate-900">Fiyatlar</a>
              <a href="#faq" className="text-sm text-slate-600 hover:text-slate-900">SSS</a>
            </nav>

            <div className="flex items-center gap-2 sm:gap-3">
              <Link href="/login" className="hidden sm:inline text-sm font-medium text-slate-700 hover:text-slate-900">
                Giriş Yap
              </Link>
              <Link
                href="/register"
                className="hidden sm:inline-flex items-center rounded-lg bg-brand-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-brand-700 transition-colors"
              >
                Ücretsiz Başla
              </Link>
              <MobileMenuButton />
            </div>
          </div>
        </div>
      </header>

      <main>
        <section className="bg-slate-950 text-white">
          <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 sm:py-24 lg:px-8">
            <div className="grid gap-10 lg:grid-cols-[1fr_480px] lg:items-center">
              <div>
                <div className="inline-flex items-center gap-2 rounded-full bg-white/10 px-3 py-1 text-xs font-medium text-brand-100 mb-6 border border-white/15">
                  <Zap size={12} />
                  Pilot program açık, kredi kartı gerekmez
                </div>
                <h1 className="text-3xl font-bold tracking-tight sm:text-5xl lg:text-6xl leading-tight">
                  Servis, araç ve expertiz yönetimini tek panelde toplayın.
                </h1>
                <p className="mt-6 text-base sm:text-lg text-slate-300 max-w-2xl leading-relaxed">
                  {appConfig.appName}; servis emri, müşteri, araç, stok, ödeme ve expertiz süreçlerini
                  sahadaki ekiplerin hızlı kullanabileceği modern bir panele taşır.
                </p>
                <div className="mt-8 flex flex-col sm:flex-row gap-3 sm:gap-4">
                  <Link
                    href="/register"
                    className="inline-flex items-center justify-center gap-2 rounded-xl bg-white px-5 py-3 text-sm sm:text-base font-semibold text-slate-950 shadow-lg hover:bg-slate-100 transition-colors"
                  >
                    Ücretsiz Demo İste
                    <ArrowRight size={18} />
                  </Link>
                  <Link
                    href="/login"
                    className="inline-flex items-center justify-center gap-2 rounded-xl border border-white/25 px-5 py-3 text-sm sm:text-base font-semibold text-white hover:bg-white/10 transition-colors"
                  >
                    Hesabım Var
                  </Link>
                </div>
                <div className="mt-8 flex flex-wrap gap-4 text-xs sm:text-sm text-slate-300">
                  <div className="flex items-center gap-1.5"><CheckCircle size={14} /> Kurulum gerektirmez</div>
                  <div className="flex items-center gap-1.5"><CheckCircle size={14} /> Rol bazlı erişim</div>
                  <div className="flex items-center gap-1.5"><CheckCircle size={14} /> Mobil uyumlu</div>
                </div>
              </div>

              <div className="rounded-2xl border border-white/10 bg-white text-slate-900 shadow-2xl overflow-hidden">
                <div className="bg-slate-100 border-b border-slate-200 px-4 py-3 flex items-center gap-2">
                  <div className="flex gap-1.5" aria-hidden>
                    <div className="h-3 w-3 rounded-full bg-red-400" />
                    <div className="h-3 w-3 rounded-full bg-amber-400" />
                    <div className="h-3 w-3 rounded-full bg-green-400" />
                  </div>
                  <div className="flex-1 mx-3 rounded-md border border-slate-200 bg-white px-3 py-1 text-center text-xs text-slate-400 truncate">
                    app.garajpass.com/dashboard
                  </div>
                </div>
                <div className="p-5 sm:p-6">
                  <div className="flex items-start justify-between gap-3 mb-6">
                    <div>
                      <h2 className="text-base font-bold text-slate-900">Genel Bakış</h2>
                      <p className="text-xs text-slate-500 mt-0.5">Canlı servis operasyonu</p>
                    </div>
                    <div className="flex items-center gap-2 rounded-lg bg-green-50 px-3 py-1.5 text-xs font-semibold text-green-700">
                      <span className="h-1.5 w-1.5 rounded-full bg-green-500" />
                      Çevrimiçi
                    </div>
                  </div>

                  <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 mb-6">
                    {PREVIEW_STATS.map(({ icon: Icon, label, value, color, bg }) => (
                      <div key={label} className="rounded-xl border border-slate-100 bg-slate-50 p-4">
                        <div className={`h-8 w-8 rounded-lg ${bg} flex items-center justify-center mb-3`}>
                          <Icon size={16} className={color} />
                        </div>
                        <p className="text-xs text-slate-500 mb-1">{label}</p>
                        <p className={`text-xl font-bold ${color}`}>{value}</p>
                      </div>
                    ))}
                  </div>

                  <div className="rounded-xl border border-slate-100 overflow-hidden">
                    <div className="bg-slate-50 px-4 py-2.5 border-b border-slate-100">
                      <span className="text-xs font-semibold text-slate-600 uppercase">Son Servis Emirleri</span>
                    </div>
                    {[
                      { plate: '34 ABC 123', owner: 'Ahmet Y.', status: 'Devam Ediyor', style: 'bg-amber-100 text-amber-700' },
                      { plate: '06 XYZ 456', owner: 'Mehmet K.', status: 'Tamamlandı', style: 'bg-green-100 text-green-700' },
                      { plate: '35 DEF 789', owner: 'Fatma S.', status: 'Beklemede', style: 'bg-slate-100 text-slate-600' },
                    ].map((order) => (
                      <div key={order.plate} className="flex items-center justify-between gap-3 px-4 py-3 border-b border-slate-50 last:border-0">
                        <div className="flex items-center gap-3 min-w-0">
                          <div className="h-7 w-7 rounded-lg bg-brand-50 flex items-center justify-center shrink-0">
                            <Car size={13} className="text-brand-600" />
                          </div>
                          <div className="min-w-0">
                            <p className="text-sm font-semibold text-slate-800 truncate">{order.plate}</p>
                            <p className="text-xs text-slate-400 truncate">{order.owner}</p>
                          </div>
                        </div>
                        <span className={`shrink-0 text-xs font-medium px-2 py-0.5 rounded-full ${order.style}`}>
                          {order.status}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        <section id="features" className="py-16 sm:py-20 bg-white">
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="max-w-2xl mb-10 sm:mb-12">
              <p className="text-sm font-semibold text-brand-600 uppercase tracking-wide mb-3">Özellikler</p>
              <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">Servis akışının tüm kritik parçaları</h2>
              <p className="mt-4 text-base text-slate-600">
                Dağınık evrak, ayrı tablolar ve manuel takip yerine tek bir operasyon ekranı kullanın.
              </p>
            </div>
            <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6">
              {FEATURES.map(({ icon: Icon, title, description }) => (
                <div key={title} className="rounded-xl border border-slate-200 bg-white p-5 sm:p-6 hover:border-brand-300 hover:shadow-md transition-all">
                  <div className="h-10 w-10 rounded-lg bg-brand-50 flex items-center justify-center mb-4">
                    <Icon size={20} className="text-brand-600" />
                  </div>
                  <h3 className="text-base font-semibold text-slate-900 mb-2">{title}</h3>
                  <p className="text-sm text-slate-600 leading-relaxed">{description}</p>
                </div>
              ))}
            </div>
          </div>
        </section>

        <section id="how-it-works" className="py-16 sm:py-20 bg-slate-50">
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="grid gap-10 lg:grid-cols-[360px_1fr]">
              <div>
                <p className="text-sm font-semibold text-brand-600 uppercase tracking-wide mb-3">Nasıl Çalışır</p>
                <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">5 adımda dijital servis operasyonu</h2>
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                {STEPS.map(({ step, title, description }) => (
                  <div key={step} className="rounded-xl border border-slate-200 bg-white p-5">
                    <div className="mb-4 flex h-9 w-9 items-center justify-center rounded-full bg-brand-600 text-xs font-bold text-white">
                      {step}
                    </div>
                    <h3 className="font-semibold text-slate-900 mb-1">{title}</h3>
                    <p className="text-sm text-slate-600">{description}</p>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </section>

        <section id="pricing" className="py-16 sm:py-20 bg-white">
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-xl text-center">
              <p className="text-sm font-semibold text-brand-600 uppercase tracking-wide mb-3">Fiyatlandırma</p>
              <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">Pilot dönemde ücretsiz</h2>
              <p className="mt-4 text-base text-slate-600">
                Seçili işletmelerle pilot program yürütüyoruz. Hesap oluşturun, demo ve kurulum için size ulaşalım.
              </p>
            </div>
            <div className="max-w-md mx-auto mt-10 rounded-2xl border-2 border-brand-600 bg-white p-6 sm:p-8 text-center shadow-xl">
              <div className="inline-flex items-center gap-1 rounded-full bg-brand-50 px-3 py-1 text-xs font-semibold text-brand-700 mb-4">
                <Star size={12} />
                Pilot Program
              </div>
              <div className="text-4xl sm:text-5xl font-bold text-slate-900 mb-2">Ücretsiz</div>
              <p className="text-slate-500 mb-6 text-sm sm:text-base">Pilot süresince tüm ana modüller dahil</p>
              <ul className="text-left space-y-3 mb-6">
                {['Servis emri ve araç yönetimi', 'Stok ve parça takibi', 'Expertiz raporu', 'QR paylaşım', 'Rol bazlı ekip yönetimi'].map((item) => (
                  <li key={item} className="flex items-center gap-2 text-sm text-slate-700">
                    <CheckCircle size={16} className="text-brand-600 shrink-0" />
                    {item}
                  </li>
                ))}
              </ul>
              <Link
                href="/register"
                className="block w-full rounded-xl bg-brand-600 py-3 text-center text-sm sm:text-base font-semibold text-white hover:bg-brand-700 transition-colors"
              >
                Demo Talep Et
              </Link>
            </div>
          </div>
        </section>

        <section id="faq" className="py-16 sm:py-20 bg-slate-50">
          <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-10">
              <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">Sık Sorulan Sorular</h2>
            </div>
            <div className="space-y-3">
              {FAQS.map(({ q, a }) => (
                <FaqItem key={q} q={q} a={a} />
              ))}
            </div>
          </div>
        </section>

        <section className="py-16 sm:py-20 bg-slate-950">
          <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8 text-center text-white">
            <h2 className="text-2xl sm:text-3xl font-bold mb-4">Servisinizi dijitalleştirmeye hazır mısınız?</h2>
            <p className="text-slate-300 mb-8 text-base sm:text-lg">
              Dakikalar içinde başlayın. Pilot program kapsamında ücretsiz demo alın.
            </p>
            <div className="flex flex-col sm:flex-row gap-3 sm:gap-4 justify-center">
              <Link
                href="/register"
                className="inline-flex items-center justify-center gap-2 rounded-xl bg-white px-6 py-3 text-sm sm:text-base font-semibold text-slate-950 hover:bg-slate-100 transition-colors"
              >
                Hemen Başla
                <ArrowRight size={18} />
              </Link>
              <Link
                href="/login"
                className="inline-flex items-center justify-center gap-2 rounded-xl border border-white/25 px-6 py-3 text-sm sm:text-base font-semibold text-white hover:bg-white/10 transition-colors"
              >
                Hesabım Var
              </Link>
            </div>
          </div>
        </section>
      </main>

      <footer className="bg-slate-900 text-slate-400 py-10 sm:py-12">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-6 sm:gap-8 mb-8">
            <div className="col-span-2 lg:col-span-1">
              <div className="flex items-center gap-2 mb-3">
                <div className="h-7 w-7 rounded-lg bg-brand-600 flex items-center justify-center">
                  <span className="text-white font-bold text-[10px]">GP</span>
                </div>
                <span className="font-semibold text-white">{appConfig.appName}</span>
              </div>
              <p className="text-sm leading-relaxed">Modern servis yönetim platformu.</p>
            </div>
            <div>
              <h3 className="text-white font-semibold mb-3 text-sm">Ürün</h3>
              <ul className="space-y-2 text-sm">
                <li><a href="#features" className="hover:text-white transition-colors">Özellikler</a></li>
                <li><a href="#pricing" className="hover:text-white transition-colors">Fiyatlar</a></li>
                <li><a href="#faq" className="hover:text-white transition-colors">SSS</a></li>
              </ul>
            </div>
            <div>
              <h3 className="text-white font-semibold mb-3 text-sm">Hesap</h3>
              <ul className="space-y-2 text-sm">
                <li><Link href="/login" className="hover:text-white transition-colors">Giriş Yap</Link></li>
                <li><Link href="/register" className="hover:text-white transition-colors">Kayıt Ol</Link></li>
              </ul>
            </div>
            <div>
              <h3 className="text-white font-semibold mb-3 text-sm">Güvenlik</h3>
              <div className="flex items-center gap-2 text-sm mb-2">
                <Shield size={14} />
                <span>SSL şifreli bağlantı</span>
              </div>
              <div className="flex items-center gap-2 text-sm">
                <BarChart3 size={14} />
                <span>Rol bazlı erişim</span>
              </div>
            </div>
          </div>
          <div className="border-t border-slate-800 pt-6 text-center text-xs text-slate-500">
            © {new Date().getFullYear()} {appConfig.appName}. Tüm hakları saklıdır.
          </div>
        </div>
      </footer>
    </div>
  );
}
