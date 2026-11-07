# Limits

What this setup can carry and where it stops. Numbers are dated and say how
they were measured. Read this before choosing a smaller server or promising
something to a customer.

## What it can carry

Measured on 2026-11-07 on a laptop with the whole stack pinned to 4 logical CPUs
(2 physical cores with hyperthreading), to stand in for the 4 vCPU server.
One run per setting, so treat differences of a few milliseconds as noise.
[Run a load test](../how-to/run-a-load-test.md) repeats it.

| Visits a second | Requests a second | Data p95 | Data p99 | Verdict |
|---|---|---|---|---|
| 16 | about 290 | 10 ms | 16 ms | passes |
| 28 | about 640 | 15 ms | 23 ms | passes |
| 34 | about 770 | 17 ms | 25 ms | passes |
| 40 | about 920 | 21 ms | 60 ms and over | p99 over the 50 ms limit |
| 46 | about 1,050 | 21 to 26 ms | 35 to 44 ms | p95 at the limit; varies between runs |
| 52 | about 1,080 | 1,000 ms and over | 3,000 ms and over | collapses |

A visit is one person opening 4 to 7 screens: about 45 seconds and 25
requests. In people: visits a second times 45 seconds is how many are using
the app at the same moment, so 34 visits a second is about 1,500 people at
once. The limits are p95 under 25 ms and p99 under 50 ms for data requests
(p95 means 95% of requests were faster than that, p99 means 99%). The real
production server has not been load tested; these figures are for the
laptop stand-in.

In words:

- **Comfortable:** up to about 34 visits a second, or 770 requests a second.
- **The edge:** 40 to 46 visits a second. Tails get long, the API starts
  to queue.
- **The ceiling:** about 1,080 requests a second. Past it, latency goes from
  milliseconds to seconds because the CPUs are full, not because of memory, the
  connection pool or the load generator.

At the ceiling the stack uses about 2.8 cores: the API 1.6, Postgres 0.8,
nginx 0.3. Memory stays well inside the caps (the API about 300 MB of 1 GB).

Where the CPU goes, from a profile at 34 visits a second:

- Every request pays an RSA signature check on its access token (RS256). In the
  profile it was roughly 14% of the sampled request time.
- The members list queries are about 44% of database time, at 1 to 4 ms each;
  the cost is the number of calls, not any one query.
- Password hashing (Argon2id, about 40 ms each) is about 6% of API CPU at the
  load tested. It is limited by a gate, so a burst of sign-ins queues instead
  of starving the rest.
- Garbage collection is about 3% of wall time. Server GC halves the pauses but
  did not raise the ceiling.

These came from one profile, sampled by time, not measured CPU. Confirm with a
before and after run before acting on one.

## What it cannot do

- **No failover.** One server. If it is down, Echo is down until it is
  rebuilt: [set up a new server](../how-to/set-up-a-server.md), deploy a
  release and [restore the latest dump](../how-to/restore-a-backup.md). That
  recovery has not been rehearsed or timed.
- **Backups stay on the server.** The nightly dump is on the same disk as the
  database. A lost server loses both. Copying dumps elsewhere is not set up.
- **Every deploy restarts everything.** Expect a short outage while the
  containers come back; it has not been measured. The deploy waits for health
  before it calls the release good.
- **The status page cannot see the server go down.** It runs on the server.
  There are no alerts.
- **A rollback does not undo migrations.** See [Migrations](../how-to/migrations.md).

## Settings you may hit

The API's rate limits are in [Environment variables](../reference/environment.md#rate-limits),
nginx's request limit and upload cap are in [nginx](../reference/nginx.md#limits-and-logging),
and the database connection limit and memory caps are in
[Services](../reference/services.md).

A church's staff share one address. A dashboard load asks for about twenty
things at once, so a `429` from a big church is a real finding about the limits
and not a fault in the test.

## When not to use this setup

- You need zero-downtime deploys or automatic failover: use more than one
  server behind a load balancer.
- You expect more than about 770 requests a second for long stretches: add CPU
  first, then reduce the cost per request (token checks, the list queries).
- You cannot lose a day of data: copy the dumps off the server and shorten the
  interval before you rely on it.
