# GarajPass MVP Status

## 1. Tamamlanan Moduller

- Auth: login, register, refresh, logout, me, email verification, forgot/reset password kod akisi.
- Tenant onboarding: isletme kodu, servis adi ve owner kullanici ile tenant olusturma.
- Dashboard: gunluk operasyon ozetleri ve authenticated landing.
- Customers: musteri listeleme, olusturma, detail ve iliskili kayit gorunumleri.
- Vehicles: arac olusturma, listeleme ve musteri ile iliskilendirme.
- Appointments: haftalik plan, liste gorunumu, detail, status ve service order'a donusturme akislari.
- Service Orders: olusturma, detail, print ve status akislari.
- Labor: servis emri detailinde iscilik ekleme/kaldirma.
- Parts: servis emri detailinde parca ekleme/kaldirma.
- Payments: tahsilat ekleme ve finansal toplamlar.
- Consumables: servis emri olusturma/detail tarafinda sarf/kullanilan urun kaydi.
- Service Catalog: fiyat ve currency ile create/update/list/detail.
- Motorcycle catalog suggestions: motosiklet marka/model suggestion.
- Inspection PDF/body map: print sayfasinda motosiklet gorseli, isaretli body-map bolgeleri, legend ve notlar.
- Email flow: GarajPass branding ile verification, reset code ve 2FA email akislari.
- Basic mobile responsiveness: landing, auth ve ana dashboard ekranlari icin temel responsive davranis.

## 2. Kismen Tamamlanan Moduller

- Service order create fiyat entegrasyonu: catalog price smoke edildi; daha kapsamli edge-case ve coklu hizmet senaryolari polish gerektirir.
- Inspection workflow: print/body map hazir; paket bazli tum checklist varyasyonlari icin daha genis kabul testi gerekir.
- Mobile UI: temel kontroller gecti; tum CRUD formlari icin cihaz matrisi tamamlanmadi.
- Log/circuit hardening: kullanici akisi stabil; Blazor Server hizli browser close/navigate durumlarinda framework seviyesinde log gurultusu izlenmeye devam edilmeli.
- Role/permission matrisi: temel policy'ler var; tum rollerle tam regresyon tamamlanmadi.

## 3. MVP Icin Zorunlu Ama Eksik Kalan Isler

- Production email verification gercek inbox testi.
- Role/permission final kontrolu.
- Backup/restore plani ve restore tatbikati.
- Production deployment checklist onayi.
- Monitoring/logging dashboard.
- Error alerting.
- Legal pages durumu: KVKK, kullanici sozlesmesi, gizlilik ve cerez metinleri netlestirilmeli.
- Subscription/payment: MVP disi kabul ediliyorsa dokumante edilmeli; aksi halde kapsam belirlenmeli.

## 4. MVP Disi Sonraki Faz Isleri

- GarajPass ucretli planlar ve abonelik yonetimi.
- Dogrulanabilir arac gecmisi raporu.
- Musteri portali.
- SMS/WhatsApp bildirimleri.
- Gelismis raporlama ve operasyon analitigi.
- Multi-branch servis yapisi.
- Public landing SEO gelistirmeleri.
- B2C arac gecmisi sorgulama deneyimi.

## 5. Riskler

- Blazor Server circuit noise: hizli browser close/navigate durumlari framework loglarinda gorunebilir; kullanici akisi etkilenmemeli.
- Email dogrulama gercek mailbox testi bu smoke ortaminda yapilmadiysa production oncesi zorunludur.
- Staging ve production env ayrimi: secret, SMTP, AppBaseUrl ve logging seviyeleri final kontrol ister.
- Backup stratejisi: otomatik backup ve restore testi tamamlanmadan production riski vardir.
- Package warnings: build warningleri temizlenmeli veya risk kabul dokumani olusturulmalidir.

## 6. Onerilen Siradaki 5 Adim

1. Production env checklist'ini doldur ve secret/AppBaseUrl/SMTP degerlerini ikinci kisiyle dogrula.
2. Gercek mailbox ile register, verify email, forgot password ve reset code akisini test et.
3. Backup/restore tatbikati yap ve geri donus suresini kaydet.
4. Owner/Admin/Receptionist/Technician rolleriyle permission regresyonu kos.
5. Production deploy sonrasi ayni smoke akisini calistirip `/api/version`, log guvenligi ve kritik CRUD akislari icin imzali onay al.
