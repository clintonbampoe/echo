# Database Backups

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

## Overview

Scheduled backups via `backup` container using **Supercronic** (not standard cron) for reliable environment variable passing.

## Configuration

- **Client**: `postgresql18-client` (must match server version)
- **Format**: Postgres custom binary (`-Fc`) — compressed, selective restore
- **Logging**: `2>&1 | tee -a /app/dumps/backup.log` — captures stdout/stderr in Docker logs and file
- **Schedule**: Crontab copied into image AND mounted as volume — change schedule on host without rebuild

## Files

- `infra/postgres/backup/Dockerfile` — backup container
- `infra/postgres/backup/backup.sh` — backup script
- `infra/postgres/backup/restore.sh` — restore script
- `infra/postgres/backup/dumps/` — mounted volume for dump files