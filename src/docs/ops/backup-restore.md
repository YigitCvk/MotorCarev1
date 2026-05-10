# GarajPass Backup / Restore Planı

Bu doküman PostgreSQL backup ve restore tatbikatı için güvenli operasyon adımlarını tarif eder. Secret/env dosyaları terminale basılmamalı, Postgres container yanlışlıkla stop/remove/recreate edilmemelidir.

## Temel Kurallar

- Staging komutlarında proje adı korunur: `-p motorcare-stack-src`.
- Staging env dosyası kullanılır: `.env.staging`.
- `motorcare-staging-postgres` container'ı durdurulmaz, silinmez, recreate edilmez.
- Backup dosyaları repo içine yazılmaz.
- Restore tatbikatı mümkünse ayrı bir restore-check veritabanında yapılır.

## Backup Komutu

Sunucuda:

```bash
cd /opt/motorcare-stack-src/src
mkdir -p /opt/motorcare-backups
docker exec motorcare-staging-postgres sh -lc 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom --no-owner --no-acl' > /opt/motorcare-backups/motorcare-staging-$(date -u +%Y%m%dT%H%M%SZ).dump
```

Kontrol:

```bash
ls -lh /opt/motorcare-backups
```

Not: `pg_dump` komutu container içindeki mevcut `POSTGRES_USER` ve `POSTGRES_DB` değerlerini kullanır; `.env.staging` içeriği ekrana basılmaz.

## Restore Tatbikatı

Restore kontrolü canlı staging DB üzerine değil, geçici bir DB üzerine yapılmalıdır.

1. Backup dosyasını seç:

```bash
backup_file=/opt/motorcare-backups/motorcare-staging-YYYYMMDDTHHMMSSZ.dump
```

2. Geçici restore DB oluştur:

```bash
docker exec motorcare-staging-postgres sh -lc 'dropdb -U "$POSTGRES_USER" --if-exists motorcare_restore_check'
docker exec motorcare-staging-postgres sh -lc 'createdb -U "$POSTGRES_USER" motorcare_restore_check'
```

3. Backup'ı geçici DB'ye yükle:

```bash
cat "$backup_file" | docker exec -i motorcare-staging-postgres sh -lc 'pg_restore -U "$POSTGRES_USER" -d motorcare_restore_check --no-owner --no-acl'
```

4. Temel doğrulama sorguları çalıştır:

```bash
docker exec motorcare-staging-postgres sh -lc 'psql -U "$POSTGRES_USER" -d motorcare_restore_check -c "select count(*) from \"Tenants\";"'
docker exec motorcare-staging-postgres sh -lc 'psql -U "$POSTGRES_USER" -d motorcare_restore_check -c "select to_regclass('\''public.\"PublicRecordAccesses\"'\'');"'
```

5. Tatbikat DB'sini temizle:

```bash
docker exec motorcare-staging-postgres sh -lc 'dropdb -U "$POSTGRES_USER" --if-exists motorcare_restore_check'
```

## Canlı Restore Gerektiğinde

- Önce olay kaydı alın: deploy commit, `/api/version`, migration, backup dosyası, mevcut container durumu.
- API/App trafiği durdurma veya bakım modu ihtiyacı ayrıca değerlendirilir.
- Postgres container silinmez veya recreate edilmez.
- Canlı DB üzerine restore ancak en güncel backup doğrulandıktan ve veri kaybı etkisi onaylandıktan sonra yapılır.
- Canlı restore sonrasında migrator çalıştırılmadan önce migration durumu kontrol edilir.

## Dikkat Edilecek Secret/Env Konuları

- `.env.staging` ve production env dosyaları `cat`, `type`, `echo` ile log'a basılmaz.
- SMTP, JWT, database password, token veya kod değerleri rapora yazılmaz.
- Gerekiyorsa yalnızca key varlığı maskeli kontrol edilir.
- Backup dosyaları gizli veri içerir; erişim yetkisi ve saklama süresi sınırlandırılmalıdır.

## Hızlı Sağlık Kontrolü

```bash
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}' | grep motorcare
curl -i http://127.0.0.1:5102/health
curl -i http://127.0.0.1:5102/api/version
```
