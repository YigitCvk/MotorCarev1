# GarajPass Backup / Restore Runbook

This runbook covers PostgreSQL dumps and restore drills for the Compose-based staging and production stacks.

## Safety Rules

- Take a backup before migrations or production deploys.
- Store dumps outside the repo checkout, with restricted access and retention.
- Treat dumps as production-like customer data even when they came from staging.
- Never print populated env files, passwords, JWT keys, SMTP credentials, or dump contents into logs.
- Do not stop, remove, or recreate a Postgres container as part of routine app deploys.
- Prove restore capability in a separate database before considering a live restore.

## Scripts

Linux:

```bash
MOTORCARE_BACKUP_DIR=/opt/motorcare-backups sh scripts/backup-postgres.sh staging
MOTORCARE_BACKUP_DIR=/opt/motorcare-backups sh scripts/backup-postgres.sh production
sh scripts/restore-postgres.sh staging /opt/motorcare-backups/motorcare-staging-YYYYMMDDTHHMMSSZ.dump
```

PowerShell:

```powershell
.\scripts\backup-postgres.ps1 -Environment staging -BackupDir D:\motorcare-backups
.\scripts\backup-postgres.ps1 -Environment production -BackupDir D:\motorcare-backups
.\scripts\restore-postgres.ps1 -Environment staging -BackupFile D:\motorcare-backups\motorcare-staging-YYYYMMDDTHHMMSSZ.dump
```

The existing `backup-staging.sh` and `backup-staging.ps1` commands remain as wrappers for older procedures.

## What The Scripts Do

`backup-postgres`:

1. Uses the running container's `POSTGRES_USER` and `POSTGRES_DB` values without printing them.
2. Creates a custom-format `pg_dump` archive with no owner or ACL metadata.
3. Verifies that the archive is non-empty and readable by `pg_restore`.
4. Writes a SHA-256 checksum beside the archive when the host tool is available.

`restore-postgres`:

1. Validates the custom-format archive.
2. Recreates `motorcare_restore_check` unless a different check-database name is supplied.
3. Restores the archive into that separate database.
4. Refuses to target the live `POSTGRES_DB` database.
5. Leaves the check database in place for manual validation.

## Restore Drill Validation

After the script finishes, run focused checks against `motorcare_restore_check`:

```bash
docker exec motorcare-staging-postgres sh -lc 'psql -U "$POSTGRES_USER" -d motorcare_restore_check -c "select count(*) from \"Tenants\";"'
docker exec motorcare-staging-postgres sh -lc 'psql -U "$POSTGRES_USER" -d motorcare_restore_check -c "select count(*) from \"__EFMigrationsHistory\";"'
docker exec motorcare-staging-postgres sh -lc 'dropdb -U "$POSTGRES_USER" --if-exists --force motorcare_restore_check'
```

For production drills, replace only the container name. Keep the target database separate from the live DB.

## Live Restore Gate

The automated restore scripts intentionally do not overwrite the live database. A live restore needs a separate incident record and an explicit decision covering:

- deploy commit, image tags, `/api/version`, and latest migration;
- selected archive name plus checksum verification;
- expected data-loss window and business approval;
- traffic stop or maintenance mode plan;
- a successful restore drill from the same archive;
- post-restore smoke owner and rollback owner.

If those conditions are not satisfied, roll back app images first and leave the database untouched.

## Post-Restore Smoke

Minimum checks after a restore drill or approved live restore:

1. `/health` and `/api/version`
2. Login with a test tenant
3. Dashboard load
4. Service order list
5. Appointment list
6. Inspection print route
7. Logs checked for unexpected `500`, `password`, `token`, reset code, or SMTP secret exposure

## Related Secret Handling

See `docs/ops/secret-management.md` for env-file storage, placeholder rules, build-context exclusions, rotation notes, and secret inventory.
