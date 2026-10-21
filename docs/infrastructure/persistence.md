# Persistence & Volumes

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

| Volume | Purpose | Safe to delete? |
|---|---|---|
| `postgres-data` | Database files | **No** — deletes all data |
| `nuget-cache` | NuGet package cache (dev) | **Yes** — rebuilds on next run |
| `backup-dumps` | Database backup files | **No** — deletes backups |

Volumes are defined in `docker-compose.yml` and survive container recreation.