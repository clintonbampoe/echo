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

## Postgres tuning

`infra/postgres/conf/tuning.conf` (mounted with the rest of `conf/`) is sized for the production box: 2 vCPU, and
2 GB for the database container. Highlights: `max_connections` 40 (the API pool holds at most 30), `shared_buffers`
768 MB, `work_mem` 8 MB, `jit` off (every query runs in milliseconds), `max_parallel_workers_per_gather` 0 (parallel
queries would take cores from the API), `random_page_cost` 1.1 (SSD), and larger, less frequent checkpoints. Changing
`max_connections` or `shared_buffers` needs a restart of the `db` container. The API also prepares statements that
run twice (`Max Auto Prepare` 64) and reuses its `DbContext` objects (`AddDbContextPool`).
