# Observability

**Last updated:** 2026-10-30 · Plan and open decisions: [observability-plan.md](observability-plan.md)

Metrics, traces and logs for the app and the machine it runs on, viewed in Grafana.

```
api ──OTLP──▶ Alloy ──▶ Tempo        traces, 7 days
                   ├──▶ Prometheus   metrics, 30 days
                   └──▶ Loki         logs, 28 days
docker.sock ─▶ Alloy ─▶ Loki         container logs: nginx and Postgres only (labelled container, host, stream)
host + containers + Postgres ─▶ Alloy ─▶ Prometheus
                                     Grafana reads all three
```

In production this is **two servers**: the app server runs the app and Alloy, the observability server runs Loki,
Prometheus, Tempo and Grafana. See "Production: two servers" below.

## Two compose projects

| Folder | Project | What |
|---|---|---|
| `infra/observability` | `obs` | Loki, Prometheus, Tempo, Grafana |
| `infra/alloy` | `alloy` | The agent: receives OTLP from the API, tails Docker logs, exports container, host and Postgres metrics |

Alloy joins both Docker networks (`obs_default` and `echo_echo-network`), so **start them in this order**:

```bash
docker compose up -d                                  # the app: creates echo_echo-network
(cd infra/observability && docker compose up -d --build)   # creates obs_default
(cd infra/alloy && docker compose up -d --build)           # also loads docker-compose.override.yaml (dev only)
```

One-time, for the Postgres metrics: create the read-only role and put its DSN in `infra/alloy/.env`
(copy `.env.example`):

```bash
MONITOR_PASSWORD=<long random> ./infra/postgres/create-monitor-role.sh
```

The API sends to Alloy: `OTEL_EXPORTER_OTLP_ENDPOINT=http://alloy:4317`. Config files are copied into the
images, so after editing one run `docker compose up -d --build <service>` in its folder.

## Ports (all bound to 127.0.0.1)

| Port | What |
|---|---|
| 3000 | Grafana (user `admin`, password `GF_ADMIN_PASSWORD` in `infra/observability/.env`, git-ignored) |
| 9090 | Prometheus |
| 3100 | Loki |
| 4317 / 4318 | Alloy: OTLP in (gRPC / HTTP) |
| 12345 | Alloy's own UI and API |

## What is collected

| Data | From | Notes |
|---|---|---|
| API metrics | OTLP, once a minute | ASP.NET Core, Kestrel, .NET runtime, HTTP client, rate limiting, `auth_*` counters, `echo_congregation_request_volume_total` |
| Traces | OTLP | Includes Npgsql spans; health checks are left out |
| API logs | OTLP | **Warnings and above only** (Alloy drops the rest). Carry `trace_id`, so Grafana links a log line to its trace and back |
| API connection pool | OTLP (Npgsql meter) | `db_client_connection_*` (used, idle, limit), `db_client_operation_duration_*` |
| Summary cache | OTLP | `echo_summary_requests_total{summary, cached}` |
| Container metrics | Alloy's cAdvisor exporter | `container_*`, labelled `container` (the name used by the logs), `host`, `image`. Allow-listed in `config.alloy` |
| Host metrics | Alloy's unix exporter | `node_*`: CPU, load, memory, disk, network |
| Postgres metrics | Alloy's Postgres exporter, as the read-only `monitor` role | `pg_*`: connections, transactions, cache blocks, locks, size, tables, WAL, and `pg_stat_statements` per query id |
| nginx logs | Docker socket | One JSON line per request; `status` and `method` are labels. Kept 14 days |
| Postgres logs | Docker socket | Slow statements (> 200 ms), plans (> 500 ms), lock waits, checkpoints |

**Deliberately not collected:** the API's stdout (it sends the same lines over OTLP), the Vite dev server, Portainer,
the observability containers' own logs, and API info-level logs (the traces show them).

**Privacy of the logs.** nginx logs the path **without** its query string (`$uri`), because email-verification and
password-reset tokens travel in query strings. Postgres leaves bound parameter values out of its slow-statement log
(`log_parameter_max_length = 0`), because they can be names and emails. The `monitor` role can read statistics and
nothing else (`permission denied` on any table).

Alloy reads the host through read-only mounts (`/`, `/proc`, `/sys`, `/var/lib/docker`, the containerd socket)
and runs with the host's process and cgroup view. That, plus the Docker socket, is root-equivalent access to the
machine: keep Alloy's ports on localhost.

## Dashboards

JSON files in `infra/observability/grafana/dashboards/`, loaded by `provisioning/dashboards/echo.yml`.
**The files are the source of truth.** To change one: edit it in Grafana, use *Export → Export as JSON*,
replace the file, commit. A change made only in the UI is lost when Grafana restarts.

