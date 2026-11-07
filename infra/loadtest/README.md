# Load test

k6 scripts that act like the church staff using the app, so we can measure the API before and after a change.
Local for now; the same scripts run against the real box later. The test goes **through nginx** (`BASE_URL` is the
production stack's nginx at `https://echo.localhost`, set up by `infra/nginx/local-certs.sh`), so its connection handling, rate limit, compression and access log are part of what is
measured. k6 gives each church its own address in `X-Forwarded-For`, which nginx only believes with the extra file in
this folder, so start the stack like this before a run (`run.sh` checks and says so if you forget):

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f infra/loadtest/docker-compose.loadtest.yml up -d nginx alloy loki prometheus tempo grafana
```

nginx limits each address to 50 requests a second (burst 200). A church's staff share one address and a dashboard
load asks for about twenty things at once, so a `429` in this test is a real finding about the limit, not a harness
error (at 25 requests a second and a burst of 50 the 500-user test refused 0.28% of requests).

## The load

- **Focus tenant** (Grace Chapel International): 20 staff, arriving over the first minute, the way an office signs in on a Sunday morning and staying for the whole
  run, using all of that church's seeded accounts.
- **Background**: people from the other churches arrive slowly, 20% of them in the first 10% of the run and the rest
  up to `PEAK_VUS` (500, the focus staff included) by 90% of it, then hold. Defaults: 15 minutes.
- **Every virtual user is one person with a real session.** They sign in with their password (bcrypt), renew with the
  refresh token a minute before the access token runs out, and start over if a renewal fails. The token lifetime is
  whatever the API is set to (`Jwt:AccessTokenLifetimeMinutes`), read from each response, so a shorter lifetime
  means more renewals.
- **A church shares one address.** Every request carries an `X-Forwarded-For` made from the church's name, which the
  API trusts from the Docker network, so its rate limiters count a church like an office behind a router. Sign-in is
  limited to 30 a minute per address and renewal to 120: if a church is big enough to need more, the test shows it as
  `session_signin_rejected` or `session_refresh_rejected`.
- A person opens 4–7 screens, 3–10 seconds apart, then starts over. Which screens depends on the role
  (`profiles/index.js`): admin sees everything, accountant tithes and money, clerk attendance and people, member the
  read-only pages. Every screen sends the same requests as the frontend (`profiles/pages.js`), the dashboard as one
  burst of about twenty.
- Date ranges are the dashboard's six periods (this week, last week, this month, last 3 and 6 months, this year), all
  ending on the reference date, with "this week" the most common. Each person keeps theirs, as the picker remembers
  it in the browser, and sometimes changes it. People at one church who pick the same period share a cached answer.
- Searching types a real member's name at 2, 4 and 6 letters, like the type-ahead box.
- Reads only. Writes would need valid payloads and would grow the database during the run.
- The first minute is the API warming up. Its requests carry `phase="warmup"` (the rest `phase="steady"`), so runs
  can be compared on the steady part.

## The target

The target server has 4 vCPU and 8 GB, with the monitoring stack on the same machine. The test runs the whole stack on
4 CPUs of this machine (see `docker-compose.loadtest.yml`) and the CPU weights decide who wins when they are busy.

The thresholds in `main.js` are **500 people signed in, with the app's data requests at p95 under 25 ms and p99 under
50 ms** (steady phase, so the warm-up minute is left out), no failures, and no sign-in or renewal turned away. They were
set on the earlier 2 vCPU and 4 GB box: the first runs on this setup are the new baseline, and the bar is raised from
there. k6 exits non-zero and `run.sh` still writes the SQL report when one is missed. Sign-in (bcrypt, about 200 ms by
design) is judged separately, at p95 under 1 s.

## Run it

```bash
./prepare.sh                 # once, and after re-seeding: writes the seeded accounts to .accounts.json
SMOKE=1 ./run.sh smoke       # 3 minutes, 5 + 15 people: do the scripts work?
./run.sh before-gc           # the real run: 15 minutes, up to 500 people
PEAK_VUS=200 RAMP_MINUTES=10 ./run.sh quick-look
STRESS=1 ./run.sh stress     # barely any pause between screens: finds where latency bends
```

500 people on one machine that also runs the API, Postgres and Grafana will make k6 itself compete for CPU: watch
the host panel, and treat the first big run as a way to find out whether the machine is the limit.

| Variable | Default | |
|---|---|---|
| `BASE_URL` | `https://echo.localhost/api/v1` | the production stack's nginx. `http://localhost:5025/api/v1` is the API itself |
| `TRACE_RATIO` | `0.01` | share of requests that ask the API to record a trace (it traces 1% by default) |
| `PEAK_VUS` | 500 | people signed in at the end of the ramp, focus staff included |
| `RAMP_MINUTES` | 15 | length of the run |
| `FOCUS_VUS` / `FOCUS_ARRIVAL_MINUTES` | 20 / 1 | staff in the focus tenant, and how long they take to arrive |
| `THINK_MIN` / `THINK_MAX` | 3 / 10 | seconds between screens |
| `RENEW_BEFORE_SECONDS` | 60 | how long before expiry a session is renewed |
| `REF_DATE` | 2026-10-20 | the seeder's "today" |

## Watching it

| Where | What |
|---|---|
| `http://localhost:5665` | k6's live dashboard while the run is going. The report is saved to `results/<testid>.html` |
| Grafana, **Load test (k6)** | k6's numbers next to the API, container and Postgres dashboards, same time axis. Pick the run by `testid` |
| Grafana, **Echo / API**, **Platform / Database** | What the server did during the run |
| `results/<testid>-sql.txt` | The 15 SELECTs that took the most time during the run. `pg_stat_statements` is reset at the start of `run.sh`, so it is only this run |
| Tempo | Every request carries a `traceparent`. A failed request prints its trace id in the k6 output: search it in Grafana → Explore → Tempo to see the endpoint, the EF/Npgsql spans and the SQL |

Durations arrive as native histograms (`k6_http_req_duration_seconds`), so any quantile over any route and stretch of
time is exact: `histogram_quantile(0.95, sum by (name) (rate(k6_http_req_duration_seconds{testid="..."}[5m])))`.
Prometheus runs with `--enable-feature=native-histograms` for this. The older `k6_http_req_duration_p95` series are a
p95 per five seconds: averaging them overstates routes that are hit only a few times in each window.

Every request is tagged `name` (the route, `GET /members/{id}`), `profile` (the role) and `tenant_kind`
(`focus` or `background`), and every series has the `testid` of the run (`<label>-<git sha>`).

## Before and after

1. Run `./run.sh before-<change>` on the unchanged code and note the testid.
2. Make one change, restart the API, run `./run.sh after-<change>`.
3. In Grafana, compare the two testids. `results/*.json` hold the summaries for a quick diff.

Change one thing at a time, and let the API warm up (the first minute is the warm-up of the ramp). A run is only
comparable with another if the data, the machine and the load are the same.
