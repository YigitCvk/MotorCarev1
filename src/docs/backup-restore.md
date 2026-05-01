# GarajPass Backup / Restore Runbook

## 1. Genel Yaklasim

- Backup alinmadan production deploy yapilmaz.
- Restore islemi dogrudan production DB uzerinde denenmez.
- Restore once sandbox/test DB uzerinde denenir.
- Backup dosyalari guvenli, erisimi sinirli bir yerde saklanir.
- Secret bilgileri dokumana yazilmaz.
- Backup dosyalari gercek musteri verisi icerebilir; production verisi gibi korunur.

## 2. Staging Backup Komutu

Staging PostgreSQL container:

```text
motorcare-staging-postgres
```

Docker exec icinde backup alma:

```bash
docker exec motorcare-staging-postgres pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -F c -f /tmp/motorcare_staging_backup.dump
```

Eger `docker exec` icinde env variable expand olmuyorsa shell uzerinden calistir:

```bash
docker exec motorcare-staging-postgres sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -F c -f /tmp/motorcare_staging_backup.dump'
```

Backup dosyasini local/sunucu backup klasorune al:

```bash
mkdir -p ./backups
docker cp motorcare-staging-postgres:/tmp/motorcare_staging_backup.dump ./backups/motorcare_staging_backup_YYYYMMDD_HHMM.dump
```

## 3. Backup Dogrulama

- Dosya var mi?
- Dosya boyutu 0'dan buyuk mu?
- Dosya timestamp'i dogru mu?
- Backup dosyasi guvenli dizine alindi mi?
- Backup dosyasi erisim yetkileri sinirli mi?
- Backup dosyasi staging/production ayrimina gore isimlendirildi mi?

Ornek kontrol:

```bash
ls -lh ./backups/motorcare_staging_backup_YYYYMMDD_HHMM.dump
test -s ./backups/motorcare_staging_backup_YYYYMMDD_HHMM.dump
```

## 4. Restore Sandbox Komutu

Production veya staging DB uzerine direkt restore yapma. Once sandbox DB/container kullan.

Sandbox icinde ornek:

```bash
createdb -U "$POSTGRES_USER" motorcare_restore_sandbox
pg_restore -U "$POSTGRES_USER" -d motorcare_restore_sandbox --clean --if-exists /tmp/motorcare_staging_backup.dump
```

Docker container icinde ornek:

```bash
docker cp ./backups/motorcare_staging_backup_YYYYMMDD_HHMM.dump sandbox-postgres:/tmp/motorcare_restore.dump
docker exec sandbox-postgres sh -c 'createdb -U "$POSTGRES_USER" motorcare_restore_sandbox || true'
docker exec sandbox-postgres sh -c 'pg_restore -U "$POSTGRES_USER" -d motorcare_restore_sandbox --clean --if-exists /tmp/motorcare_restore.dump'
```

Restore komutlari destructive olabilir. Production/staging DB adini hedef olarak yazmadan once ikinci kisi kontrolu yap.

## 5. Restore Sonrasi Kontrol

- `/api/version`
- Login
- Dashboard
- Service order list
- Appointment list
- Inspection print
- Smoke checklist

Minimum smoke:

```text
1. Test tenant ile login ol.
2. Dashboard aciliyor mu kontrol et.
3. Servis emirleri listesi aciliyor mu kontrol et.
4. Randevular haftalik plan aciliyor mu kontrol et.
5. Ekspertiz print/PDF route'u aciliyor mu kontrol et.
6. App/API loglarinda unexpected 500 veya secret sızıntisi var mi kontrol et.
```

## 6. Rollback Notu

Rollback icin deploy oncesi kaydedilecek bilgiler:

- Son saglikli commit.
- Docker image tag/commit.
- DB backup timestamp.
- `/api/version` ciktisi.
- Portainer recreate adimlari.
- Postgres container'a dokunmama notu.

Rollback prensibi:

- Once app/api onceki saglikli image veya commit'e dondurulur.
- DB restore sadece zorunluysa ve sandbox restore dogrulandiysa yapilir.
- Postgres container recreate edilmez; veri volume'u korunur.

