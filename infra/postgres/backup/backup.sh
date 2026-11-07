#!/usr/bin/env bash
set -euo pipefail

TIMESTAMP="$(date +%Y-%m-%dT%H-%M-%S)"
DUMP_FILE="/app/dumps/echo_${TIMESTAMP}.dump"

BACKUP_START_TIMESTAMP=$TIMESTAMP
echo "[${BACKUP_START_TIMESTAMP}] Starting backup of ${DB_NAME}"

pg_dump -Fc -h db -U "${DB_USERNAME}" "${DB_NAME}" >"${DUMP_FILE}"

pg_restore -l "${DUMP_FILE}" >/dev/null

echo "[${BACKUP_START_TIMESTAMP}] Written: ${DUMP_FILE} ($(du -h "${DUMP_FILE}" | cut -f1))"

find /app/dumps -name "echo_*.dump" -mtime +30 -delete
echo "[${BACKUP_START_TIMESTAMP}] Pruned dumps older than 30 days"

echo "[${BACKUP_START_TIMESTAMP}] Backups complete."
