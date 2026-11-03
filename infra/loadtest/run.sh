#!/usr/bin/env bash
# Runs the load test and sends the results to Prometheus (so they sit next to the API, container and
# Postgres panels in Grafana) and to k6's own live dashboard at http://localhost:5665.
#
#   ./run.sh <label>          label names the run: before-gc, after-gc, ...
#   PEAK_VUS=200 RAMP_MINUTES=10 ./run.sh quick-look
#   SMOKE=1 ./run.sh smoke    three minutes, 5 + 10 people: checks the scripts work
#   STRESS=1 ./run.sh stress  people barely pause between screens: finds where latency bends
set -euo pipefail
cd "$(dirname "$0")"

label=${1:?usage: ./run.sh <label>}
sha=$(git rev-parse --short HEAD)
export TESTID="${label}-${sha}"

# REPEAT=3 ./run.sh before-x: the same run three times, one after the other, and the spread of the numbers. One run does
# not say whether a 9 ms and a 12 ms are different or just noise.
if [ "${REPEAT:-1}" -gt 1 ]; then
  for i in $(seq 1 "$REPEAT"); do REPEAT=1 "$0" "${label}-r${i}" || true; done
  printf '\n%-34s %10s %10s %10s\n' run "data p95" "data p99" failed
  for i in $(seq 1 "$REPEAT"); do
    jq -r --arg id "${label}-r${i}-${sha}" '.metrics | "\($id | .[0:34] | . + (" " * (34 - length))) \(.["http_req_duration{kind:data,phase:steady}"]["p(95)"] | floor | tostring + " ms" | (" " * (10 - length)) + .) \(.["http_req_duration{kind:data,phase:steady}"]["p(99)"] | floor | tostring + " ms" | (" " * (10 - length)) + .) \((.http_req_failed.value * 100 * 100 | floor) / 100 | tostring + " %" | (" " * (10 - length)) + .)"' "results/${label}-r${i}-${sha}.json" 2>/dev/null || echo "${label}-r${i}: no summary"
  done
  exit 0
fi

if [ "${SMOKE:-}" = 1 ]; then
  export FOCUS_VUS=5 PEAK_VUS=15 RAMP_MINUTES=3 FOCUS_ARRIVAL_MINUTES=1 THINK_MIN=1 THINK_MAX=2
fi

if [ "${STRESS:-}" = 1 ]; then
  export THINK_MIN=0.5 THINK_MAX=2
fi

mkdir -p results

# The test goes through nginx and needs it to trust the addresses k6 sends (see nginx-realip.conf).
if ! docker exec nginx test -f /etc/nginx/extra/realip.conf 2>/dev/null; then
  echo "nginx is not running with infra/loadtest/docker-compose.loadtest.yml, so it would count every request against" >&2
  echo "one address and refuse most of them. Start the stack the way the test needs it, then run again:" >&2
  echo "  infra/loadtest/up.sh" >&2
  exit 1
fi

# The production box has 2 vCPUs for everything on it. The app containers are pinned to two cores by the compose file
# above (api, database, nginx); Alloy runs on that box too, so it is pinned here, and k6 is kept on the other cores so it
# does not take CPU from them. Change the cores with APP_CPUS and K6_CPUS.
APP_CPUS=${APP_CPUS:-0,1}
if [ "$(docker inspect -f '{{.HostConfig.CpusetCpus}}' api 2>/dev/null)" != "$APP_CPUS" ]; then
  echo "The api is not pinned to cores $APP_CPUS, so it would have the whole machine and not the production box's 2 vCPUs." >&2
  echo "Start the stack the way the test needs it (infra/loadtest/up.sh), or pin it now:" >&2
  echo "  docker update --cpuset-cpus $APP_CPUS api database nginx" >&2
  exit 1
fi
docker update --cpuset-cpus "$APP_CPUS" alloy >/dev/null 2>&1 || echo "(alloy is not running, so it is not pinned)"
# k6 and the monitoring stack get every logical CPU that is not on the app's physical cores. A CPU's hyperthread
# sibling shares the core's execution units, so k6 running on the sibling of an app core would slow the app down.
topology=$(lscpu -p=CPU,CORE | grep -v '^#')
app_cores=$(echo "$topology" | awk -F, -v apps=",$APP_CPUS," 'index(apps, "," $1 ",") {print $2}' | tr '\n' ' ')
K6_CPUS=${K6_CPUS:-$(echo "$topology" | awk -F, -v cores="$app_cores" 'BEGIN { n = split(cores, c, " "); for (i = 1; i <= n; i++) skip[c[i]] = 1 } !($2 in skip) { printf "%s%s", sep, $1; sep = "," }')}
pin_k6=()
if [ -n "$K6_CPUS" ]; then
  pin_k6=(taskset -c "$K6_CPUS")
  # The monitoring stack is on its own server in production, so it is kept off the app's cores as well. Unpinned,
  # Grafana, Loki, Prometheus and Tempo can run on them and take CPU from the app.
  docker update --cpuset-cpus "$K6_CPUS" grafana loki prometheus tempo >/dev/null 2>&1 || true
