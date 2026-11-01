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
  echo "one address and refuse most of them. Start it like this, then run again:" >&2
  echo "  docker compose -f docker-compose.yml -f docker-compose.override.yml -f infra/loadtest/docker-compose.loadtest.yml up -d nginx" >&2
  exit 1
fi

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
k6 run -o experimental-prometheus-rw --summary-export "results/${TESTID}.json" main.js || status=$?

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
