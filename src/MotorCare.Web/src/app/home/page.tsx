import Link from 'next/link';
import type { Metadata } from 'next';
import {
  ClipboardList, Car, Package, FileSearch, QrCode, Users,
  CheckCircle, ArrowRight, Star, Shield, Zap, BarChart3,
  TrendingUp, Wrench, CalendarCheck,
} from 'lucide-react';
import { MobileMenuButton } from './MobileMenuButton';
import { FaqItem } from './FaqItem';

export const metadata: Metadata = {
  title: 'BakımSuite – Servis, Araç ve Expertiz Yönetimi',
  description: 'Oto servis, araç ve expertiz yönetimini tek panelden yönetin. Müşteri, stok, ödeme ve raporlama hepsi bir arada.',
};

// ---- Section data ----

const FEATURES = [
  {
    icon: ClipboardList,
    title: 'Servis Emri Yönetimi',
    description: 'İşçilik, parça ve sarf ürünlerini tek ekranda takip edin. Gerçek zamanlı maliyet hesaplama.',
  },
  {
    icon: Car,
    title: 'Müşteri ve Araç Takibi',
    description: 'Her müşteri ve aracın tam geçmişi. Plakadan anında servis geçmişine ulaşın.',
  },
  {
    icon: Package,
    title: 'Stok ve Parça Yönetimi',
    description: 'Kritik stok uyarıları, parça kullanım takibi ve otomatik stok güncellemesi.',
  },
  {
    icon: FileSearch,
    title: 'Dijital Expertiz Raporu',
    description: 'Araç kontrol listesi, hasar tespiti ve profesyonel PDF raporu oluşturun.',
  },
  {
    icon: QrCode,
    title: 'QR ile Anlık Paylaşım',
    description: 'Servis kaydı ve expertiz raporunu QR kodu ile müşteriye anında iletin.',
  },
  {
    icon: Users,
    title: 'Ekip ve Rol Yönetimi',
    description: 'Teknisyen, muhasebe ve yönetici rolleriyle erişim kontrolü sağlayın.',
  },
];

const STEPS = [
  { step: '01', title: 'İşletme Hesabı Oluşturun', description: 'Dakikalar içinde kayıt olun, işletme kodunuzu belirleyin.' },
  { step: '02', title: 'Müşteri ve Araç Ekleyin', description: 'Müşteri bilgilerini ve araç detaylarını sisteme kaydedin.' },
  { step: '03', title: 'Servis Emri Açın', description: 'Araç kabulünde servis emri oluşturun, şikayeti kaydedin.' },
  { step: '04', title: 'İşçilik ve Parça Ekleyin', description: 'Yapılan işleri, kullanılan parçaları ve fiyatları girin.' },
  { step: '05', title: 'Ödeme Alın ve Paylaşın', description: 'Faturayı onaylayın, QR ile müşteriye dijital kaydı gönderin.' },
];

const SECTORS = [
  { title: 'Oto Servisler', desc: 'Bağımsız ve zincir oto servis işletmeleri' },
  { title: 'Motosiklet Servisleri', desc: 'Motor ve scooter bakım atölyeleri' },
  { title: 'Ekspertiz Firmaları', desc: 'Araç inspeksiyon ve değerleme şirketleri' },
  { title: 'Özel Servis Zincirleri', desc: 'Çok şubeli servis ağları' },
];

const FAQS = [
  {
    q: 'Ücretsiz deneme var mı?',
    a: "Evet. Pilot dönemde seçili işletmelerle ücretsiz çalışıyoruz. Demo talep formunu doldurun, size ulaşalım.",
  },
  {
    q: 'Verilerim güvende mi?',
    a: "Tüm veriler Türkiye'de barındırılan sunucularda, şifreli bağlantılar üzerinden saklanır.",
  },
  {
    q: 'Kaç kullanıcı ekleyebilirim?',
    a: 'Teknisyen, muhasebeci ve yönetici dahil birden fazla kullanıcı ekleyebilirsiniz. Rol bazlı erişim kontrolü mevcuttur.',
  },
  {
    q: 'Mobil uygulama var mı?',
    a: 'Web versiyonu mobil uyumludur. Native mobil uygulama geliştirme yol haritamızda yer almaktadır.',
  },
  {
    q: 'Entegrasyon yapılabilir mi?',
    a: 'REST API sunan açık bir mimarimiz var. Muhasebe ve e-fatura entegrasyonları için bizimle iletişime geçin.',
  },
];