| Dashboard | uid | For |
|---|---|---|
| Echo / API | `echo-api` | Traffic, latency, errors, .NET runtime |
| Echo / Tenants and product | `echo-tenants` | Use by church, sign-ups, invitations |
| Echo / Auth and abuse | `echo-auth` | Sign-in health, failed logins, refused requests |
| Platform / Containers and host | `platform-containers` | CPU, memory, network, disk per container and for the machine |
| Echo / Overview | `echo-overview` | One screen: API, database, machine, front door, latest problems |
| Platform / Database | `platform-database` | Postgres and the API's connection pool, slow statements, scans without an index |
| Echo / Edge (nginx) | `echo-edge` | What nginx answered and refused (429, 5xx, 499), request times |

## Production: two servers

```
app server                                   observability server
┌──────────────────────────┐   private net   ┌────────────────────────────────────┐
│ nginx · api · database   │                 │ Loki  :3100   Prometheus :9090     │
│ Alloy ───────────────────┼────────────────▶│ Tempo :4317                        │
└──────────────────────────┘                 │ Grafana (not published)            │
                                             │ proxy :80/:443 ◀── developers (HTTPS, login)
                                             └────────────────────────────────────┘
```

**Observability server** (`infra/observability`):

```bash
cp .env.example .env     # set GF_ADMIN_PASSWORD, GRAFANA_DOMAIN, OBS_BIND_IP (its PRIVATE address)
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

- Loki, Prometheus and Tempo are published **only on `OBS_BIND_IP`**. Firewall them so only the app server's private
  address can reach 3100, 9090 and 4317. Never open them to the internet.
- Grafana is not published. The only way in is the `proxy` (Caddy) on ports 80 and 443, which gets a certificate for
  `GRAFANA_DOMAIN` by itself. Create the DNS record first.
- Open to the internet: 80 and 443 only (plus SSH, from your address).

**App server** (`infra/alloy`): copy `.env.example` to `.env`, set `LOKI_URL`, `PROMETHEUS_URL` and
`TEMPO_OTLP_ENDPOINT` to the observability server's private address, `ALLOY_HOST=echo-app`, then start without the
dev override:

```bash
docker compose -f docker-compose.yaml up -d --build
```

The API keeps `OTEL_EXPORTER_OTLP_ENDPOINT=http://alloy:4317` (Alloy is on the app's own Docker network).

**Who can open Grafana.** Developers open `https://<GRAFANA_DOMAIN>` and sign in with their own account: no SSH,
no VPN. In Grafana: *Administration → Users and access → Users → New user*. Give developers the **Editor** role
(they can use Explore for logs and traces; the provisioned dashboards cannot be changed from the UI) and keep
**Admin** for yourself. Sign-up and anonymous access are off, sessions end after 7 days unused, cookies are
HTTPS-only, and Grafana locks an account after repeated wrong passwords. Single sign-on (for example GitHub,
limited to your organisation) would remove separate passwords and drop a leaver's access with their GitHub
membership; it is a later step.

## Gotchas

- **Postgres collectors are listed in full** in `config.alloy`. Naming only `stat_statements` left the exporter
  with a tiny default set. Checkpoint metrics are not in this exporter version; checkpoints are in the Postgres log.
- **`pg_stat_statements` here is by `queryid` only.** To read the SQL: `SELECT query FROM pg_stat_statements WHERE
  queryid = <id>`. The slow-statement log shows SQL as it ran.

- **Rate panels need `timeInterval: 15s`** on the Prometheus datasource (set in `provisioning/datasources`), matching
  the 15 s scrape interval and the API's `OTEL_METRIC_EXPORT_INTERVAL=15000`. A request shows on a panel about 20 s
  later. (The .NET default export is 60 s, which made activity look minutes late.)
- **A counter has no series until it first happens.** `auth_registration_*`, `auth_password_reset_*` and the
  like are empty on a fresh install. That is not a fault.
- **A counter's first event is counted only in windows that reach back to the API's start.** The API's metrics go to
  Prometheus's OTLP endpoint (`/api/v1/otlp`, via Alloy) with `created-timestamp-zero-ingestion` on, so Prometheus
  records a zero at API start. Stat panels over a range (`$__range`, 24h) therefore include the first failed login or
  first hit of a summary. A short window (`[15s]`, `[5m]`) over a long-running API still ignores a brand-new
  series' first event; later events show normally.
- **Logs are shaped in Alloy, not stored raw.** nginx's JSON line becomes `GET /path · 200 · 0.04 s · ip`, with
  `status`, `method` and `level` as labels and `path`, `rt`, `ip` as structured metadata (query them without
  `| json`). Postgres lines keep only the message (`level`, `pid` as fields; multi-line statements joined). API logs
  arrive over OTLP with the message only. Lines written before this change stay raw until retention removes them.
- **cAdvisor needs containerd.** This host stores images through containerd, so Alloy mounts
  `/run/containerd/containerd.sock`; without it cAdvisor sees only the root cgroup and no containers.
- **Memory-limit and CPU-throttle panels are empty** until a container has a limit set.
- Metric labels must stay low-cardinality: no user, member or email ids. `congregation_id` is fine; show it
  with `topk`.
