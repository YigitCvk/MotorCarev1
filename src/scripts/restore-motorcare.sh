#!/usr/bin/env sh
set -eu

usage() {
  cat <<'USAGE'
Usage: restore-motorcare.sh <staging|production> <db-backup-file> <attachments-archive> [target-database] [postgres-container] [api-container] [attachments-restore-path]

The default target database is motorcare_restore_check.
Attachments are extracted to <attachments-path>/restore-check/<target-database> by default.
The script refuses to extract directly into the live attachment root.
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

if [ "$#" -lt 3 ] || [ "$#" -gt 7 ]; then
  usage >&2
  exit 2
fi

ENVIRONMENT="$1"
DB_BACKUP_FILE="$2"
ATTACHMENTS_ARCHIVE="$3"
TARGET_DATABASE="${4:-motorcare_restore_check}"

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

POSTGRES_CONTAINER="${5:-$DEFAULT_POSTGRES_CONTAINER}"
API_CONTAINER="${6:-$DEFAULT_API_CONTAINER}"
LIVE_ATTACHMENTS_PATH="${Storage__AttachmentsPath:-/var/lib/motorcare/attachments}"
ATTACHMENTS_RESTORE_PATH="${7:-${LIVE_ATTACHMENTS_PATH}/restore-check/${TARGET_DATABASE}}"
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"

validate_container_path "$LIVE_ATTACHMENTS_PATH"
validate_container_path "$ATTACHMENTS_RESTORE_PATH"

if [ "$ATTACHMENTS_RESTORE_PATH" = "$LIVE_ATTACHMENTS_PATH" ] || [ "$ATTACHMENTS_RESTORE_PATH" = "/" ]; then
  echo "Refusing to restore attachments into live root: $ATTACHMENTS_RESTORE_PATH" >&2
  exit 1
fi

if [ ! -s "$DB_BACKUP_FILE" ]; then
  echo "Database backup file is missing or empty: $DB_BACKUP_FILE" >&2
  exit 1
fi

if [ ! -s "$ATTACHMENTS_ARCHIVE" ]; then
  echo "Attachment archive is missing or empty: $ATTACHMENTS_ARCHIVE" >&2
  exit 1
fi

echo "Validating attachment archive: $ATTACHMENTS_ARCHIVE"
tar -tzf "$ATTACHMENTS_ARCHIVE" >/dev/null
if tar -tzf "$ATTACHMENTS_ARCHIVE" | grep -Eq '(^/|(^|/)\.\.(/|$))'; then
  echo "Attachment archive contains unsafe paths" >&2
  exit 1
fi

sh "$SCRIPT_DIR/restore-postgres.sh" "$ENVIRONMENT" "$DB_BACKUP_FILE" "$TARGET_DATABASE" "$POSTGRES_CONTAINER"

echo "Extracting attachments into restore-check path: ${API_CONTAINER}:${ATTACHMENTS_RESTORE_PATH}"
docker exec -i -e MOTORCARE_ATTACHMENTS_RESTORE_PATH="$ATTACHMENTS_RESTORE_PATH" "$API_CONTAINER" sh -lc \
  'mkdir -p "$MOTORCARE_ATTACHMENTS_RESTORE_PATH" && cd "$MOTORCARE_ATTACHMENTS_RESTORE_PATH" && tar -xzf -' \
  < "$ATTACHMENTS_ARCHIVE"

echo "Restore drill completed: ${ENVIRONMENT}/${TARGET_DATABASE}"
echo "Attachment restore-check path retained for validation: $ATTACHMENTS_RESTORE_PATH"
