#!/usr/bin/env sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
CONTAINER_NAME="${1:-motorcare-staging-postgres}"
BACKUP_DIR="${2:-${MOTORCARE_BACKUP_DIR:-/opt/motorcare-backups}}"

exec sh "$SCRIPT_DIR/backup-postgres.sh" staging "$BACKUP_DIR" "$CONTAINER_NAME"
