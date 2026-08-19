#!/usr/bin/env bash
set -euo pipefail

TIMESTAMP="$(date +%Y-%m-%dT%H-%M-%S)"
DUMP_FILE="/db_dumps/echo_${TIMESTAMP}.dump"

BACKUP_START_TIMESTAMP="$(date +%H-%M-%S)"
echo "[${BACKUP_START_TIMESTAMP}] Starting backup of ${DB_NAME}"

# -Fc custom format - compressed by default, supports selective restore via pg_restore
# No separate gzip step needed.

pg_dump -Fc -h db -U "${DB_USERNAME}" "${DB_NAME}" >"${DUMP_FILE}"

echo "[${BACKUP_START_TIMESTAMP}] Written: ${DUMP_FILE} ($(du -h "${DUMP_FILE}" | cut -f1))"

# Keep last 30 days of local db_dumps
find /db_dumps -name "echo_*.dump" -mtime +30 -delete
echo "[${BACKUP_START_TIMESTAMP}] Pruned dumps older than 30 days"

echo "[${BACKUP_START_TIMESTAMP}] Backups complete."