// ---- Dashboard preview stat cards ----

const PREVIEW_STATS = [
  { icon: TrendingUp, label: 'Toplam Gelir', value: '₺12.450', color: 'text-brand-600', bg: 'bg-brand-50' },
  { icon: Wrench, label: 'Aktif Servis', value: '8', color: 'text-amber-600', bg: 'bg-amber-50' },
  { icon: CalendarCheck, label: 'Bugün Tamamlanan', value: '5', color: 'text-green-600', bg: 'bg-green-50' },
];

// ---- Page ----

export default function LandingPage() {
  return (
    <div className="bg-white">
      {/* Nav */}
      <header className="sticky top-0 z-50 bg-white/80 backdrop-blur border-b border-slate-100">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center justify-between">
            {/* Logo */}
            <div className="flex items-center gap-2">
              <div className="h-8 w-8 rounded-lg bg-brand-600 flex items-center justify-center">
                <span className="text-white font-bold text-sm">B</span>
              </div>
              <span className="font-bold text-lg text-slate-900">BakımSuite</span>
            </div>

            {/* Desktop nav */}
            <nav className="hidden md:flex items-center gap-6">
              <a href="#features" className="text-sm text-slate-600 hover:text-slate-900">Özellikler</a>
              <a href="#how-it-works" className="text-sm text-slate-600 hover:text-slate-900">Nasıl Çalışır</a>
              <a href="#pricing" className="text-sm text-slate-600 hover:text-slate-900">Fiyatlar</a>
              <a href="#faq" className="text-sm text-slate-600 hover:text-slate-900">SSS</a>
            </nav>

            {/* Desktop CTAs + mobile hamburger */}
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
              {/* Mobile hamburger — client component */}
              <MobileMenuButton />
            </div>
          </div>
        </div>
      </header>

      {/* Hero */}
      <section className="relative overflow-hidden bg-gradient-to-b from-brand-800 via-brand-700 to-brand-600 text-white">
        <div
          className="absolute inset-0 opacity-10"
          style={{
            backgroundImage:
              'radial-gradient(circle at 20% 80%, white 1px, transparent 1px), radial-gradient(circle at 80% 20%, white 1px, transparent 1px)',
            backgroundSize: '60px 60px',
          }}
        />
        <div className="relative mx-auto max-w-7xl px-4 py-16 sm:px-6 sm:py-32 lg:px-8">
          <div className="max-w-3xl">
            <div className="inline-flex items-center gap-2 rounded-full bg-white/10 px-3 py-1 text-xs font-medium text-brand-100 mb-6 border border-white/20">
              <Zap size={12} />
              Pilot program açık — ücretsiz deneyin
            </div>
            <h1 className="text-3xl font-bold tracking-tight sm:text-5xl lg:text-6xl leading-tight">
              Servis, araç ve expertizi<br />
              <span className="text-brand-200">tek panelden</span> yönetin.
            </h1>
            <p className="mt-6 text-base sm:text-lg text-brand-100 max-w-2xl leading-relaxed">
              BakımSuite; müşteri, araç, servis emri, stok, ödeme ve expertiz süreçlerini
              dijitalleştiren modern servis yönetim platformudur.
            </p>
            <div className="mt-8 sm:mt-10 flex flex-col xs:flex-row sm:flex-row gap-3 sm:gap-4">
              <Link
                href="/register"
                className="inline-flex items-center justify-center gap-2 rounded-xl bg-white px-5 py-3 sm:px-6 sm:py-3.5 text-sm sm:text-base font-semibold text-brand-700 shadow-lg hover:bg-brand-50 transition-colors"
              >
                Ücretsiz Demo İste
                <ArrowRight size={18} />
              </Link>
              <Link
                href="/login"
                className="inline-flex items-center justify-center gap-2 rounded-xl border border-white/30 px-5 py-3 sm:px-6 sm:py-3.5 text-sm sm:text-base font-semibold text-white hover:bg-white/10 transition-colors"
              >
                Hesabım Var, Giriş Yap
              </Link>
            </div>
            <div className="mt-8 sm:mt-10 flex flex-wrap gap-4 sm:gap-6 text-xs sm:text-sm text-brand-200">
              <div className="flex items-center gap-1.5"><CheckCircle size={14} /> Kurulum gerektirmez</div>
              <div className="flex items-center gap-1.5"><CheckCircle size={14} /> Kredi kartı gerekmez</div>
              <div className="flex items-center gap-1.5"><CheckCircle size={14} /> 5 dakikada hazır</div>
            </div>
          </div>
        </div>
      </section>

      {/* Dashboard Preview */}
      <section className="py-16 sm:py-20 bg-slate-50">
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-8 sm:mb-10">
            <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">Paneli önizleyin</h2>
            <p className="mt-3 text-sm sm:text-base text-slate-500">
              Tüm servis verileriniz tek ekranda, gerçek zamanlı olarak.
            </p>
          </div>

          {/* Browser-frame widget */}
          <div className="rounded-2xl shadow-2xl border border-slate-200 overflow-hidden">
            {/* Browser chrome */}
            <div className="bg-slate-100 border-b border-slate-200 px-4 py-3 flex items-center gap-2">
              <div className="flex gap-1.5">
                <div className="h-3 w-3 rounded-full bg-red-400" />
                <div className="h-3 w-3 rounded-full bg-amber-400" />
                <div className="h-3 w-3 rounded-full bg-green-400" />
              </div>
              <div className="flex-1 mx-3 sm:mx-6">
                <div className="bg-white rounded-md px-3 py-1 text-xs text-slate-400 text-center truncate border border-slate-200">
                  app.bakimsuite.com/dashboard
                </div>
              </div>
              <div className="w-12 sm:w-16" />
            </div>

            {/* Dashboard body */}
            <div className="bg-white px-4 sm:px-6 py-6 sm:py-8">
              {/* Top bar */}
              <div className="flex flex-col xs:flex-row sm:flex-row items-start sm:items-center justify-between gap-3 mb-6 sm:mb-8">
                <div>
                  <h3 className="text-base sm:text-lg font-bold text-slate-900">Genel Bakış</h3>
                  <p className="text-xs sm:text-sm text-slate-500 mt-0.5">Bugün, 25 Mayıs 2026</p>
                </div>
                <div className="flex items-center gap-2 bg-brand-600 text-white text-xs font-semibold px-3 py-1.5 rounded-lg">
                  <span className="h-1.5 w-1.5 rounded-full bg-green-300 inline-block" />
                  Çevrimiçi
                </div>
              </div>

              {/* Stat cards */}
              <div className="grid grid-cols-1 xs:grid-cols-3 sm:grid-cols-3 gap-3 sm:gap-4 mb-6 sm:mb-8">
                {PREVIEW_STATS.map(({ icon: Icon, label, value, color, bg }) => (
                  <div key={label} className="rounded-xl border border-slate-100 bg-slate-50 p-4 sm:p-5">
                    <div className={`h-8 w-8 sm:h-9 sm:w-9 rounded-lg ${bg} flex items-center justify-center mb-3`}>
                      <Icon size={16} className={color} />
                    </div>
                    <p className="text-xs text-slate-500 mb-1">{label}</p>
                    <p className={`text-xl sm:text-2xl font-bold ${color}`}>{value}</p>
                  </div>
                ))}
              </div>

              {/* Mock recent orders table */}
              <div className="rounded-xl border border-slate-100 overflow-hidden">
                <div className="bg-slate-50 px-4 py-2.5 border-b border-slate-100">
                  <span className="text-xs font-semibold text-slate-600 uppercase tracking-wide">Son Servis Emirleri</span>
                </div>
                <div className="divide-y divide-slate-50">
                  {[
                    { plate: '34 ABC 123', owner: 'Ahmet Y.', status: 'Devam Ediyor', statusColor: 'bg-amber-100 text-amber-700' },
                    { plate: '06 XYZ 456', owner: 'Mehmet K.', status: 'Tamamlandı', statusColor: 'bg-green-100 text-green-700' },
                    { plate: '35 DEF 789', owner: 'Fatma S.', status: 'Beklemede', statusColor: 'bg-slate-100 text-slate-600' },
                  ].map(({ plate, owner, status, statusColor }) => (
                    <div key={plate} className="flex items-center justify-between px-4 py-3 hover:bg-slate-50 transition-colors">
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="h-7 w-7 rounded-lg bg-brand-50 flex items-center justify-center shrink-0">
                          <Car size={13} className="text-brand-600" />
                        </div>
                        <div className="min-w-0">
                          <p className="text-xs sm:text-sm font-semibold text-slate-800 truncate">{plate}</p>
                          <p className="text-xs text-slate-400 truncate">{owner}</p>
                        </div>
                      </div>
                      <span className={`shrink-0 text-xs font-medium px-2 py-0.5 rounded-full ${statusColor}`}>
                        {status}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Problem/Solution */}
      <section className="py-16 sm:py-20 bg-white">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="grid lg:grid-cols-2 gap-10 lg:gap-12 items-start lg:items-center">
            <div>
              <p className="text-sm font-semibold text-brand-600 uppercase tracking-wide mb-3">Sorun</p>
              <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 mb-6">Dağınık süreçler, kaçırılan gelir</h2>
              <div className="space-y-4">
                {[
                  'Kağıt tabanlı servis kayıtları kaybolabiliyor',
                  'Araç geçmişi bulunması zaman alıyor',
                  'Manuel stok takibinde hatalar oluyor',
                  'Ödeme ve tahsilat takibi karışıyor',
                  'Müşteri iletişimi telefon notlarında kalıyor',
                ].map((item) => (
                  <div key={item} className="flex items-start gap-3">
                    <div className="mt-0.5 h-5 w-5 rounded-full bg-red-100 flex items-center justify-center shrink-0">
                      <span className="text-red-600 text-xs font-bold">✕</span>
                    </div>
                    <span className="text-sm sm:text-base text-slate-600">{item}</span>
                  </div>
                ))}
              </div>
            </div>
            <div>
              <p className="text-sm font-semibold text-brand-600 uppercase tracking-wide mb-3">Çözüm</p>
              <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 mb-6">Her şey tek panelde, gerçek zamanlı</h2>
              <div className="space-y-4">
                {[
                  'Dijital servis emri, plakadan saniyede açılır',
                  'Araç ve müşteri geçmişi anında erişilebilir',
                  'Stok otomatik güncellenir, kritik uyarı alırsınız',
                  'Ödeme takibi ve kalan borç her an görünür',
                  'QR linkle müşteriye otomatik bildirim yapılır',
                ].map((item) => (
                  <div key={item} className="flex items-start gap-3">
                    <div className="mt-0.5 h-5 w-5 rounded-full bg-green-100 flex items-center justify-center shrink-0">
                      <CheckCircle size={12} className="text-green-600" />
                    </div>
                    <span className="text-sm sm:text-base text-slate-600">{item}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Features */}
      <section id="features" className="py-16 sm:py-20 bg-slate-50">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12 sm:mb-16">
            <p className="text-sm font-semibold text-brand-600 uppercase tracking-wide mb-3">Özellikler</p>
            <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 sm:text-4xl">
              İhtiyacınız olan her şey burada
            </h2>
            <p className="mt-4 text-base sm:text-lg text-slate-600 max-w-2xl mx-auto">
              Karmaşık kurulum yok. Modüler yapısıyla ihtiyaçlarınıza göre büyüyen bir platform.
            </p>
          </div>
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-8">
            {FEATURES.map(({ icon: Icon, title, description }) => (
              <div key={title} className="group rounded-2xl border border-slate-200 bg-white p-5 sm:p-6 hover:border-brand-300 hover:shadow-md transition-all">
                <div className="h-10 w-10 rounded-xl bg-brand-50 flex items-center justify-center mb-4 group-hover:bg-brand-100 transition-colors">
                  <Icon size={20} className="text-brand-600" />
                </div>
                <h3 className="text-sm sm:text-base font-semibold text-slate-900 mb-2">{title}</h3>
                <p className="text-xs sm:text-sm text-slate-600 leading-relaxed">{description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* How it works */}
      <section id="how-it-works" className="py-16 sm:py-20 bg-white">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12 sm:mb-16">
            <p className="text-sm font-semibold text-brand-600 uppercase tracking-wide mb-3">Nasıl Çalışır</p>
            <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 sm:text-4xl">5 adımda dijital servise geçin</h2>
          </div>
          <div className="max-w-3xl mx-auto space-y-5 sm:space-y-6">
            {STEPS.map(({ step, title, description }) => (
              <div key={step} className="flex gap-4 sm:gap-6 items-start">
                <div className="shrink-0 h-10 w-10 sm:h-12 sm:w-12 rounded-full bg-brand-600 flex items-center justify-center text-white font-bold text-xs sm:text-sm shadow-md">
                  {step}
                </div>
                <div className="pt-1">
                  <h3 className="text-sm sm:text-base font-semibold text-slate-900 mb-1">{title}</h3>
                  <p className="text-xs sm:text-sm text-slate-600">{description}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Stats */}
      <section className="py-16 sm:py-20 bg-brand-700">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-6 sm:gap-8 text-center text-white">
            {[
              { value: '10+', label: 'Pilot işletme' },
              { value: '500+', label: 'Servis kaydı' },
              { value: '%60', label: 'Daha az kağıt işi' },
              { value: '5dk', label: 'Kurulum süresi' },
            ].map(({ value, label }) => (
              <div key={label}>
                <div className="text-3xl sm:text-4xl font-bold text-white">{value}</div>
                <div className="mt-1 text-brand-200 text-xs sm:text-sm">{label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Sectors */}
      <section className="py-16 sm:py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10 sm:mb-12">
            <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">Kimler Kullanıyor?</h2>
          </div>
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
            {SECTORS.map(({ title, desc }) => (
              <div key={title} className="rounded-xl border border-slate-200 bg-white p-4 sm:p-6 text-center hover:border-brand-300 transition-colors">
                <h3 className="text-sm sm:text-base font-semibold text-slate-900 mb-1 sm:mb-2">{title}</h3>
                <p className="text-xs sm:text-sm text-slate-500">{desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Pricing */}
      <section id="pricing" className="py-16 sm:py-20 bg-slate-50">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10 sm:mb-12">
            <p className="text-sm font-semibold text-brand-600 uppercase tracking-wide mb-3">Fiyatlandırma</p>
            <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">Pilot dönemde ücretsiz</h2>
            <p className="mt-4 text-base sm:text-lg text-slate-600">
              Şu anda seçili işletmelerle pilot program yürütüyoruz.
              <br />Demo talep edin, sizinle iletişime geçelim.
            </p>
          </div>
          <div className="max-w-md mx-auto">
            <div className="rounded-2xl border-2 border-brand-600 bg-white p-6 sm:p-8 text-center shadow-xl">
              <div className="inline-flex items-center gap-1 rounded-full bg-brand-50 px-3 py-1 text-xs font-semibold text-brand-700 mb-4">
                <Star size={12} />
                Pilot Program
              </div>
              <div className="text-4xl sm:text-5xl font-bold text-slate-900 mb-2">Ücretsiz</div>
              <p className="text-slate-500 mb-6 sm:mb-8 text-sm sm:text-base">Pilot süresince tüm özellikler dahil</p>
              <ul className="text-left space-y-3 mb-6 sm:mb-8">
                {[
                  'Sınırsız servis kaydı',
                  'Stok ve parça yönetimi',
                  'Expertiz modülü',
                  'QR paylaşım',
                  'Öncelikli destek',
                  'Kurulum ve eğitim yardımı',
                ].map((item) => (
                  <li key={item} className="flex items-center gap-2 text-sm text-slate-700">
                    <CheckCircle size={16} className="text-brand-600 shrink-0" />
                    {item}
                  </li>
                ))}
              </ul>
              <Link
                href="/register"
                className="block w-full rounded-xl bg-brand-600 py-3 sm:py-3.5 text-center text-sm sm:text-base font-semibold text-white hover:bg-brand-700 transition-colors"
              >
                Demo Talep Et
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ */}
      <section id="faq" className="py-16 sm:py-20">
        <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10 sm:mb-12">
            <h2 className="text-2xl sm:text-3xl font-bold text-slate-900">Sık Sorulan Sorular</h2>
          </div>
          <div className="space-y-3 sm:space-y-4">
            {FAQS.map(({ q, a }) => (
              <FaqItem key={q} q={q} a={a} />
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="py-16 sm:py-20 bg-gradient-to-r from-brand-700 to-brand-600">
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8 text-center text-white">
          <h2 className="text-2xl sm:text-3xl font-bold sm:text-4xl mb-4">Servisinizi dijitalleştirmeye hazır mısınız?</h2>
          <p className="text-brand-200 mb-6 sm:mb-8 text-base sm:text-lg">
            Dakikalar içinde başlayın. Pilot program kapsamında ücretsiz.
          </p>
          <div className="flex flex-col xs:flex-row sm:flex-row gap-3 sm:gap-4 justify-center">
            <Link
              href="/register"
              className="inline-flex items-center justify-center gap-2 rounded-xl bg-white px-6 sm:px-8 py-3 sm:py-3.5 text-sm sm:text-base font-semibold text-brand-700 hover:bg-brand-50 transition-colors"
            >
              Hemen Başla
              <ArrowRight size={18} />
            </Link>
            <Link
              href="/login"
              className="inline-flex items-center justify-center gap-2 rounded-xl border border-white/30 px-6 sm:px-8 py-3 sm:py-3.5 text-sm sm:text-base font-semibold text-white hover:bg-white/10 transition-colors"
            >
              Hesabım Var
            </Link>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-slate-900 text-slate-400 py-10 sm:py-12">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-6 sm:gap-8 mb-6 sm:mb-8">
            <div className="col-span-2 sm:col-span-1">
              <div className="flex items-center gap-2 mb-3">
                <div className="h-7 w-7 rounded-lg bg-brand-600 flex items-center justify-center">
                  <span className="text-white font-bold text-xs">B</span>
                </div>
                <span className="font-semibold text-white">BakımSuite</span>
              </div>
              <p className="text-xs sm:text-sm leading-relaxed">Modern servis yönetim platformu.</p>
            </div>
            <div>
              <h4 className="text-white font-semibold mb-3 text-xs sm:text-sm">Ürün</h4>
              <ul className="space-y-2 text-xs sm:text-sm">
                <li><a href="#features" className="hover:text-white transition-colors">Özellikler</a></li>
                <li><a href="#pricing" className="hover:text-white transition-colors">Fiyatlar</a></li>
                <li><a href="#faq" className="hover:text-white transition-colors">SSS</a></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-semibold mb-3 text-xs sm:text-sm">Hesap</h4>
              <ul className="space-y-2 text-xs sm:text-sm">
                <li><Link href="/login" className="hover:text-white transition-colors">Giriş Yap</Link></li>
                <li><Link href="/register" className="hover:text-white transition-colors">Kayıt Ol</Link></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-semibold mb-3 text-xs sm:text-sm">Güvenlik</h4>
              <div className="flex items-center gap-2 text-xs sm:text-sm mb-1">
                <Shield size={14} />
                <span>SSL Şifreli</span>
              </div>
              <div className="flex items-center gap-2 text-xs sm:text-sm">
                <BarChart3 size={14} />
                <span>Türkiye Sunucuları</span>
              </div>
            </div>
          </div>
          <div className="border-t border-slate-800 pt-5 sm:pt-6 text-center text-xs text-slate-500">
            © {new Date().getFullYear()} BakımSuite. Tüm hakları saklıdır.
          </div>
        </div>
      </footer>
    </div>
  );
}
