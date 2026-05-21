#!/usr/bin/env sh
set -eu

usage() {
  cat <<'USAGE'
Usage: backup-motorcare.sh <staging|production> [backup-dir] [postgres-container] [api-container] [attachments-path]

Creates a PostgreSQL dump and a service-order attachment archive with the same timestamp.

Environment variables:
  MOTORCARE_BACKUP_DIR      Default backup directory when backup-dir is omitted.
  Storage__AttachmentsPath  Container path for attachment storage.
USAGE
}

validate_container_path() {
  path="$1"
  case "$path" in
    /*) ;;
    *)
      echo "Container path must be absolute: $path" >&2
      exit 2
      ;;
  esac

  if printf '%s' "$path" | grep -Eq "[^A-Za-z0-9_./-]"; then
    echo "Container path contains unsupported characters: $path" >&2
    exit 2
  fi
}

if [ "$#" -lt 1 ] || [ "$#" -gt 5 ]; then
  usage >&2
  exit 2
fi

ENVIRONMENT="$1"
case "$ENVIRONMENT" in
  staging)
    DEFAULT_POSTGRES_CONTAINER="motorcare-staging-postgres"
    DEFAULT_API_CONTAINER="motorcare-staging-api"
    ;;
  production)
    DEFAULT_POSTGRES_CONTAINER="motorcare-production-postgres"
    DEFAULT_API_CONTAINER="motorcare-production-api"
    ;;
  *)
    echo "Unsupported environment: $ENVIRONMENT" >&2
    usage >&2
    exit 2
    ;;
esac

BACKUP_DIR="${2:-${MOTORCARE_BACKUP_DIR:-/opt/motorcare-backups}}"
POSTGRES_CONTAINER="${3:-$DEFAULT_POSTGRES_CONTAINER}"
API_CONTAINER="${4:-$DEFAULT_API_CONTAINER}"
ATTACHMENTS_PATH="${5:-${Storage__AttachmentsPath:-/var/lib/motorcare/attachments}}"

validate_container_path "$ATTACHMENTS_PATH"

TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"
BASE_NAME="motorcare-${ENVIRONMENT}-${TIMESTAMP}"
DB_ARCHIVE_NAME="${BASE_NAME}.dump"
ATTACHMENTS_ARCHIVE_NAME="${BASE_NAME}-attachments.tar.gz"
DB_ARCHIVE_PATH="${BACKUP_DIR}/${DB_ARCHIVE_NAME}"
ATTACHMENTS_ARCHIVE_PATH="${BACKUP_DIR}/${ATTACHMENTS_ARCHIVE_NAME}"
MANIFEST_PATH="${BACKUP_DIR}/${BASE_NAME}.manifest.txt"

umask 077
mkdir -p "$BACKUP_DIR"

echo "Creating ${ENVIRONMENT} PostgreSQL backup from ${POSTGRES_CONTAINER}"
docker exec "$POSTGRES_CONTAINER" sh -lc \
  'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom --no-owner --no-acl' \
  > "$DB_ARCHIVE_PATH"

if [ ! -s "$DB_ARCHIVE_PATH" ]; then
  echo "Database backup failed or archive is empty: $DB_ARCHIVE_PATH" >&2
  exit 1
fi

docker exec -i "$POSTGRES_CONTAINER" sh -lc 'pg_restore --list >/dev/null' < "$DB_ARCHIVE_PATH"

echo "Creating ${ENVIRONMENT} attachment backup from ${API_CONTAINER}:${ATTACHMENTS_PATH}"
docker exec -e MOTORCARE_ATTACHMENTS_PATH="$ATTACHMENTS_PATH" "$API_CONTAINER" sh -lc \
  'if [ -d "$MOTORCARE_ATTACHMENTS_PATH" ]; then cd "$MOTORCARE_ATTACHMENTS_PATH"; else mkdir -p /tmp/motorcare-empty-attachments && cd /tmp/motorcare-empty-attachments; fi; tar -czf - .' \
  > "$ATTACHMENTS_ARCHIVE_PATH"

if [ ! -s "$ATTACHMENTS_ARCHIVE_PATH" ]; then
  echo "Attachment backup failed or archive is empty: $ATTACHMENTS_ARCHIVE_PATH" >&2
  exit 1
fi

if command -v tar >/dev/null 2>&1; then
  tar -tzf "$ATTACHMENTS_ARCHIVE_PATH" >/dev/null
fi

if command -v sha256sum >/dev/null 2>&1; then
  (
    cd "$BACKUP_DIR"
    sha256sum "$DB_ARCHIVE_NAME" > "${DB_ARCHIVE_NAME}.sha256"
    sha256sum "$ATTACHMENTS_ARCHIVE_NAME" > "${ATTACHMENTS_ARCHIVE_NAME}.sha256"
  )
fi

cat > "$MANIFEST_PATH" <<EOF
environment=$ENVIRONMENT
created_utc=$TIMESTAMP
postgres_container=$POSTGRES_CONTAINER
api_container=$API_CONTAINER
attachments_path=$ATTACHMENTS_PATH
database_archive=$DB_ARCHIVE_NAME
attachments_archive=$ATTACHMENTS_ARCHIVE_NAME
EOF

DB_BYTES="$(wc -c < "$DB_ARCHIVE_PATH" | tr -d ' ')"
ATTACHMENT_BYTES="$(wc -c < "$ATTACHMENTS_ARCHIVE_PATH" | tr -d ' ')"
echo "Database backup created: $DB_ARCHIVE_PATH (${DB_BYTES} bytes)"
echo "Attachment backup created: $ATTACHMENTS_ARCHIVE_PATH (${ATTACHMENT_BYTES} bytes)"
echo "Manifest written: $MANIFEST_PATH"
