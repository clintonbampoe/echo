# Echo — Performance & Delivery Plan

> **Living plan.** Any agent or human picking this up: read the whole file, then start at
> **Move 0**. Each move is independently shippable and independently valuable — stop after
> any of them and the project is better off than before it.
>
> Companion files:
> - `docs/performance/log.md` — the measurement log (the ritual, below)
> - `docs/infrastructure/index.md` — how the stack fits together
> - `docs/frontend/architecture.md` — frontend rules
>
> Last updated: 2026-10-21.

---

## 1. Context

Multi-tenant church management platform. ASP.NET Core 10 + PostgreSQL 18, React SPA, single
2 vCore / 4 GB Hetzner box for API + DB + nginx. Observability and the load generator live on
a second box.

**Target customer:** many small churches, multiple users each. The design ceiling is a
~20k-member congregation; that is a marketing number, not the average case. Multi-tenancy is
the sharding key, so per-query cost scales with **one tenant's** rows, not the database's.

**The load shape that follows:** capacity scales by *adding tenants*. The wall is concurrent
users **within a single tenant** — a 500-member church with 20 staff all opening the dashboard
at 9am on a Sunday. "5k concurrent users" is a sales number, not a measurement.

**The ceiling, concretely:** 20k members ≈ 3.2M attendance rows on one tenant. That is what
the free-tier promise has to survive. `BigMembers`' range ceiling of 500,000 is aspirational
over-engineering and should be trimmed to match the promise.

## 2. "Done" is three different things

| Level | Means | Cost |
|---|---|---|
| **Shippable** | A real church can use it without hand-holding | days of fiddly work |
| **Defensible** | It holds under load, and you can *prove* it | weeks of measurement |
| **Publishable** | You can put your name on it and show your working | days of writing |

Shippable is close once the backend is fast. Defensible is the bulk. Publishable should be
*accumulated as you go*, not scheduled.

## 3. The four walls

### Wall 1 — Correctness & speed
*Moves 0–5.* Make the server do less work, in the right order: make it talk → measure → stop
asking → do work once → fix the data model → stop repeating.

**Exit criteria**
- One dashboard paint measured, and fast
- Every summary is one scan, not N
- The schema can express the queries the dashboard asks
- Integration tests exist and assert the numbers
- A cache in front of the summaries

*Surprise risk:* Move 0 may halve the slowest query on its own (stale statistics +
`work_mem=4MB`), reordering everything after it. That is the plan bending to evidence.

### Wall 2 — Capacity
*Move 6, plus the tuning that comes after it.*

Tuning and rate limits come **after** the load test, not before. Tune from evidence.
Load test first; then Docker limits, connection pool, PgBouncer, rate limits, GC, nginx.

**Exit criteria**
- A number: "sustains X rps at Y p95 on the target box"
- The binding layer identified (app / edge / database)
- A repeatable load test script anyone can run
- All limits sized from the measurement

*Note:* a real hostname + TLS is needed **before** the integrate test (real SNI, real cert
chain, real browser). Do the domain brainstorm at the Wall 2 → Wall 3 boundary.

### Wall 3 — Production
*Domain, TLS, CI/CD, deploys, observability, backups, real congregations.*

**Exit criteria**
- Deploys are one command and repeatable
- You find out when something breaks (the custom OTel stack rebuild lives here)
- Backups are **restored** at least once, not just taken
- A few real congregations are on it and instrumented

*Order:* domain + TLS → deploy strategy → CI/CD → observability coordination (API on server 1,
obs + load generator on server 2) → frontend hosting decision → real users.

The frontend hosting decision comes last here, and interacts with rate limiting: if Cloudflare
fronts it, the edge IP becomes your client IP and an IP-keyed limiter collapses into one
bucket.

### Wall 4 — Product & presence
*UI polish, frontend cleanup, landing page, the deck, the public log.*

**Exit criteria**
- The visual language is consistent and is what a treasurer should be looking at
- The landing page converts
- The motion design deck exists
- The public log is running with a few entries

**This wall is parallel.** Nothing in Walls 1–3 depends on it.

## 4. The ladder

Work reduction has a strict order. Exhaust a rung before climbing.

| Rung | Move | Multiplier |
|------|------|------------|
| 1 | Don't do the work | requests × 40 → × 1 |
| 2 | Do the work once, not N times | scans per request × 4–5 → × 1 |
| 3 | Do less of the work | data scanned, data shaped |
| 4 | Make what's left cheap | indexes, memory, hardware |

Rung 4 is last. It is also where everyone starts, and it is the least leveraged.

---

## 5. The moves

