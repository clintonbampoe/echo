# Observability

Metrics, traces and logs for the app and the machine it runs on, viewed in
Grafana. It runs on the same server as the app, in the same compose project,
behind the `obs` profile. Uptime and job checks are a separate piece, the
[status page](./status-page.md).

```
api ──OTLP──▶ Alloy ──▶ Tempo        traces, 7 days
                   ├──▶ Prometheus   metrics, 30 days
                   └──▶ Loki         logs, 28 days
docker.sock ─▶ Alloy ─▶ Loki         container logs: nginx and Postgres only
host + containers + Postgres ─▶ Alloy ─▶ Prometheus
                                     Grafana reads all three
```

## Run it

Development, from the repo root:

```bash
docker compose --profile obs up -d
```

Everything is bound to localhost: Grafana `3000`, Prometheus `9090`, Loki
`3100`, Alloy `4317` and `4318` (OTLP in) and `12345` (its own UI). Sign in to
Grafana as `admin` with `GF_ADMIN_PASSWORD` from `.env`.

Production starts the same services on every deploy. Grafana is at
`https://<domain>/grafana/`, behind its own login. Config files are baked into
the images, so a config change ships with a release.

The API sends to Alloy with `OTEL_EXPORTER_OTLP_ENDPOINT=http://alloy:4317`.
Alloy reads Postgres metrics as a read-only role `monitor`. A new database
creates the role from `MONITOR_PASSWORD`; on a database that predates it, run
`docker exec database /docker-entrypoint-initdb.d/monitor-role.sh` once.

## What is collected

| Data | From | Notes |
|---|---|---|
| API metrics | OTLP, every 15 s | ASP.NET Core, Kestrel, .NET runtime, HTTP client, rate limiting, `auth_*` counters, `echo_congregation_request_volume_total` |
| Traces | OTLP | Includes Npgsql spans; health checks are left out. The API traces every request; Alloy keeps a trace that had an exception, a 5xx, or a 400, 403, 409 or 429, plus `TRACES_SAMPLE_PERCENT` of the rest |
| API logs | OTLP | Warnings and above only. They carry `trace_id`, so Grafana links a line to its trace and back |
| API connection pool | OTLP (Npgsql meter) | `db_client_connection_*` (used, idle, limit), `db_client_operation_duration_*` |
| Summary cache | OTLP | `echo_summary_requests_total{summary, cached}` |
| Container metrics | Alloy's cAdvisor exporter | `container_*`, labelled `container`, `host`, `image`; an allow-list in `config.alloy` |
| Host metrics | Alloy's unix exporter | `node_*`: CPU, load, memory, disk, network |
| Postgres metrics | Alloy's Postgres exporter | `pg_*`: connections, transactions, cache, locks, size, tables, WAL, and `pg_stat_statements` per query id |
| nginx logs | Docker socket | Every error and a random 1% of other requests, one JSON line each. Errors kept 28 days, the sample 7 |
| Postgres logs | Docker socket | Slow statements (over 200 ms), plans (over 500 ms), lock waits, checkpoints |

Not collected, on purpose: the API's stdout (the same lines arrive over OTLP),
the Vite dev server, and the monitoring containers' own logs.

Retention is set by `LOKI_RETENTION`, `LOKI_NGINX_RETENTION`,
`TEMPO_RETENTION` and `PROMETHEUS_RETENTION`; see
[Environment](../reference/environment.md).

### Privacy

- nginx logs the path without its query string, because email-verification
  and password-reset tokens travel in query strings.
- Postgres leaves bound parameter values out of its slow-statement log
  (`log_parameter_max_length = 0`), because they can be names and emails.
- The `monitor` role can read statistics and nothing else.

Alloy reads the host through read-only mounts (`/`, `/proc`, `/sys`,
`/var/lib/docker`, the containerd socket) and the Docker socket. That is
root-equivalent access to the machine: keep Alloy's ports on localhost.

## Logging

Only a small share of normal traffic is recorded; everything that went wrong
always is.

| Source | Normal requests | Warnings and errors |
|---|---|---|
| API logs | off (`API_LOG_LEVEL` is `Warning`; set `Information` to see requests) | all |
| API traces | 1% of the fast successful ones (`TRACES_SAMPLE_PERCENT`) | all, whole: 5xx, the 4xx that matter (400, 403, 409, 429) and anything over 100 ms are always kept (tail sampling in Alloy) |
| nginx access log | a random 1%, each line marked `"sampled":1` | all 4xx and 5xx |

