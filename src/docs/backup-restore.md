# GarajPass Backup / Restore

The canonical operations runbook lives in `docs/ops/backup-restore.md`.

Quick entry points:

```bash
MOTORCARE_BACKUP_DIR=/opt/motorcare-backups sh scripts/backup-postgres.sh staging
MOTORCARE_BACKUP_DIR=/opt/motorcare-backups sh scripts/backup-postgres.sh production
sh scripts/restore-postgres.sh staging /opt/motorcare-backups/motorcare-staging-YYYYMMDDTHHMMSSZ.dump
```

Use the restore script for a restore-check database first. It intentionally refuses to target the live application database. Treat every dump as production-like customer data and keep real env values out of terminal output, docs, and tickets.
