#!/usr/bin/env sh
set -eu

CONTAINER_NAME="${1:-motorcare-staging-postgres}"
BACKUP_DIR="${2:-./backups}"
TIMESTAMP="$(date -u +%Y%m%d_%H%M%S)"
REMOTE_FILE="/tmp/motorcare_staging_backup.dump"
LOCAL_FILE="${BACKUP_DIR}/motorcare_staging_backup_${TIMESTAMP}.dump"

mkdir -p "$BACKUP_DIR"

echo "Creating staging backup from container: ${CONTAINER_NAME}"
docker exec "$CONTAINER_NAME" sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -F c -f /tmp/motorcare_staging_backup.dump'

echo "Copying backup to: ${LOCAL_FILE}"
docker cp "${CONTAINER_NAME}:${REMOTE_FILE}" "$LOCAL_FILE"

if [ ! -s "$LOCAL_FILE" ]; then
  echo "Backup failed or file is empty: ${LOCAL_FILE}" >&2
  exit 1
fi

BYTES="$(wc -c < "$LOCAL_FILE" | tr -d ' ')"
echo "Backup created successfully: ${LOCAL_FILE} (${BYTES} bytes)"