### Move 0 — Make the box talk
*What:* turn on Postgres's own instrumentation (slow-query log, query statistics, I/O
timing). Run `ANALYZE` after a seed — the seeder's bulk COPY does not update planner
statistics, so the baseline may be measuring plans Postgres would never choose. Stop the API
logging query parameters outside development.
*Done when:* the database returns a ranked list of its slowest queries; the slow-query log
is non-empty; `ANALYZE` reports what it updated.
*Teaches:* you cannot optimise what the machine will not tell you. Your baseline may be a lie.

### Move 1 — Get the real number, once, repeatably
*What:* a small `tools/measure/` script that opens the dashboard, counts its HTTP requests,
times each, and totals the database work they caused.
*Done when:* one line prints — "one dashboard paint = N requests, X s wall, Y s of DB work".
*Teaches:* the endpoint number and the user-visible number are different numbers. The second
is the one being optimised. This harness is reused by every later move.

### Move 2 — Stop asking for things you already know
*What:* give the asset summary the category breakdown it lacks (≈288 requests → 1). Add the
monthly/series endpoints that don't exist; point the frontend at the two that already exist
and are ignored. One endpoint for the whole screen.
*Done when:* request count per paint collapses to single digits. Wall time drops with the
queries unchanged.
*Teaches:* the cheapest query is the one you never send. When an API lacks an aggregate, the
client synthesises an N+1, and it is always worse than the aggregate you should have written.

### Move 3 — Do the work once, not N times
*What:* rewrite the slow summaries into the shape the one fast summary already uses
(grouped pass + filtered aggregates). Turn the two correlated subqueries (first-time
visitors, recurring visitors) into single grouped passes.
*Done when:* the attendance summary drops from ~1.5–2.4 s to tens of ms; scans per call go
4–5 → 1.
*Teaches:* round trips and work are different axes. Optimising the wrong one feels like
progress. The house pattern produced "one round trip, N scans".

### Move 4 — Fix the data model so the query layer can exist
*What:* the hot table has no date column (the date lives in another table behind a join);
one boolean is stored as text; a month is stored as text so it cannot be compared; two date
filters are written as functions of the column. No index can rescue these. Denormalise the
date, make the boolean a boolean, make the month a number/date, add month buckets — *then*
add the indexes those queries need.
*Done when:* hot query plans flip from seq scan + hash aggregate to index scans; p95 improves
without touching the C#.
*Teaches:* the data model is the ceiling on the query layer. First move needing a migration
on a live table — give it its own careful step.

### Move 5 — Don't do it twice
*What:* short-lived in-process cache keyed by tenant + period + filters. Working set is
tiny (21 congregations × a few periods).
*Done when:* request rate at the API drops an order of magnitude; p95 under load improves
dramatically.
*Teaches:* the cheapest work is work you don't repeat. Dashboard load becomes roughly
independent of how many people are looking.

### Move 6 — Prove it with a load test shaped like reality
*What:* a k6 harness with an arrival-rate model and explicit thresholds. Three scenarios:
many small tenants (platform limits), one large tenant (query limits), one tenant
misbehaving (fairness — this is what justifies re-keying rate limits off IP). Run against
the API directly first (isolate), then through nginx (integrate). On target-shaped hardware.
*Done when:* "sustains X rps at Y p95 on the target box", and the binding layer is known.
*Teaches:* a closed-model load test hides queueing. Never finish unable to answer "which
layer is the constraint?"

### Move 7 — Ship it to real congregations
*What:* 3–5 real congregations. Instrument, watch, let observed traffic set the target.
*Done when:* a real congregation's load is predictable from the model.
*Teaches:* a benchmark tells you about your benchmark.

---

## 6. The seeder has two jobs

- **Bug profile** (current shape: one large tenant + smalls) — good at exposing query bugs.
  Use for Moves 0–5. Do not change it yet.
- **Load profile** (many small tenants, realistic distribution) — required for Move 6, because
  the load test must reflect "myriads of small churches with multiple users".

The `SmallCount` cap of 20 and the ~30 name combos will need to grow for the load profile.
Total row count barely changes either way (~16M across 200 small churches vs ~5.7M now) —
**what changes is the distribution**, and that is only needed for the load test.

## 7. The ritual

After every move, append an entry to `docs/Performance-log.md`:

1. **Hypothesis** — what we think will happen
2. **Before** — the number
3. **After** — the number
4. **Learned** — the principle, in one sentence
5. **Surprise** — what it changed our mind about

This is the engineering log, the content pipeline, and the guard against repeating a mistake.

## 8. Deliberately deferred

