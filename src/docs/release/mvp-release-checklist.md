# GarajPass MVP Release Checklist

Bu checklist staging ve production aday deploy'larında kullanılacak kısa kontrol listesidir. Secret/env dosyalarının içeriği terminale basılmamalıdır.

## 1. Deploy Commit

- Deploy edilecek commit kaydedildi:
  - `git log -1 --oneline`
- API version endpoint'i commit bilgisini aynı commit ile gösteriyor:
  - `curl -i http://127.0.0.1:5102/api/version`
- Staging son bilinen MVP adayı:
  - `f55c12b6f27d071b76907e1f2a3665413b6b175a`

## 2. Migration Kontrolü

- Repo'daki son migration doğrulandı:
  - `ls MotorCare.Infrastructure/Migrations`
- Migrator sadece uygulama migration'ı için çalıştırıldı:
  - `docker compose --env-file .env.staging -p motorcare-stack-src -f docker-compose.staging.yml run --rm migrator`
- `/api/version` içindeki `latestAppliedMigration` repo'daki son migration ile eşleşiyor.
- MVP QR/Public Report Phase 1 son migration:
  - `20260509201214_AddPublicRecordAccesses`

## 3. Container Kontrolü

- Postgres container stop/remove/recreate edilmedi.
- Sadece API ve App recreate edildiğinde beklenen durum:
  - `motorcare-staging-api` healthy, `127.0.0.1:5102->8080`
  - `motorcare-staging-app` healthy, `127.0.0.1:5101->8080`
  - `motorcare-staging-postgres` healthy, StartedAt değişmedi
- Kontrol komutu:
  - `docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}' | grep motorcare`

## 4. Health Check

- API health:
  - `curl -i http://127.0.0.1:5102/health`
- API version:
  - `curl -i http://127.0.0.1:5102/api/version`
- App route:
  - `curl -I http://127.0.0.1:5101/`

## 5. Backup

- Deploy öncesi staging veya production DB backup alındı.
- Backup dosyası tarihli, erişimi sınırlı bir dizinde saklandı.
- Backup restore edilebilirliği mümkünse ayrı restore-check DB üzerinde doğrulandı.
- Detaylı prosedür: `docs/ops/backup-restore.md`

## 6. Smoke Checklist

- Auth: register, email verification, login, forgot/reset password.
- Dashboard: boş state ve veri sonrası widget'lar.
- Customers: oluşturma, liste, detay, arama.
- Vehicles: oluşturma, model seçimi, müşteri ilişkisi, servis emri auto-fill.
- Service Catalog: hizmet/fiyat oluşturma, düzenleme, negatif fiyat validation.
- Appointments: haftalık plan, farklı gün/saat, liste ve detay.
- Service Order: oluşturma, operasyon/parça/sarf/tahsilat, detay, print/PDF.
- Inspection: oluşturma, checklist/body map, detay, print/PDF.
- QR/Public Report: service record ve inspection public preview auth olmadan açılıyor.
- Roles: Owner, Admin, Receptionist, Technician minimum yetki smoke'u.

## 7. Log ve Güvenlik Kontrolü

- API log:
  - `docker logs motorcare-staging-api --tail 500`
- App log:
  - `docker logs motorcare-staging-app --tail 300`
- Aranacak riskler:
  - unexpected `exception`, `error`, `500`, unexpected `401`
  - `ObjectDisposedException`, `JSDisconnectedException`
  - secret, token, password, code
  - public QR slug veya public full link

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

- Production domain ve TLS doğrulandı.
- `PublicReports__BaseUrl` production domain'e çekildi.
- SMTP production ayarları secret sızdırmadan doğrulandı.
- Backup saklama politikası ve restore tatbikatı tamamlandı.
- Rollback sorumlusu, deploy sorumlusu ve smoke sorumlusu netleştirildi.
