# MotorCare — Ürün Keşif ve Pazar Analizi

**Tarih:** 2026-04-18  
**Rol:** Product Manager + Business Analyst  
**Kaynak:** Kaynak kodu incelemesi (domain modeli, servis akışları, veri yapıları)

---

## 1. KODDAN ÇIKARTILAN İŞ MODELİ

Önce teknik değil, iş gözü ile kodun ne anlattığına bakalım.

### Kodun anlattığı 10 şey

| Kod Gözlemi | İş Anlamı |
|---|---|
| `PlateNumber` Türk plaka formatını normalize ediyor | Türkiye pazarı için yazılmış |
| `Whatsapp` müşteri entity'sinde birinci sınıf alan | Türkiye'de iş iletişimi WhatsApp üzerinden yürüyor |
| `ServiceOrder` → `Open → InProgress → WaitingForParts → Completed → Delivered` | Araç teslim süreci: araç gelir, tamir edilir, müşteriye teslim edilir |
| `LaborTotal`, `PartsTotal`, `DiscountTotal`, `GrandTotal`, `PaidTotal` ayrı ayrı | Fatura kesmek, işçilik + yedek parça ayrımı, taksit/kısmi ödeme takibi |
| `VehicleKm` servis emrinde tutulur | Her serviste kilometre kaydı — bakım geçmişi |
| `OrderNo`: `SRV-20260418-0001` | Günlük sıralı iş emri numarası — kağıt iş emri sistemini taklit ediyor |
| `WaitingForParts` durumu | Yedek parça beklemek oto servislerin en büyük gecikme sebebi |
| `PaymentMethod`: Nakit / Kredi Kartı / Havale | Küçük servisler her üç ödeme yöntemini kullanır |
| `InternalNote` (müşteriye gösterilmez) | Teknisyenler arası iç not — saha gerçekliği |
| Multi-tenant + TenantId her yerde | B2B SaaS: her servis dükkanı ayrı bir kiracı |

**Sonuç:** Bu kod, **küçük ve orta ölçekli oto servislerini dijitalleştirmek** için yazılmıştır. Kağıt tabanlı iş emri defterinin, tel defterin ve aklın ucundaki "o araç ne zaman geldi?" sorusunun dijital karşılığıdır.

---

## 2. DOMAIN ANALİZİ

### Bounded Context Haritası

```
┌────────────────────────────────────────────────────────────────┐
│                    MotorCare Platform                          │
│                                                                │
│  ┌─────────────┐    ┌─────────────┐    ┌──────────────────┐  │
│  │   TENANT    │    │  CUSTOMER   │    │    VEHICLE       │  │
│  │ MANAGEMENT  │    │ MANAGEMENT  │    │   MANAGEMENT     │  │
│  │             │    │             │    │                  │  │
│  │ Tenant kimliği│  │ Müşteri     │    │ Araç kaydı       │  │
│  │ ve aktiflik │    │ iletişim    │    │ Plaka, marka     │  │
│  │ yönetimi    │    │ bilgileri   │    │ model, km        │  │
│  └──────┬──────┘    └──────┬──────┘    └────────┬─────────┘  │
│         │                  │                     │            │
│         └──────────────────┴─────────────────────┘            │
│                            │                                   │
│                   ┌────────▼────────┐                         │
│                   │    SERVICE      │                         │
│                   │   MANAGEMENT   │                          │
│                   │                │                          │
│                   │ İş emri akışı  │                          │
│                   │ İşçilik+Parça  │                          │
│                   │ Ödeme takibi   │                          │
│                   └────────────────┘                         │
└────────────────────────────────────────────────────────────────┘
```

### Mevcut Aggregate'ler ve Olgunluk Düzeyleri

| Aggregate | Ne Yapıyor | Olgunluk |
|---|---|---|
| **Tenant** | Servis dükkanı kimliği, aktif/pasif | Temel — yeterli |
| **Customer** | Müşteri bilgileri, telefon, WhatsApp | İyi — işe yarar |
| **Vehicle** | Araç kaydı, foto, not, müşteri bağlantısı | İyi — temel ihtiyaçları karşılıyor |
| **ServiceOrder** | İş emri tam döngüsü, finansal hesaplar | Güçlü — en olgun aggregate |

### Multi-Tenant Yapısı

