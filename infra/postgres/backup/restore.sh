#!/usr/bin/env bash
set -euo pipefail

DUMP_FILE="${1:?Usage: restore.sh <path-to-dump-file>}"

if [ ! -f "$DUMP_FILE" ]; then
    echo "Error: dump file not found: ${DUMP_FILE}"
    exit 1
fi

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Restoring ${DB_NAME} from ${DUMP_FILE}"

pg_restore -h db -U "${DB_USERNAME}" -d "${DB_NAME}" --clean --if-exists "${DUMP_FILE}"

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Restore complete."
