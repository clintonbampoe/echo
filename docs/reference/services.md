# Services, ports and volumes

What runs, what it can use, what it stores. One compose project, `echo`.

## Compose files

| File | Loaded | Purpose |
|---|---|---|
| `docker-compose.yml` | always | all services, shared settings |
| `docker-compose.override.yml` | automatically in dev | published ports, Vite, source mounts |
| `docker-compose.prod.yml` | `-f` on the server | memory caps, CPU weights, HTTPS, Grafana sub-path |

Naming `-f docker-compose.prod.yml` switches the dev override off, so dev and
production settings cannot both apply.

```bash
docker compose up                                                        # dev
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d    # production
```

Profiles: `obs` (monitoring and status), `db-tools` (backup, migrator,
seeder). A plain `up` starts neither.

## Services in production

Memory is a hard limit with no swap: a container over its limit restarts
instead of slowing the server. CPU is a weight, not pinned cores: it only
matters when the CPUs are busy, and then the higher weight wins.

| Service | Group | Memory | CPU weight | What it is |
|---|---|---|---|---|
| `nginx` | core | 128 MB | 2048 | the only public entry: `/`, `/api`, `/grafana/`; HTTPS; `status.<domain>` |
| `api` | core | 1 GB | 1024 (default) | ASP.NET Core API |
| `db` | core | 2.5 GB | 2048 | Postgres 18, tuned in `infra/postgres/conf/` |
| `backup` | core | 512 MB | 128 | nightly `pg_dump` at 02:00 UTC, reports to the status page |
| `certbot` | core | 256 MB | 128 | renews the certificate daily at 03:30 UTC, reports to the status page |
| `alloy` | obs | 256 MB | 512 | collects logs, metrics and traces |
| `loki` | obs | 768 MB | 256 | log store |
| `prometheus` | obs | 1 GB | 256 | metric store |
| `tempo` | obs | 512 MB | 256 | trace store |
| `grafana` | obs | 384 MB | 256 | dashboards, at `/grafana/` |
| `status` | obs | 128 MB | 128 | Gatus, at `status.<domain>` |
| `migrator` | db-tools | 512 MB | – | runs EF migrations; one-shot |
| `seeder` | db-tools | – | – | loads fake data; development only |

The group is the deploy's, not a compose profile: `core` is the list
`deploy.sh` starts and waits for, and `backup` is in it even though compose puts
it under the `db-tools` profile. The deploy starts the `obs` services
separately, so a monitoring failure does not roll a release back.

The sizes add up to about 7.4 GB if every container reached its limit at once.
That does not happen; the box has 8 GB. Re-check the caps after a load test
against the container memory panels.

Every deploy restarts every container, the database included. That is on
purpose: it keeps one rule for all services.

## Ports

Only nginx is reachable from outside in production.

| Port | Service | Dev | Production |
|---|---|---|---|
| 80 | nginx | – | yes: certificate challenge, redirect to HTTPS |
| 443 | nginx | – | yes |
| 8080 | nginx | yes | – |
| 5173 | Vite (`client`) | yes | – |
| 5025 | api | yes | – |
| 5432 | db | yes | – |
| 3000, 3100, 9090, 12345, 4317, 4318 | Grafana, Loki, Prometheus, Alloy | localhost only | – |

The cloud firewall allows 22, 80 and 443 only.

## Volumes

| Volume | Holds | Safe to delete? |
|---|---|---|
| `postgres-data` | the database | **No.** Deleting it deletes all data. |
| `api-keys` | ASP.NET data-protection keys | No: sessions and tokens that use them stop working |
| `letsencrypt` | certificates and renewal state | Only if you can reissue (rate limits apply) |
| `certbot-webroot` | challenge files | Yes |
| `loki-data`, `prometheus-data`, `tempo-data` | monitoring history | Yes: history is lost |
| `grafana-data` | Grafana users and settings | No: it holds the admin password |
| `alloy-data` | collector position files | Yes |
| `status-data` | status-page history and uptime percentages | Yes: history is lost |
| `nuget-cache` | NuGet packages, dev only | Yes |

Backup files go to `BACKUP_DIR` on the host (default
`infra/postgres/backup/dumps/`), not into a volume.

Postgres only creates its user and database when it starts on an empty
volume. If a start with the wrong settings created the volume, later starts
reuse it. See [Troubleshooting](../troubleshooting.md).

## Postgres settings

`infra/postgres/conf/tuning.conf` is sized for 4 vCPU and a 2.5 GB container:
`max_connections` 40, `shared_buffers` 1 GB, `effective_cache_size` 2 GB,
`work_mem` 8 MB,
`max_parallel_workers_per_gather` 2, `jit` off, `random_page_cost` 1.1 (SSD).
The config is baked into the image, so a change ships with a release.
`max_connections` and `shared_buffers` need a container restart, which every
deploy does.
