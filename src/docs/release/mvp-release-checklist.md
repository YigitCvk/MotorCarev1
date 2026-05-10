# GarajPass MVP Release Checklist

Bu checklist staging ve production aday deploy'larında kullanılacak kısa kontrol listesidir. Secret/env dosyalarının içeriği terminale basılmamalıdır.

## 1. Deploy Commit

- [x] Deploy edilecek commit kaydedildi:
  - `git log -1 --oneline`
- [x] API version endpoint'i commit bilgisini aynı commit ile gösteriyor:
  - `curl -i http://127.0.0.1:5102/api/version`
- [x] Staging son bilinen MVP adayı:
  - `ed558ca` (main branch, 2026-05-10)

## 2. Migration Kontrolü

- [x] Repo'daki son migration doğrulandı:
  - `ls MotorCare.Infrastructure/Migrations`
- [x] Migrator sadece uygulama migration'ı için çalıştırıldı:
  - `docker compose --env-file .env.staging -p motorcare-stack-src -f docker-compose.staging.yml run --rm migrator`
- [x] `/api/version` içindeki `latestAppliedMigration` repo'daki son migration ile eşleşiyor.
- [x] MVP QR/Public Report Phase 1 son migration:
  - `20260509201214_AddPublicRecordAccesses`

## 3. Container Kontrolü

- [x] Postgres container stop/remove/recreate edilmedi.
- [x] Sadece API ve App recreate edildiğinde beklenen durum:
  - `motorcare-staging-api` healthy, `127.0.0.1:5102->8080`
  - `motorcare-staging-app` healthy, `127.0.0.1:5101->8080`
  - `motorcare-staging-postgres` healthy, StartedAt değişmedi
- Kontrol komutu:
  - `docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}' | grep motorcare`

## 4. Health Check

- [x] API health:
  - `curl -i http://127.0.0.1:5102/health`
- [x] API version:
  - `curl -i http://127.0.0.1:5102/api/version`
- [x] App route:
  - `curl -I http://127.0.0.1:5101/`

## 5. Backup

- [x] Deploy öncesi staging veya production DB backup alındı.
  - `/opt/motorcare-backups/motorcare-staging-20260510T140327Z.dump` (123K)
- [x] Backup dosyası tarihli, erişimi sınırlı bir dizinde saklandı.
- [ ] Backup restore edilebilirliği mümkünse ayrı restore-check DB üzerinde doğrulandı. *(tatbikat bekliyor)*
- Detaylı prosedür: `docs/ops/backup-restore.md`

## 6. Smoke Checklist

- [x] Auth: register, email verification, login, forgot/reset password.
- [ ] Dashboard: boş state ve veri sonrası widget'lar. *(manuel doğrulama bekliyor)*
- [x] Customers: oluşturma, liste, detay, arama. *(API üzerinden doğrulandı)*
- [x] Vehicles: oluşturma, model seçimi, müşteri ilişkisi, servis emri auto-fill.
- [x] Service Catalog: hizmet/fiyat oluşturma, düzenleme, negatif fiyat validation.
- [x] Appointments: haftalık plan, farklı gün/saat, liste ve detay.
- [x] Service Order: oluşturma, operasyon/parça/sarf/tahsilat, detay, print/PDF.
- [x] Inspection: oluşturma, checklist/body map, detay, print/PDF.
- [x] QR/Public Report: service record ve inspection public preview auth olmadan açılıyor.
- [ ] Roles: Owner, Admin, Receptionist, Technician minimum yetki smoke'u. *(ek kullanıcı gerekiyor)*

## 7. Log ve Güvenlik Kontrolü

- [x] API log:
  - `docker logs motorcare-staging-api --tail 500`
- [x] App log:
  - `docker logs motorcare-staging-app --tail 300`
- [x] Aranacak riskler:
  - unexpected `exception`, `error`, `500`, unexpected `401`
  - `ObjectDisposedException`, `JSDisconnectedException`
  - secret, token, password, code
  - public QR slug veya public full link → `RequestPathRedactionEnricher` aktif, slug'lar logda `{slug}` olarak görünüyor

## 8. Rollback Planı

- Önce durum kaydı alın:
  - deploy commit
  - `/api/version`
  - `docker ps`
  - son migration
  - son backup dosyası
- Kod rollback gerekiyorsa önce eski imaj veya eski commit ile sadece API/App geri alın.
- Postgres container stop/remove/recreate edilmez.
- Migration rollback yalnızca migration'ın veri etkisi analiz edildikten ve backup restore planı hazırlandıktan sonra yapılır.
- Kritik veri bozulması varsa backup restore prosedürü `docs/ops/backup-restore.md` üzerinden uygulanır.

## 9. Known Risks

- Public report Phase 1'de payment/paywall yok; yalnızca sınırlı preview verisi gösterilir.
- Public QR enable/disable UI, access count UI ve QR etiket çıktısı Faz 2'ye bırakıldı.
- Production domain kesinleşene kadar `PublicReports__BaseUrl` IP veya staging domain üzerinden çalışabilir.
- EF owned collection query'lerinde split query kullanımı korunmalı; yeni aggregate listelerinde aynı risk tekrar kontrol edilmeli.

## 10. Production Öncesi

- [ ] Production domain ve TLS doğrulandı.
- [ ] `PublicReports__BaseUrl` production domain'e çekildi.
- [ ] SMTP production sender domain, SPF/DKIM/DMARC kayıtları doğrulandı.
- [ ] Production env secrets güvenli ortamda hazırlandı (JWT, DB, SMTP).
- [ ] KVKK / gizlilik politikası / kullanım koşulları metinleri hazırlandı.
- [ ] Backup saklama politikası ve restore tatbikatı tamamlandı.
- [ ] Rollback sorumlusu, deploy sorumlusu ve smoke sorumlusu netleştirildi.
