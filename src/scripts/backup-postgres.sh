#!/usr/bin/env sh
set -eu

usage() {
  cat <<'USAGE'
Usage: backup-postgres.sh <staging|production> [backup-dir] [container-name]

Environment variables:
  MOTORCARE_BACKUP_DIR  Default backup directory when backup-dir is omitted.
USAGE
}

if [ "$#" -lt 1 ] || [ "$#" -gt 3 ]; then
  usage >&2
  exit 2
fi

ENVIRONMENT="$1"
case "$ENVIRONMENT" in
  staging)
    DEFAULT_CONTAINER_NAME="motorcare-staging-postgres"
    ;;
  production)
    DEFAULT_CONTAINER_NAME="motorcare-production-postgres"
    ;;
  *)
    echo "Unsupported environment: $ENVIRONMENT" >&2
    usage >&2
    exit 2
    ;;
esac

BACKUP_DIR="${2:-${MOTORCARE_BACKUP_DIR:-/opt/motorcare-backups}}"
CONTAINER_NAME="${3:-$DEFAULT_CONTAINER_NAME}"
TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"
ARCHIVE_NAME="motorcare-${ENVIRONMENT}-${TIMESTAMP}.dump"
ARCHIVE_PATH="${BACKUP_DIR}/${ARCHIVE_NAME}"
CHECKSUM_PATH="${ARCHIVE_PATH}.sha256"

umask 077
mkdir -p "$BACKUP_DIR"

echo "Creating ${ENVIRONMENT} PostgreSQL backup from ${CONTAINER_NAME}"
docker exec "$CONTAINER_NAME" sh -lc \
  'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom --no-owner --no-acl' \
  > "$ARCHIVE_PATH"

if [ ! -s "$ARCHIVE_PATH" ]; then
  echo "Backup failed or archive is empty: $ARCHIVE_PATH" >&2
  exit 1
fi

docker exec -i "$CONTAINER_NAME" sh -lc 'pg_restore --list >/dev/null' < "$ARCHIVE_PATH"

if command -v sha256sum >/dev/null 2>&1; then
  (
    cd "$BACKUP_DIR"
    sha256sum "$ARCHIVE_NAME" > "${ARCHIVE_NAME}.sha256"
  )
  echo "Checksum written: $CHECKSUM_PATH"
else
  echo "sha256sum not found; checksum file was not created" >&2
fi

BYTES="$(wc -c < "$ARCHIVE_PATH" | tr -d ' ')"
echo "Backup created: $ARCHIVE_PATH (${BYTES} bytes)"