fi
echo "app on cpus $APP_CPUS, k6 and monitoring on cpus ${K6_CPUS:-all (no spare cores)}"

# Postgres keeps per-statement totals since they were last reset; start from zero so the run is all it shows.
sql() { docker exec database psql -U postgres -d echo_db -c "$1" 2>/dev/null; }
sql "SELECT pg_stat_statements_reset()" > /dev/null || echo "(could not reset pg_stat_statements; SQL report skipped)"
export K6_PROMETHEUS_RW_SERVER_URL=${K6_PROMETHEUS_RW_SERVER_URL:-http://localhost:9090/api/v1/write}
export K6_PROMETHEUS_RW_TREND_STATS='avg,p(50),p(90),p(95),p(99),max'
export K6_PROMETHEUS_RW_PUSH_INTERVAL=5s
export K6_PROMETHEUS_RW_TREND_AS_NATIVE_HISTOGRAM=true
export K6_WEB_DASHBOARD=true
export K6_WEB_DASHBOARD_EXPORT="results/${TESTID}.html"

echo "testid=$TESTID"
# k6 exits non-zero when a threshold is crossed, which is exactly when the SQL report is most wanted.
status=0
started=$(date +%s)
"${pin_k6[@]}" k6 run -o experimental-prometheus-rw --summary-export "results/${TESTID}.json" main.js || status=$?

# The run queue: the average number of tasks waiting for each app core, the highest it reached during the run. It must stay
# under 1 on both cores: past that, requests wait for a core and latency spikes whatever else is healthy. Needs the
# pressure collectors in Alloy (set_collectors) and Prometheus on localhost:9090.
queue=$(curl -sS -m 20 -G "${PROMETHEUS_URL:-http://localhost:9090}/api/v1/query" \
  --data-urlencode "query=max by (cpu) (max_over_time(rate(node_schedstat_waiting_seconds_total{cpu=~\"${APP_CPUS//,/|}\"}[30s])[$(( $(date +%s) - started + 30 ))s:15s]))" 2>/dev/null \
  | jq -r '[.data.result[] | "core\(.metric.cpu)=\((.value[1] | tonumber * 100 | floor) / 100)"] | join(" ")' 2>/dev/null) || queue=""
queue_over=$(echo "$queue" | awk '{ for (i = 1; i <= NF; i++) { split($i, kv, "="); if (kv[2] + 0 >= 1) over = 1 } } END { print over + 0 }')
echo "run queue (tasks waiting per app core, highest; under 1): ${queue:-unavailable}$([ "$queue_over" = 1 ] && echo '   <-- OVER')"
[ "$queue_over" = 1 ] && status=1

# The verdict, against the targets: data p95 10 ms (15 ms at the very most), sign-in p95 under 500 ms, renewals under 50 ms.
if [ -f "results/${TESTID}.json" ]; then
  phase=steady; [ "${HOLD_MINUTES:-7}" -gt 0 ] && phase=hold
  jq -r --arg phase "$phase" '.metrics as $m |
    def ms(k; q): ($m[k][q] // null) | if . == null then "-" else (. * 10 | floor / 10 | tostring) end;
    "\nverdict (\($phase) phase)\n  data p95     \(ms("http_req_duration{kind:data,phase:" + $phase + "}"; "p(95)")) ms   (target 10, at most 15)\n  data p99     \(ms("http_req_duration{kind:data,phase:" + $phase + "}"; "p(99)")) ms\n  sign-in p95  \(ms("http_req_duration{name:POST /auth/sessions/login}"; "p(95)")) ms   (under 500)\n  renewal p95  \(ms("http_req_duration{name:POST /auth/sessions/refresh}"; "p(95)")) ms   (under 50)\n  failed       \((($m.http_req_failed.value // 0) * 10000 | floor) / 100) %"' "results/${TESTID}.json"
fi

# The statements that took the most time during the run.
sql "SELECT calls, round(total_exec_time) AS total_ms, round(mean_exec_time::numeric, 1) AS mean_ms,
            round(max_exec_time) AS max_ms, left(regexp_replace(query, '\\s+', ' ', 'g'), 160) AS query
     FROM pg_stat_statements WHERE query ILIKE 'select%' ORDER BY total_exec_time DESC LIMIT 15" \
  > "results/${TESTID}-sql.txt" || true
echo "slowest statements: results/${TESTID}-sql.txt"

# The most memory each container held since it started (restart them before a run for a clean reading). These are
# the numbers the production memory limits in docker-compose.prod.yml are sized against.
for c in api database nginx; do
  peak=$(docker exec "$c" cat /sys/fs/cgroup/memory.peak 2>/dev/null || echo 0)
  printf '%s %s MB\n' "$c" "$((peak / 1024 / 1024))"
done | tee "results/${TESTID}-memory.txt"
exit $status
