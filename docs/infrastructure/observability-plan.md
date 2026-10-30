# Observability — Review and Plan

**Written:** 2026-10-30 · **Status:** Phases 0 to 4 are built; alerts wait for the load test, and production is prepared but untested (see Progress)

What exists today, what is missing, and the order to fix it in. Then the dashboards: which ones, what
each answers, and where the data comes from. Everything marked *verified* was checked against the running
stack on 2026-10-30.

### Progress

| Item | State |
|---|---|
| Phase 0: tidy | **Done** |
| Phase 1: container and host metrics | **Done** (10 containers and the host) |
| Phase 2: database and app metrics | **Done**: Postgres exporter with a read-only role, Npgsql pool meter, summary-cache counter. nginx is covered by its logs |
| Phase 3: logs, narrow set | **Done**: duplicates dropped, API warn+ only, nginx as JSON with labels, 14-day nginx retention, no query strings or parameter values in logs |
| Phase 4: dashboards | **7 built**: Overview, API, Tenants, Auth, Containers and host, Database, Edge. Not built: backup age (needs a metric from the backup job) |
| Phase 5: alerts | **Waiting** (decision 3) |
| Phase 6: production | **Prepared, untested**: two-server compose files, private-address ports, Grafana behind HTTPS with logins. Cannot be tried until the servers exist |

---

## 1. What exists today

```
api ──OTLP──▶ Alloy ──▶ Tempo        (traces, 7 days)
                   ├──▶ Prometheus   (metrics, remote write, 30 days)
                   └──▶ Loki         (logs, 28 days)
docker.sock ─▶ Alloy ─▶ Loki         (container stdout, labelled container/host/stream)
                                     Grafana reads all three (datasources provisioned)
```

Two compose projects: `infra/alloy` (the agent) and `infra/observability` (Loki, Prometheus, Tempo,
Grafana). Alloy joins both networks, so on the current machine everything is one host. Ports are bound to
`127.0.0.1`. The admin password lives in `infra/observability/.env` (git-ignored — *verified*).

**Working well**

- App metrics arrive (*verified*, 392 series names): ASP.NET Core, Kestrel, .NET runtime (GC, heap, thread
  pool), HTTP client, rate limiting, 9 auth counters, and `echo_congregation_request_volume_total`.
- Traces include Npgsql spans. Logs arrive natively over OTLP with `trace_id`, and Grafana links log → trace
  and trace → logs both ways.
- Postgres is already prepared: `pg_stat_statements`, slow-statement log (> 200 ms), `auto_explain`,
  lock-wait and checkpoint logging.
- Retention is set per store; volumes are tiny (Loki 0.8 MB, Prometheus 5 MB, Tempo 38 MB).

**Gaps and problems**

| # | Finding | Why it matters |
|---|---|---|
| 1 | **No container or host metrics at all.** Both `node-exporter` scrape jobs are down (*verified*): nothing listens on `:9100`, and `127.0.0.1` from inside the Prometheus container is itself. `node_*` and `container_*` return zero series. | You cannot see CPU, memory, restarts, OOM kills or disk. This is the ask. |
| 2 | **No database metrics.** Postgres is instrumented for logs, but nothing exports connections, cache hit ratio, locks, size growth. The app does not export the Npgsql connection-pool meter either. | The DB is the part that will fail first (3 GB in dev; pool exhaustion). |
| 3 | **No edge metrics.** nginx has no `stub_status` and logs in the default format (no request time). | The only place a 429, 499 or 502 from the front door shows up. |
| 4 | **API logs are shipped twice**: once from the container's stdout (`container="api"`) and once over OTLP (`service_name="Echo.Api"`). | Double the lines, different labels, confusing in queries. |
| 5 | **No dashboards, no alerts.** Provisioning only contains datasources. | Data nobody looks at does not help at 2 a.m. |
| 6 | `prometheus.yml` labels two hosts (`echo-app`, `obs`) but Alloy attaches to both Docker networks on one host. | The target topology is unclear. Decides where the agent runs and what is exposed. |
| 7 | The old `collector` (otel-lgtm) service is still in `docker-compose.yml`, on ports 3000 and 9090 — the same ports the new Grafana and Prometheus use. | A trap: `--profile observability` would collide. |
| 8 | Docs are stale: `logging.md` says Loki is "not set up yet"; nothing describes the new stack. | |

---

## 2. Plan

Ordered so each step is useful on its own. Sizes: S ≈ an hour, M ≈ half a day.

### Phase 0 — Tidy (S)
- Delete the `collector` service and the `observability` profile from `docker-compose.yml`.
- Remove the two dead `node-exporter-*` jobs from `prometheus.yml` (Phase 1 replaces them).
- Write `docs/infrastructure/observability.md` (the diagram above, ports, retention, how to start/stop) and
  fix `logging.md`.

### Phase 1 — Container and host metrics (M) — *the ask*
**Use Alloy's built-in exporters, so no new containers.** Alloy already runs with the Docker socket.

