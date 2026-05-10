# GarajPass MVP Demo Script

Staging: http://46.225.166.254:5101  
Tarih: 2026-05-10  
Hazırlayan: demo-garajpass tenant

## Demo Hesabı

| Alan | Değer |
|------|-------|
| Tenant | demo-garajpass |
| E-posta | demo@garajpass.com |
| Şifre | Demo@123456 |

## Demo Verisi

### Müşteriler

| Ad | Telefon | Not |
|----|---------|-----|
| Mehmet Kaya | +90 532 123 45 67 | 2 araç, 1 tamamlanan servis |
| Fatma Demir | +90 541 123 45 67 | 1 araç, 1 tamamlanan servis |
| Ali Çelik | +90 555 123 99 99 | 1 araç, 1 ekspertiz |

### Araçlar

| Plaka | Marka/Model | Yıl | KM | Müşteri |
|-------|-------------|-----|-----|---------|
| 34MK001 | Honda CB500F | 2021 | 18.700 | Mehmet Kaya |
| 34MK002 | Yamaha MT-07 | 2022 | 9.200 | Mehmet Kaya |
| 06FD100 | Kawasaki Z650 | 2020 | 31.200 | Fatma Demir |
| 35AC050 | Suzuki GSX-S750 | 2019 | 47.000 | Ali Çelik |

### Hizmet Katalogu

| Hizmet | Kategori | Süre | Fiyat |
|--------|----------|------|-------|
| Yağ Değişimi | Motorsiklet Bakımı | 30 dk | 850 TL |
| Zincir Ayarı | Motorsiklet Bakımı | 20 dk | 350 TL |
| Lastik Değişimi | Lastik Servisi | 45 dk | 1.200 TL |

### Randevular

| Tarih | Müşteri | Araç | Tip |
|-------|---------|------|-----|
| 12 Mayıs 2026 09:00 | Mehmet Kaya | Honda CB500F | Bakım |
| 13 Mayıs 2026 11:00 | Fatma Demir | Kawasaki Z650 | Ekspertiz |
| 14 Mayıs 2026 14:00 | Ali Çelik | Suzuki GSX-S750 | Onarım |

### Servis Emirleri

| No | Araç | Durum | İşlemler | Tahsilat |
|----|------|-------|----------|---------|
| SRV-20260510-0001 | 34MK001 Honda CB500F | Tamamlandı | Yağ Değişimi (850₺) + Zincir Germe (350₺) | 1.200 TL Kredi Kartı |
| SRV-20260510-0002 | 06FD100 Kawasaki Z650 | Tamamlandı | Lastik Montaj (1.200₺) + Fren Kontrolü (200₺) | — |

### Ekspertiz

| No | Araç | Paket | Durum | Kritik Bulgu |
|----|------|-------|-------|-------------|
| EXP-20260510-001 | 35AC050 Suzuki GSX-S750 | Full Ekspertiz | Tamamlandı | 0 |

### Public QR Sluglar

| Tür | Slug | URL |
|-----|------|-----|
| Servis Kaydı | JwRMqUYzPRXoLHFtl50Apl9UX_2MFwgk | http://46.225.166.254:5101/public/service-record/JwRMqUYzPRXoLHFtl50Apl9UX_2MFwgk |
| Ekspertiz | V2iiab25xMQqQGl0idXoUdaMwa642e5Q | http://46.225.166.254:5101/public/inspection-report/V2iiab25xMQqQGl0idXoUdaMwa642e5Q |

---

## Demo Akışı

### 1. Landing Page

- http://46.225.166.254:5101 adresini aç.
- Hero bölümündeki "Hemen Başla" butonuna tıkla.
- Trust strip, feature grid, testimonials, footer'ı göster.

### 2. Giriş

- "Giriş Yap" butonuna tıkla.
- Tenant: `demo-garajpass`
- E-posta: `demo@garajpass.com`
- Şifre: `Demo@123456`
- Giriş yap, dashboard'a yönlendir.

### 3. Dashboard

- Açık/Tamamlanan servis emirleri widget'larını göster.
- Bugünün randevuları ve son aktiviteler.

### 4. Müşteri Yönetimi

- Sol menü → Müşteriler.
- Mehmet Kaya'yı aç → 2 aracı, geçmiş servis emirleri.
- "Yeni Müşteri" butonuyla yeni müşteri oluşturma formunu göster, iptal et.

### 5. Araç Yönetimi

- Sol menü → Araçlar veya Mehmet Kaya → 34MK001.
- Honda CB500F detayını aç.
- Servis geçmişi, araç bilgileri tab'larını göster.

### 6. Randevu Takvimi

- Sol menü → Randevular.
- Haftalık görünümde 3 randevuyu göster.
- 12 Mayıs randevusuna tıkla, detayını göster.

### 7. Servis Emri — Tamamlanan + Tahsilat

- Sol menü → Servis Emirleri.
- `SRV-20260510-0001` (Honda CB500F) detayını aç.
- İşlemler: Yağ Değişimi + Zincir Germe.
- Sarf malzemesi: Motul 5100 10W-40.
- Tahsilat: 1.200 TL Kredi Kartı.
- Print/PDF butonunu göster.

### 8. Servis Emri — QR Public Report

- Aynı servis emrinde QR/Paylaş butonuna tıkla.
- Slug: `JwRMqUYzPRXoLHFtl50Apl9UX_2MFwgk`
- Tarayıcıda gizli sekme (auth yok) aç:  
  http://46.225.166.254:5101/public/service-record/JwRMqUYzPRXoLHFtl50Apl9UX_2MFwgk
- Müşterinin göreceği public önizlemeyi göster.

### 9. Ekspertiz

- Sol menü → Ekspertiz.
- `EXP-20260510-001` (Suzuki GSX-S750) detayını aç.
- Full Ekspertiz paketi, tamamlandı durumu, kritik bulgu yok.
- Print/PDF butonunu göster.

### 10. Ekspertiz QR Public Report

- Gizli sekme aç:  
  http://46.225.166.254:5101/public/inspection-report/V2iiab25xMQqQGl0idXoUdaMwa642e5Q
- Public ekspertiz özetini göster.
- "Bu kayıt GarajPass üzerinden doğrulanmıştır." metni.

### 11. Şifremi Unuttum

- Çıkış yap.
- Giriş sayfasında "Şifremi Unuttum" tıkla.
- E-posta alanına `demo@garajpass.com` gir, gönder.
- Akışı anlat (mail gider, link ile reset yapılır).

---

## Beklenen Sorular ve Cevaplar

**Production ne zaman?**  
Domain ve TLS alınır alınmaz. `PublicReports__BaseUrl` production domain'e çekilir.

**Fatura/ödeme modülü var mı?**  
Mevcut tahsilat kaydı MVP kapsamında. E-fatura entegrasyonu Faz 2.

**QR kodu disable etme?**  
Faz 2. Şu anda tüm servis kayıtları varsayılan aktif.

**Çoklu kullanıcı/rol?**  
Owner, Admin, Receptionist, Technician rolleri hazır. Kullanıcı davet akışı mevcut.

**Mobil uygulama?**  
Web uygulaması mobil-responsive. Native MAUI uygulaması roadmap'te.

---

## Backup Bilgisi

Backup dosyası: `/opt/motorcare-backups/motorcare-staging-20260510T140327Z.dump` (123K)  
Prosedür: `docs/ops/backup-restore.md`