Her işletme (servis) kendi "tenant"ıdır. Veriler birbirinden izole; aynı müşteri iki farklı servise gittiğinde birbirinin müşterisini göremez. Bu doğru ve güvenli kurulmuş.

### İmplemente Edilmiş İş Akışları

**Tam çalışıyor (kod var):**
- Müşteri kayıt ve güncelleme
- Araç kayıt ve müşteriye bağlama
- İş emri açma ve kilometre kaydı
- İş emri durum yönetimi (state machine)
- İşçilik kalemi ekleme
- Yedek parça ekleme
- Ödeme ekleme (nakit/kart/havale)
- Günlük iş emri numarası üretimi

**Kod altyapısı var ama henüz bitmemiş:**
- API endpoint'leri (sadece araç için var, servis emri endpoint'leri henüz yok)
- Sorgulama akışları (hangi araçlar serviste? bugün ne geldi?)

**Henüz hiç başlanmamış (domain'de de yok):**
- Randevu sistemi
- WhatsApp / SMS bildirim
- Müşteri portalı ("aracım hazır mı?")
- Stok / Parça envanter yönetimi
- Tedarikçi yönetimi
- Raporlama ve gelir analizi
- Fatura / fiş yazdırma
- Çalışan / teknisyen yönetimi
- Fotoğraf yükleme altyapısı (model var, depolama yok)

---

## 3. HEDEF KİTLE VE PAZAR ANALİZİ

### Bu Ürün Kime Hitap Ediyor?

Koda bakıldığında en net sinyal şudur: **küçük ve orta ölçekli oto servis atölyeleri**. 

Neden?
- İş emri numaraları günlük sıfırlanıyor → günde 5-50 iş emri olan hacim
- Müşteri + araç kaydı basit → büyük filolar için çok az
- Parça takibi basit → büyük bayiler için çok az
- WhatsApp birinci sınıf → esnaf ölçeği iletişim

Türkiye'de bu kitleyi şöyle tarif edebiliriz:
- 2-10 çalışanı olan oto servis atölyesi
- Günde 3-15 araç bakan mekanik veya servis ustası
- Şu an kağıt defter, Excel veya akılda tutuyor
- Muhasebe için ayrı bir program kullanıyor (Logo, Mikro gibi)
- WhatsApp grubunu hem müşteri hem çalışan yönetimi için kullanıyor

**Türkiye'de bu kitle ne kadar büyük?**

Türkiye'de yaklaşık **40.000-60.000 bağımsız oto servis atölyesi** bulunuyor (yetkili servisler hariç). Bunların büyük çoğunluğu dijital yönetim aracı kullanmıyor ya da çok eski/pahalı sistemler kullanıyor. Bu rakam, Türkiye için niche ama ölçeklenebilir bir SaaS fırsatıdır.

### Türkiye'deki Mevcut Rekabet

| Rakip | Güçlü Yanı | Zayıf Yanı |
|---|---|---|
| **Logo / Mikro Servis Modülü** | Muhasebe entegrasyonu | Pahalı, kurulum gerektirir, eski arayüz |
| **Netsis Servis** | Büyük işletmelere uygun | KOBİ için fazla karmaşık, yüksek maliyet |
| **OtoMate / ServiSoft** | Sektör spesifik | Mobil uygulama zayıf, modern değil |
| **Excel + WhatsApp** | Ücretsiz, alışkın | Veri kaybı, arama yok, raporlama yok |
| **Kağıt defter** | Sıfır maliyet | Yok artık |

**Boşluk:** Modern, mobil öncelikli, kurulum gerektirmeyen, WhatsApp entegrasyonlu, aylık abonelikle çalışan bir SaaS yok. Bu boşluk gerçek.

---

## 4. MVP YÖN ÖNERİLERİ

---

### YÖN A — "Servis Ustasının Dijital Defteri"
**Küçük Oto Servisler için İş Emri Yönetimi**

**Hedef Kitle:** 1-5 teknisyenli, yetkisiz, bağımsız oto servis atölyeleri

**Temel Özellikler:**
- Müşteri + araç kaydı (QR kod ile araç kaydı)
- Dijital iş emri açma, durum takibi
- İşçilik ve parça kalemi ekleme
- Ödeme kaydı ve bakiye takibi
- WhatsApp ile "aracınız hazır" bildirimi
- Günlük/aylık gelir özeti
- Mobil öncelikli (tablet ile kullanım)

**Para Kazanma Modeli:**
- Aylık abonelik: ₺299 / ay (yıllık ₺2.490)
- Deneme: 30 gün ücretsiz
- Uygulama içi SMS/WhatsApp bildirimi paketi

**Pitch:** _"Kağıt iş emri defterini bırak, müşterilerine WhatsApp'tan 'aracınız hazır' mesajı at — aylık 300 liraya."_

---

### YÖN B — "Araç Sahibinin Sağlık Kartı"
**Bireysel Araç Sahipleri için Bakım Takip Uygulaması**

**Hedef Kitle:** Aracına değer veren, bakım geçmişini takip etmek isteyen bireysel araç sahipleri (25-45 yaş, şehirli)

**Temel Özellikler:**
- Araç bakım takvimi (yağ, fren, lastik hatırlatıcıları)
- Servis geçmişi kaydı (hangi serviste ne yapıldı?)
- Kilometre takibi
- Masraf analizi ("Bu araç bu yıl bana kaç liraya mal oldu?")
- Araç satışında bakım geçmişini PDF olarak paylaşma
- Yakıt tüketimi takibi
- Sigorta ve muayene hatırlatıcıları

**Para Kazanma Modeli:**
- Freemium: 1 araç ücretsiz, ek araçlar ücretli
- Premium: ₺79/ay — sınırsız araç, PDF raporlar, öncelikli destek
- Otomotiv sigorta şirketleriyle affiliate anlaşması

**Pitch:** _"Arabanızın tüm bakım geçmişi, masrafları ve hatırlatıcıları cebinizde — satabileceğinizde belgelenmiş değer."_

---

### YÖN C — "Filo Yönetim SaaS'ı"
**Kurumsal Araç Filolarını Yöneten İşletmeler için**

**Hedef Kitle:** 10-200 araçlık filoya sahip lojistik, kurye, inşaat, kira araç şirketleri

**Temel Özellikler:**
- Çoklu araç yönetimi (marka, model, ruhsat, sigorta tarihleri)
- Araç bazlı bakım takvimi ve maliyet takibi
- Sürücü ataması ve sürücü geçmişi
- Servis randevu ve iş emri takibi
- Toplam sahip olma maliyeti (TCO) analizi
- Arıza ve kaza kayıtları
- Excel/PDF raporlama
- Yönetici dashboard'u

**Para Kazanma Modeli:**
- Araç başına fiyatlandırma: ₺49/araç/ay (minimum 10 araç)
- Kurulum + eğitim paketi: ₺2.000 (tek seferlik)
- Yıllık sözleşme indirimi: %20

**Pitch:** _"100 araçlık filonuzun kaç aracının bakımı gecikmiş, kaç tanesi bu ay masraf yaptı — tek ekranda."_

---

## 5. SWOT ANALİZİ

### YÖN A — Servis Ustasının Dijital Defteri

| | **GÜÇLÜ (Strengths)** | **ZAYIF (Weaknesses)** |
|---|---|---|
| **İç** | Domain modeli tam hazır; WhatsApp entegrasyonu kolay; düşük churn (bağımlılık yüksek) | Satış ve müşteri kazanımı saha satışı gerektirir; teknik destek yükü yüksek |

| | **FIRSAT (Opportunities)** | **TEHDİT (Threats)** |
|---|---|---|
| **Dış** | 40.000+ adreslenebilir servis; rakiplerin eski/pahalı olması; KOBİ dijitalleşme desteğleri | Yeni girişimlerin benzer ürün çıkarması; "ücretsiz rakip" olarak Excel alışkanlığı; fiyat hassasiyeti |

**Genel:** Pazar büyük ve hazır. Ama sahaya inmek gerekiyor — bu ürün dijital kanal ile satılmaz, sahadaki servislerle yüz yüze konuşularak satılır.

---

### YÖN B — Araç Sahibinin Sağlık Kartı

| | **GÜÇLÜ** | **ZAYIF** |
|---|---|---|
| **İç** | Bireysel kullanıcıya ulaşmak kolay (uygulama mağazaları); viral potansiyel yüksek | Çok düşük ödeme yapma istekliliği; mevcut domain modeli %80 yeniden yazılmalı |

| | **FIRSAT** | **TEHDİT** |
|---|---|---|
| **Dış** | Araç pazarı büyüyor; ikinci el araç satışında belge değeri; sigorta şirketleri partner olabilir | Google/Apple yerleşik uygulama yapabilir; düşük fiyat tavanı; kullanıcı aktifliği düşük |

**Genel:** Kitlesel ama düşük gelirli segment. B2C SaaS Türkiye'de hâlâ çok zor. Araç başına gelir çok düşük. Domain modeli de bu yöne uygun değil.

---

### YÖN C — Filo Yönetim SaaS'ı

| | **GÜÇLÜ** | **ZAYIF** |
|---|---|---|
| **İç** | Yüksek ARPU (araç başına gelir); kurumsal sözleşmeler güvenli; az müşteri, çok gelir | Domain modeli önemli ölçüde değişmeli (sürücü, rota, TCO); satış döngüsü uzun |

| | **FIRSAT** | **TEHDİT** |
|---|---|---|
| **Dış** | E-ticaret büyümesiyle kurye filoları artıyor; karbon ayak izi raporlaması zorunlu hale geliyor; ERP entegrasyon ihtiyacı | FleetIO, Samsara gibi uluslararası rakipler; müşteri IT departmanı onayı gerekir |

**Genel:** Kârlı ama uzun satış döngüsü. Mevcut domain modelini en az destekleyen yön bu. Sıfırdan başlamak gibi.

---

## 6. TAVSİYE

> **"Eğer ben olsaydım, Yön A'yı seçerdim — çünkü hem kod buna hazır, hem pazar bunu bekliyor, hem de Türkiye'de bu boşluğu dolduracak modern bir ürün henüz yok."**

### Detaylı Gerekçe

**Kod neredeyse hazır:** ServiceOrder aggregate'i, multi-tenant yapı, ödeme takibi, iş emri numarası — bunlar zaten var. Birkaç sprint'te çalışan bir MVP çıkarılabilir. Diğer yönler için domain'i baştan yazmak gerekir.

**Pazar kanıtlanmış bir acıyı çözüyor:** "İş emri nerede?" sorusu her servis ustasının sorduğu bir soru. Kağıt defterden dijitale geçiş motivasyonu güçlü ve anlık fayda görünür.

**WhatsApp kancası güçlü:** Türkiye'de bir servise araç bıraktıktan sonra "aracım ne oldu?" diye aramak standart. "Bırak, ben sana WhatsApp'tan yazarım" özelliği tek başına satış yapabilir.

**Fiyatlandırma makul:** Aylık 300 TL, servis ustası için günde 1 iş emrinin gelirine karşılık geliyor. ROI görünür.

**Büyüme yolu net:** Servis A → Yön A → Servis B → Zincirler → Yetkililer → Filo (Yön C). Pazarı küçükten büyüğe doğru tırmanmak mümkün.

### Sprint 1 için Yön A'nın MVP Kapsamı

Yön A ile Sprint 1 çıktısı şunları içermeli:

1. Servis kaydı (tenant onboarding)
2. Müşteri kayıt + arama
3. Araç kayıt + müşteriye bağlama
4. İş emri açma + durum takibi
5. İşçilik ve parça ekleme
6. Ödeme kaydı
7. Basit günlük özet: "Bugün X araç girdi, Y lira işlem yapıldı"

Daha sonra eklenecek özellikler (Sprintler 2-4):
- WhatsApp ile müşteri bildirimi
- Fotoğraf çekme ve iş emrine ekleme
- Haftalık gelir raporu
- Teknisyen ataması
- Mobil uygulama (MAUI)

---

## EK: Rakip Konumlandırma Haritası

```
                    MODERN / KOLAY KULLANIM
                           ▲
                           │
              MotorCare    │    (Hedef konum)
              Fırsatı  ●   │
                           │
KOBİ ─────────────────────┼──────────────── KURUMSAL
(Küçük servis)             │                  (Büyük filo)
                           │
     Excel+WA ●            │         ● Logo/Mikro
                           │
          OtoMate ●        │    ● Netsis Servis
                           │
                    ESKİ / KARMAŞIK
```

MotorCare'in fırsatı: **modern + kolay kullanım + KOBİ odağı** — şu anda bu kadranı dolduran ciddi bir oyuncu yok.

---

*Oluşturulma tarihi: 2026-04-18*  
*Bir sonraki adım: Yön A seçilirse → user story'ler ve UX wireframe'ler*