| Deferred | Why |
|---|---|
| Custom observability stack rebuild | Existing OTel traces are enough for Moves 0–4. It is a tooling project; it competes with the product. |
| Frontend reorganisation | Zero effect on API performance. Do it for velocity. |
| CI/CD + deploy strategy | After domain + TLS. Fix the CI path filter soon — infra changes do not trigger it today. |
| Motion design deck | After there is a story worth animating. |
| Index tuning, Postgres config | Move 4. Not before — you would be indexing the wrong queries. |
| Rate-limit re-keying | Move 6. The load test should tell you what the limits are. |
| Hierarchy (denominations over churches) | Not today. Revisit after Move 7. |

## 9. Parallel tracks

Nothing in Walls 1–3 depends on the visual polish. The Claude Code quota blocks Wall 4, which
is not on the critical path.

- **Track A (critical path):** Moves 0–7. Backend.
- **Track B (available now, no quota needed):** frontend cleanup (velocity, not performance)
  and the domain name brainstorm (cheap, unblocks Wall 3).
- **Track C (available now, no quota needed):** the seeder's load profile (needed by Move 6),
  and the first public log entries (writable now from Moves 0–3).

## 10. Where it can surprise you

1. **Wall 1 may finish faster than expected** — the query rewrite is pattern-matching, not
   invention. The pattern is already in the repo.
2. **Wall 2 may not be the wall you think** — with many small tenants and a cache, the wall
   may be Postgres connection count rather than query speed. A two-day fix, not two weeks.
3. **Wall 3 may be the real work** — everything before it is engineering you control.
   Deploying, monitoring, backing up and supporting real churches is a different skill set.
   Budget for it honestly.

## 11. Where the original ten-step plan landed

| Original step | Lands in |
|---|---|
| 1. Finish + standardize the UI | Wall 4 |
| 2. Optimize DB queries | Wall 1 (Moves 3–4) |
| 3. Replace the OTel image | Wall 3 |
| 4. Load test locally at 1k | Wall 2 |
| 5. Clean up the frontend | Wall 4 |
| 6. Tune Docker + the C# API | Wall 2 — **after** the load test |
| 7. Rate limits: nginx + JWT | Wall 2 — **after** the load test |
| 8. Load test 2k–5k | Wall 2 |
| 9. Domain, Resend, TLS | Wall 3 — **before** the integrate test |
| 10. Frontend hosting | Wall 3, last |
| 11. Coordinate API + obs across servers | Wall 3 |
| 12. CI/CD | Wall 3 |
| 13. Publish the methodology | Wall 4, ongoing |

**Two reorderings matter:** 6 & 7 now come *after* 8 (tune from evidence), and 9 moves
*earlier* (a real hostname is needed to test realistically).

## 12. Open questions

- **Where are the users?** → gates Move 6 (p95 budget, region) and the domain choice.
- **Hierarchy (denominations over churches)?** → not today. Revisit after Move 7.

## 13. Known landmines — parked, do not act on these yet

- nginx: `events {}` empty → 1 worker / ~1024 connections; no upstream keepalive;
  `proxy_http_version` unset → HTTP/1.0 to the API. Effective ceiling ≈512 in-flight requests.
- Rate limiting: nginx keyed on IP; the .NET `auth` policy is 5/min **per IP** and covers
  refresh-token rotation, so an office behind one NAT IP locks itself out. The global .NET
  limit (~1.7 r/s per IP) is 15× stricter than nginx's.
- Postgres: no `postgresql.conf` anywhere. Inherits `shared_buffers=128MB`, `work_mem=4MB`,
  `random_page_cost=4`, `log_min_duration_statement=-1`, `track_io_timing=off`.
- API: `EnableDetailedErrors` + `EnableSensitiveDataLogging` on in production.
- OTel: no sampler → 100% trace sampling, with `AddNpgsql()` and OTel log export on.
- Docker: no `mem_limit`/`cpus`/`ulimit` anywhere.
- API Dockerfile: no `USER` (runs as root), no GC configuration pinned.
- CI: `paths:` filter means `infra/**` and `docker-compose.yml` changes do not trigger CI.
  No integration tests exist (`Echo.IntegrationTests` is a stub, not in the solution).
- Compose: `docker-compose.override.yml` bind-mounts absolute host paths (`/build/src/...`)
  — the dev hot-reload overlay is broken. `db_dumps/` vs `dumps/` path mismatch. The backup
  service's per-environment entrypoint is referenced but defined nowhere.
- 14 redundant prefix indexes (write amplification on the hot table).
- `DesignTimeDbContextFactory` reads connection string `"Default"`; runtime uses `"echodb"`,
  so `dotnet ef` is broken design-time.

## 14. Current state

**Next action: Move 0.** Postgres `conf.d` mount (slow-query log, query statistics, I/O
timing) + `ANALYZE` in the seeder + the `IsDevelopment()` gate on the two EF logging flags.
Half a day, no risk, and it makes Move 1 possible.

*Open items: branch name (suggested `perf/dashboard-query-layer` off `main`); whether the
database is still seeded.*
