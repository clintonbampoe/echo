# Run a load test

The k6 scripts in `infra/loadtest/` act like church staff using the app, so
you can measure the API before and after a change. The test runs on your
machine, through the production nginx, with the whole stack pinned to 4 CPUs
to stand in for the 4 vCPU server.

`infra/loadtest/README.md` describes the scenarios and every setting. This page
is the short path to a first result.

## Before you start

- Linux, with `k6`, `taskset` and `mkcert` installed, and the power profile set
  to `balanced` (the run refuses otherwise, because the profile changes what a
  hash costs; `POWER_PROFILE_OK=1` overrides).
- A trusted local certificate: `infra/nginx/local-certs.sh`.
- A seeded database (the seeder, see `backend/src/Echo.Seeder/Seed.md`).
- `DB_USERNAME=postgres` in `.env`: the scripts open `psql` as `postgres` inside
  the database container.
- No other load on the machine. Do not start, stop or log in to anything while
  a run is going; it changes the numbers.

## Run

From `infra/loadtest/`:

```bash
ACCOUNTS_PER_CHURCH=110 ./add-accounts.sh   # once: one account per simulated person
./prepare.sh                                 # writes .accounts.json
./up.sh                                      # the production stack, pinned to 4 CPUs
SMOKE=1 ./run.sh smoke                       # 4 minutes: do the scripts work?
PATTERN=stepped ./run.sh first-look          # the real run
```

The stepped pattern raises the number of visits a second in steps and judges
each step on its own. A visit is one person opening 4 to 7 screens, about 45
seconds and 25 requests. Defaults: 16, 22, 28, 34, 40, 46 and 52 visits a
second, each ramping 120 s then holding 120 s. Change them with
`STEP_RATES=8,12,16`.

`up.sh` recreates the whole stack, so never run it during a test. It pins every
container to `APP_CPUS` (default `0,1,4,5`: two physical cores with both
hyperthreads on an i7-1185G7) and leaves the other cores to k6. The CPU
weights in `docker-compose.prod.yml` decide who wins when those CPUs are busy,
as they do on the server.

## Read the result

`run.sh` ends with a verdict per step, judged on the hold part of the step:

```text
  visits/s  data p95  data p99  sign-in p95  renewal p95  failed %  run queue
        34      18.8      30.2         80.1            -         0        0.6  ok
```

A step passes when data requests stay under p95 25 ms and p99 50 ms. The run
stops by itself when a step collapses. Files land in `results/` (not
committed), named `<label>-<commit>`:

| File | Holds |
|---|---|
| `.json` | the k6 summary: thresholds (`true` means **failed**), percentiles |
| `.html` | k6's own report |
| `-env.txt` | machine and API settings; two runs are only comparable if these match |
| `-sql.txt` | the slowest statements from `pg_stat_statements` |
| `-memory.txt` | container memory at the end |

While it runs, the `k6 load test` and `Platform / Containers` dashboards in
Grafana (`http://localhost:3000`) show the load and the stack's response.

## Compare two runs

Change one thing between runs, such as `API_GC_SERVER=1 ./up.sh` for Server GC.
One run is one sample. `REPEAT=3 PATTERN=stepped ./run.sh label` runs it three
times and prints each run's numbers, so you can see how much runs differ from
each other before trusting a difference.

## What the numbers mean

They describe this laptop with this stack, not the production server. Use them
to compare a change against a baseline taken the same way, and read
[Limits](../explanation/limits.md) for the figures we hold today.