```alloy
// Sketch: check argument names against the Alloy v1.21 docs when implementing.
prometheus.exporter.cadvisor "containers" {
  docker_host  = "unix:///var/run/docker.sock"
  docker_only  = true            // only Docker containers, not every cgroup
  storage_duration = "5m"
}
prometheus.exporter.unix "host" {
  procfs_path = "/host/proc"
  sysfs_path  = "/host/sys"
  rootfs_path = "/host/root"
}
prometheus.scrape "containers" { targets = prometheus.exporter.cadvisor.containers.targets  ... }
prometheus.scrape "host"       { targets = prometheus.exporter.unix.host.targets            ... }
```

- Mount read-only into the Alloy container: `/:/host/root`, `/proc:/host/proc`, `/sys:/host/sys`,
  `/var/lib/docker`, `/dev/disk`.
- **Relabel cAdvisor's `name` to `container`** so a container's metrics and its logs share the same label and
  one dashboard variable drives both.
- **Allow-list the metrics** (cAdvisor emits hundreds): CPU, working-set memory, memory limit, network and
  disk bytes, start time (restarts), OOM events, last-seen. Keeps series count and disk small.
- Host: CPU, load, memory, swap, disk space and IO, filesystem fill rate, network.
- **What Phase 1 needed in practice** (the sketch above was not enough): Alloy must run with
  `cgroup: host` and `pid: host`, mount `/sys` at `/sys`, and mount `/run/containerd/containerd.sock` because this
  host stores images through containerd (without it cAdvisor sees only the root cgroup). Set
  `store_container_labels = false`, or every Docker label becomes a metric label. Also, the datasource needs
  `timeInterval: 15s` (API export interval is 15 s, see observability.md).
- Alternative considered: a standalone cAdvisor + node-exporter pair. Rejected: two more containers and
  more privileged mounts for the same data.

### Phase 2 — Database and app-side metrics (M)
- **Postgres:** `prometheus.exporter.postgres` in Alloy with a dedicated read-only role (`GRANT pg_monitor`),
  created in `infra/postgres/init`. Gives connections vs `max_connections`, cache hit ratio, transactions,
  locks and deadlocks, temp files, database size, checkpoints.
- **Npgsql pool and command metrics:** one line in `ServiceDefaults` (`metrics.AddNpgsqlInstrumentation()`;
  the `Npgsql.OpenTelemetry` package is already referenced). Gives open/idle connections, pool waits,
  command duration.
- **Summary cache:** a counter `echo.summary.requests{name, cached}` in `SummaryCache`, so the 2-minute cache's
  hit ratio is visible.
- **Top queries:** do *not* export `pg_stat_statements` as metrics. Add a read-only **Postgres datasource** in
  Grafana and show it as a table. This is exactly the view the last performance round was done by hand with.
- **nginx:** add `stub_status` on an internal-only location and the small `nginx-prometheus-exporter`
  container, **or** derive edge numbers from access logs (Phase 3). Recommend the logs: no new container, and
  it needs the log-format change anyway.

### Phase 3 — Logs: narrow, not everything (S–M)
You said logs probably are not worth it. The numbers say they are cheap (dev: 1.3 MB an hour in total, almost
all nginx access lines), so the question is value, not cost. **Recommendation: keep three streams, drop the
rest.**

| Keep | Why |
|---|---|
| API warnings and errors over OTLP | Trace-linked: from a 500 on a dashboard straight to its log line and trace. |
| Postgres (slow > 200 ms, lock waits, checkpoints) | Already configured. The only record of a slow query after it happened. |
| nginx access + error, **parsed** (status, method, path, request time) | The only view of what the front door refused (429, 499, 502, 504). Needs `log_format` with `$request_time` and `$upstream_response_time`. |

| Drop | How |
|---|---|
| The `api` container's stdout (duplicate of OTLP) | Add `api` to the drop stage in `config.alloy`. |
| `frontend` (Vite dev server), `portainer`, info-level app logs | Same. |

If you would rather have no logs at all: remove the Docker log source and keep only OTLP errors. You lose the
nginx and Postgres views, nothing else. Either way set Loki retention to 14 days for access logs.

### Phase 4 — Dashboards as code (M, ongoing) — see section 3.

### Phase 5 — Alerts (S)
Few, symptom-based, provisioned as files. Proposed starting set (thresholds to agree):

1. API down: no successful scrape or health failure for 2 minutes.
2. 5xx rate above 1 % of requests for 5 minutes.
3. p95 latency above 1 s for 10 minutes.
4. Host disk above 80 %, or predicted full within 24 h.
5. A container restarted, or was OOM-killed.
6. Postgres connections above 80 % of the maximum, or the Npgsql pool has waiters.
7. Login failures spike (credential stuffing): failures above 5× the 7-day norm.
8. No backup in 26 hours (needs a small metric from the backup job; see section 3, last row).

Contact point: your choice (email, Telegram). Add an **external uptime check** (a free third-party probe on
the public URL) because a dead host cannot alert about itself.

### Phase 6 — Before production
- **Grafana access:** SSH tunnel (simplest) or a subdomain behind TLS and a login. Never publish 3000.
- **Topology decision** (open question 1): if the app and observability run on separate servers, Alloy runs
  on the app host and writes over a private link (WireGuard or a firewalled private network); Loki, Tempo
  and Prometheus then accept writes only from that address.
