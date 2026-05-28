#!/usr/bin/env sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
CONTAINER_NAME="${1:-motorcare-staging-postgres}"
BACKUP_DIR="${2:-${MOTORCARE_BACKUP_DIR:-/opt/motorcare-backups}}"
API_CONTAINER_NAME="${3:-motorcare-staging-api}"
ATTACHMENTS_PATH="${4:-${Storage__AttachmentsPath:-/var/lib/motorcare/attachments}}"

exec sh "$SCRIPT_DIR/backup-motorcare.sh" staging "$BACKUP_DIR" "$CONTAINER_NAME" "$API_CONTAINER_NAME" "$ATTACHMENTS_PATH"
