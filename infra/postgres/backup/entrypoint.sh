#!/bin/sh
set -e

: "${BACKUP_INTERVAL_SECONDS:=86400}"

LOG_FILE="/db_dumps/backup.log"

log() {
    echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] $1" | tee -a "$LOG_FILE"
}

log "Backup service starting. Interval: ${BACKUP_INTERVAL_SECONDS}s"

while true; do
    log "Running backup..."
    /backup.sh >> "$LOG_FILE" 2>&1
    log "Backup complete. Next run in ${BACKUP_INTERVAL_SECONDS}s"
    sleep "$BACKUP_INTERVAL_SECONDS"
done
