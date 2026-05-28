# GarajPass Backup / Restore

The canonical operations runbook lives in `docs/ops/backup-restore.md`.

Quick entry points:

```bash
MOTORCARE_BACKUP_DIR=/opt/motorcare-backups sh scripts/backup-motorcare.sh staging
MOTORCARE_BACKUP_DIR=/opt/motorcare-backups sh scripts/backup-motorcare.sh production
sh scripts/restore-motorcare.sh staging \
  /opt/motorcare-backups/motorcare-staging-YYYYMMDDTHHMMSSZ.dump \
  /opt/motorcare-backups/motorcare-staging-YYYYMMDDTHHMMSSZ-attachments.tar.gz
```

Use the restore script for a restore-check database and restore-check attachment directory first. It intentionally refuses to target the live application database or live attachment root. Treat every dump and attachment archive as production-like customer data and keep real env values out of terminal output, docs, and tickets.
