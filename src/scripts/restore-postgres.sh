#!/usr/bin/env sh
set -eu

usage() {
  cat <<'USAGE'
Usage: restore-postgres.sh <staging|production> <backup-file> [target-database] [container-name]

The default target database is motorcare_restore_check.
The script refuses to target the live POSTGRES_DB database.
USAGE
}

if [ "$#" -lt 2 ] || [ "$#" -gt 4 ]; then
  usage >&2
  exit 2
fi

ENVIRONMENT="$1"
BACKUP_FILE="$2"
TARGET_DATABASE="${3:-motorcare_restore_check}"

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

CONTAINER_NAME="${4:-$DEFAULT_CONTAINER_NAME}"

if [ ! -s "$BACKUP_FILE" ]; then
  echo "Backup file is missing or empty: $BACKUP_FILE" >&2
  exit 1
fi

if ! printf '%s' "$TARGET_DATABASE" | grep -Eq '^[A-Za-z_][A-Za-z0-9_]*$'; then
  echo "Unsafe target database name: $TARGET_DATABASE" >&2
  exit 2
fi

SOURCE_DATABASE="$(docker exec "$CONTAINER_NAME" sh -lc 'printf "%s" "$POSTGRES_DB"')"
if [ -z "$SOURCE_DATABASE" ]; then
  echo "Could not determine live POSTGRES_DB from $CONTAINER_NAME" >&2
  exit 1
fi

if [ "$TARGET_DATABASE" = "$SOURCE_DATABASE" ]; then
  echo "Refusing to restore into live database: $TARGET_DATABASE" >&2
  exit 1
fi

echo "Validating backup archive: $BACKUP_FILE"
docker exec -i "$CONTAINER_NAME" sh -lc 'pg_restore --list >/dev/null' < "$BACKUP_FILE"

echo "Recreating restore-check database: $TARGET_DATABASE"
docker exec "$CONTAINER_NAME" sh -lc "dropdb -U \"\$POSTGRES_USER\" --if-exists --force '$TARGET_DATABASE'"
docker exec "$CONTAINER_NAME" sh -lc "createdb -U \"\$POSTGRES_USER\" '$TARGET_DATABASE'"

echo "Restoring archive into $TARGET_DATABASE"
docker exec -i "$CONTAINER_NAME" sh -lc \
  "pg_restore -U \"\$POSTGRES_USER\" -d '$TARGET_DATABASE' --clean --if-exists --no-owner --no-acl" \
  < "$BACKUP_FILE"

echo "Restore drill completed: ${ENVIRONMENT}/${TARGET_DATABASE}"
echo "The restore-check database is retained for verification; drop it after smoke checks."