- **Secrets:** admin password via the same mechanism as the app's secrets; rotate the default.
- **Backups:** dashboards live in git, so only the Grafana database (users, preferences) needs a copy. Loki
  and Prometheus are disposable.

---

## 3. Dashboards

### How they are built
- **Dashboards as code.** JSON files in `infra/observability/grafana/dashboards/`, loaded by a provider file
  in `provisioning/dashboards/`. The repo is the source of truth (`allowUiUpdates: false`): build in the UI,
  export the JSON, commit. Datasource uids are already fixed (`prometheus`, `loki`, `tempo`).
- **Shared variables:** `$container` (one variable drives metrics and logs), `$host`, `$congregation`.
- **Each dashboard answers one question** and starts with a row of stat tiles, then trends, then detail.
  Panels link to the next level down (overview → API → trace).
- Folders: **Echo** (the product), **Platform** (containers, host, database).

### The set

Data is shown as *available now* (the metric already exists) or *needs Phase N*.

| # | Dashboard | Question it answers | Key panels | Data |
|---|---|---|---|---|
| 1 | **Overview** | Is Echo healthy right now? | Up/down, requests/s, error %, p95, active requests; DB connections; host disk and memory; restarts; the latest errors from Loki with trace links | Partly now; DB/host after P1–2 |
| 2 | **API** | Where is the time and where are the errors? | Latency p50/p95/p99 by route; status classes; active requests; exceptions; **rate-limit rejections** (`aspnetcore_rate_limiting_requests_total`); GC pause, heap, thread-pool queue, working set | **Now** |
| 3 | **Tenants and product** | Who is using it, and is anything wrong for one church? | Requests per congregation (top N); logins, registrations, invitations, password resets, verifications; active congregations per day | **Now** (`congregation_id` label is on the counters) |
| 4 | **Auth and abuse** | Is someone attacking sign-in? | Login success vs failure by result; refresh and revoke rates; 429s at the edge and in the app | **Now** (edge part after P3) |
| 5 | **Containers and host** | Is a machine or container running out of something? | CPU, memory vs limit, network and disk per container; restarts and OOM; host CPU, load, memory, disk fill and forecast | Needs **Phase 1** |
| 6 | **Database** | Is Postgres coping, and what is slow? | Connections vs max, cache hit ratio, transactions, locks, size growth, checkpoints; **Npgsql pool** (open, idle, waits); slow-statement log; **top queries table** from `pg_stat_statements` | Needs **Phase 2** |
| 7 | **Edge (nginx)** | What did the front door refuse or slow down? | Status classes over time; 429 / 499 / 502 / 504; slowest paths by request time; upstream time vs total | Needs **Phase 3** |
| 8 | **Cache and jobs** | Are the summary cache and the backups working? | Summary cache hit ratio; backup age and size | Needs the Phase 2 counter; backup age needs the backup script to write one metric file |

### Build order
1. **Now, in parallel with Phase 1:** dashboards 2, 3 and 4 — the data already exists and they exercise the
   whole provisioning path.
2. After Phase 1: 5, then the host half of 1.
3. After Phase 2: 6 and 8, then finish 1.
4. After Phase 3: 7.

### Cardinality rules
- `congregation_id` is a label on the auth and volume counters. Fine up to a few hundred churches. Always
  aggregate with `topk(10, …)` on panels; never graph every tenant. Show names by joining in the dashboard,
  not by adding a name label.
- Never put a user id, email, member id or raw path with ids into a metric label. Route templates only.
- The cAdvisor allow-list (Phase 1) is what keeps the container series count bounded.

---

## 4. Decisions

| # | Question | Answer (2026-10-30) |
|---|---|---|
| 1 | One host or two? | **Two servers**: one for observability, one for the app with Alloy. Not bought yet; everything is prepared for it (section 2, Phase 6) |
| 2 | Logs | **The narrow set**: API warnings and errors, Postgres slow statements, nginx access and error |
| 3 | Alert channel | **Later.** Alerts wait until everything is set up and working, and the load test has shown what normal is |
| 4 | Grafana in production | **Its own subdomain with HTTPS and a login per developer**, no SSH |
| 5 | Targets | **Placeholders** (5xx under 1 %, p95 under 500 ms, disk under 80 %) until the load test |

**Next, after the servers exist:** run the full setup there, then a **k6 load test** to find concurrent users and
latency. Prometheus already accepts remote write (`--web.enable-remote-write-receiver`), so k6 can send its own
metrics (`-o experimental-prometheus-rw`) and a "Load test" dashboard can sit next to the others. Real targets and
the alerts come from that run.

## 5. Deliberately not planned

- Tempo's metrics generator (span metrics, service graph): ASP.NET already exports request metrics, and there
  is one service.
- Continuous profiling, frontend (browser) telemetry, a long-term metrics store: revisit with real traffic.
- Standalone cAdvisor and node-exporter containers: Alloy covers both.