The nginx sample is uniform, so it can be scaled back up: a request count is
the lines with `sampled="1"` times 100, and a request-time percentile is taken
over those lines only (the Edge and Overview dashboards do both). Slow requests
are not kept on purpose, since that would make the sample look slower than the
traffic is. Exact request counts and times come from the API's metrics in
Prometheus.

Every service writes through Docker's `json-file` driver, capped at 10 files
of 10 MB, 100 MB per container, oldest rotated out. `docker compose logs` shows
only that window; Loki holds the history. Compose sets the cap once
(`x-logging: &default-logging`), and every new service must add
`logging: *default-logging`, or its logs are uncapped.

A container keeps the logging settings it was created with. After changing
them, recreate it with `docker compose up -d --force-recreate <service>`.

## Dashboards

JSON files in `infra/observability/grafana/dashboards/`, loaded by
`provisioning/dashboards/echo.yml`. **The files are the source of truth.** To
change one, edit it in Grafana, choose *Export, Export as JSON*, replace the
file and commit. A change made only in the UI is lost when Grafana restarts.

| Dashboard | uid | For |
|---|---|---|
| Echo / Overview | `echo-overview` | one screen: API, database, machine, front door, latest problems |
| Echo / API | `echo-api` | traffic, latency, errors, .NET runtime |
| Echo / Tenants and product | `echo-tenants` | use by church, sign-ups, invitations |
| Echo / Auth and abuse | `echo-auth` | sign-in health, failed logins, refused requests |
| Echo / Edge (nginx) | `echo-edge` | what nginx answered and refused (429, 5xx, 499), request times |
| Platform / Containers and host | `platform-containers` | CPU, memory, network, disk per container and for the machine |
| Platform / Database | `platform-database` | Postgres and the connection pool, slow statements, scans without an index |
| k6 load test | `k6-load` | a load test as it runs ([run one](../how-to/run-a-load-test.md)) |

## Who can open Grafana

Anyone with an account, at `https://<domain>/grafana/`. Create each person in
*Administration, Users and access, Users, New user*. Give developers the
**Editor** role (they can use Explore for logs and traces; provisioned
dashboards cannot be changed from the UI) and keep **Admin** for yourself.
Sign-up and anonymous access are off, sessions end after 7 days unused, cookies
are HTTPS-only, and Grafana locks an account after repeated wrong passwords.
The admin password in `.env` only seeds the first start. Change it in the
Grafana UI (profile, change password), or from the server if you are locked out:

```bash
docker exec grafana grafana cli admin reset-admin-password '<new password>'
```

Neither needs a restart.

## Gotchas

- **The Postgres collectors are listed in full** in `config.alloy`. Naming only
  `stat_statements` left the exporter with a tiny default set. Checkpoint
  metrics are not in this exporter version; checkpoints are in the Postgres
  log.
- **`pg_stat_statements` is by `queryid` only.** To read the SQL:
  `SELECT query FROM pg_stat_statements WHERE queryid = <id>`.
- **Rate panels need `timeInterval: 15s`** on the Prometheus datasource (set in
  `provisioning/datasources`), matching the 15 s scrape interval and
  `OTEL_METRIC_EXPORT_INTERVAL=15000`. A request shows on a panel about 20 s
  later. The .NET default export is 60 s, which made activity look minutes late.
- **A counter has no series until it first happens.** `auth_registration_*`,
  `auth_password_reset_*` and the like are empty on a fresh install. That is
  not a fault.
- **A counter's first event is counted only in windows that reach back to the
  API's start.** Prometheus's OTLP endpoint has `created-timestamp-zero-ingestion`
  on, so it records a zero at API start. A range panel (`$__range`, 24 h)
  includes the first failed login; a short window (`[15s]`, `[5m]`) over a
  long-running API ignores a new series' first event.
- **Logs are shaped in Alloy, not stored raw.** nginx's JSON line becomes
  `GET /path · 200 · 0.04 s · ip`, with `status`, `method` and `level` as
  labels and `path`, `rt`, `ip` as structured metadata (query them without
  `| json`). Postgres lines keep only the message. API logs arrive over OTLP
  with the message only.
- **cAdvisor needs containerd.** This host stores images through containerd, so
  Alloy mounts `/run/containerd/containerd.sock`; without it cAdvisor sees only
  the root cgroup and no containers.
- **Memory-limit and CPU-throttle panels are empty** until a container has a
  limit. Production sets them on every container.
- **Keep metric labels low-cardinality:** no user, member or email ids.
  `congregation_id` is fine; show it with `topk`.
