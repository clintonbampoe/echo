#!/usr/bin/env bash
# Runs the load test and sends the results to Prometheus (so they sit next to the API, container and
# Postgres panels in Grafana) and to k6's own live dashboard at http://localhost:5665.
#
#   ./run.sh <label>          label names the run: before-gc, after-gc, ...
#   PATTERN=stepped STEP_RATES=8,12,16 ./run.sh quick-look   how the load changes: patterns/, then its own variables
#   SMOKE=1 ./run.sh smoke    four minutes, two small steps: checks the scripts work
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
    jq -r --arg id "${label}-r${i}-${sha}" '.metrics | "\($id | .[0:34] | . + (" " * (34 - length))) \(.["http_req_duration{kind:data,phase:hold}"]["p(95)"] | floor | tostring + " ms" | (" " * (10 - length)) + .) \(.["http_req_duration{kind:data,phase:hold}"]["p(99)"] | floor | tostring + " ms" | (" " * (10 - length)) + .) \((.http_req_failed.value * 100 * 100 | floor) / 100 | tostring + " %" | (" " * (10 - length)) + .)"' "results/${label}-r${i}-${sha}.json" 2>/dev/null || echo "${label}-r${i}: no summary"
  done
  exit 0
fi

if [ "${SMOKE:-}" = 1 ]; then
  export FOCUS_VUS=5 STEP_RATES=1,2 STEP_RAMP_SECONDS=60 STEP_HOLD_SECONDS=60 FOCUS_ARRIVAL_MINUTES=1 THINK_MIN=1 THINK_MAX=2
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

# The machine's power settings change what a run measures (a hash takes 160 ms in one and 200 ms in another), so every run
# records them and refuses to start in any profile but the expected one (balanced). A run compared with the one before it
# says if anything differs. POWER_PROFILE_OK=1 skips the refusal; EXPECTED_PROFILE=<name> changes the expected profile.
cpu=/sys/devices/system/cpu
val() { cat "$1" 2>/dev/null | head -1 || true; }
profile=$(val /sys/firmware/acpi/platform_profile)
env_lines="power profile (platform): ${profile:-n/a}
power profile (tuned): $(tuned-adm active 2>/dev/null | sed 's/.*: //' || true)
governor / energy preference: $(val $cpu/cpu0/cpufreq/scaling_governor) / $(val $cpu/cpu0/cpufreq/energy_performance_preference)
turbo: $([ "$(val $cpu/intel_pstate/no_turbo)" = 1 ] && echo off || echo on)
clock limit: $(val $cpu/cpu0/cpufreq/scaling_max_freq | awk '{ printf "%d MHz", $1 / 1000 }')"
mkdir -p results
echo "$env_lines" | tee "results/${TESTID}-env.txt"
if [ -f results/.last-env ] && ! diff -q results/.last-env "results/${TESTID}-env.txt" >/dev/null; then
  echo "WARNING: the machine's power settings are not the same as in the previous run:" >&2
  diff results/.last-env "results/${TESTID}-env.txt" >&2 || true
fi
cp "results/${TESTID}-env.txt" results/.last-env
if [ -n "$profile" ] && [ "$profile" != "${EXPECTED_PROFILE:-balanced}" ] && [ "${POWER_PROFILE_OK:-}" != 1 ]; then
  echo "The power profile is '$profile', not '${EXPECTED_PROFILE:-balanced}'. Switch it (or set POWER_PROFILE_OK=1) and run again." >&2
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
started=$(date +%s)
"${pin_k6[@]}" k6 run -o experimental-prometheus-rw --summary-export "results/${TESTID}.json" main.js || status=$?

# Did the CPU throttle during the run? A two-hour run on a laptop can, and it would look like the API getting slower.
# The kernel counts every time a core or the package was throttled; any increase is worth knowing about.
throttles=$(curl -sS -m 20 -G "${PROMETHEUS_URL:-http://localhost:9090}/api/v1/query" \
  --data-urlencode "query=sum(increase(node_cpu_core_throttles_total[$(( $(date +%s) - started + 30 ))s])) + sum(increase(node_cpu_package_throttles_total[$(( $(date +%s) - started + 30 ))s]))" \
  2>/dev/null | jq -r '.data.result[0].value[1] // "unavailable" | if . == "unavailable" then . else (tonumber | floor | tostring) end' 2>/dev/null) || throttles=unavailable
echo "CPU throttle events during the run: ${throttles:-unavailable}$([ "${throttles:-0}" != 0 ] && [ "${throttles:-}" != unavailable ] && echo '   <-- the clock was reduced')" | tee -a "results/${TESTID}-env.txt"

# The verdict, step by step: each step is judged on its hold, against the limits in lib/config.js (k6 says which
# thresholds each step missed). The API carries the highest step before the first one
# that fails. The run queue is the average number of tasks waiting for each app core, the highest it reached during
# the step's hold: past 1 on both cores, requests wait for a core, which is what CPU-bound looks like. Needs the pressure
# collectors in Alloy (set_collectors) and Prometheus on localhost:9090.
if [ -f "results/${TESTID}.json" ]; then
  prom=${PROMETHEUS_URL:-http://localhost:9090}
  elapsed=$(( $(date +%s) - started + 30 ))
  queue_for() {
    curl -sS -m 20 -G "$prom/api/v1/query" --data-urlencode "query=max(max_over_time((rate(node_schedstat_waiting_seconds_total{cpu=~\"${APP_CPUS//,/|}\"}[30s]) and on() (sum(changes(k6_http_reqs_total{testid=\"$TESTID\",phase=\"hold\",step=\"$1\"}[30s])) > 0))[${elapsed}s:15s]))" \
      2>/dev/null | jq -r '.data.result[0].value[1] // empty | tonumber * 100 | floor / 100' 2>/dev/null || true
  }
  queues=$(for step in $(jq -r '.metrics | keys[] | select(startswith("http_req_duration{kind:data,phase:hold,step:")) | ltrimstr("http_req_duration{kind:data,phase:hold,step:") | rtrimstr("}")' "results/${TESTID}.json"); do
    printf '%s=%s\n' "$step" "$(queue_for "$step")"
  done | jq -R -s 'split("\n") | map(select(length > 0) | split("=") | {(.[0]): (.[1] | tonumber? // null)}) | add // {}')
  queues=${queues:-"{}"}
  jq -r --argjson queues "$queues" '
    .metrics as $m
    | def num(x): (x // null) | if . == null then "-" else (. * 10 | floor / 10 | tostring) end;
      def pad(n): tostring | (" " * (n - length)) + .;
      def get(k; q): ($m[k] // {}) | if .max == 0 then null else .[q] end; # no requests of that kind in the step
      [$m | keys[] | select(startswith("http_req_duration{kind:data,phase:hold,step:")) | ltrimstr("http_req_duration{kind:data,phase:hold,step:") | rtrimstr("}")] | sort_by(tonumber) as $steps
    | ($steps | map(. as $s
        | { step: $s,
            p95: get("http_req_duration{kind:data,phase:hold,step:\($s)}"; "p(95)"),
            p99: get("http_req_duration{kind:data,phase:hold,step:\($s)}"; "p(99)"),
            login: get("http_req_duration{name:POST /auth/sessions/login,phase:hold,step:\($s)}"; "p(95)"),
            refresh: get("http_req_duration{name:POST /auth/sessions/refresh,phase:hold,step:\($s)}"; "p(95)"),
            failed: get("http_req_failed{phase:hold,step:\($s)}"; "value"),
            queue: $queues[$s] }
        | .ok = ([ "http_req_duration{kind:data,phase:hold,step:\($s)}",
                  "http_req_duration{name:POST /auth/sessions/login,phase:hold,step:\($s)}",
                  "http_req_duration{name:POST /auth/sessions/refresh,phase:hold,step:\($s)}",
                  "http_req_failed{phase:hold,step:\($s)}" ] | map(($m[.].thresholds // {}) | to_entries[].value) | any | not))) as $rows
    | "\nverdict (each step judged on its hold, against the limits in lib/config.js)",
      "  visits/s  data p95  data p99  sign-in p95  renewal p95  failed %  run queue",
      ($rows[] | "  \(.step | pad(8))  \(num(.p95) | pad(8))  \(num(.p99) | pad(8))  \(num(.login) | pad(11))  \(num(.refresh) | pad(11))  \((((.failed // 0) * 10000 | floor) / 100) | pad(8))  \(num(.queue) | pad(9))  \(if .ok then "ok" else "FAILED" end)"),
      ( ($rows | map(.ok) | index(false)) as $first
        | if $first == null then "  every step held: the API carries the top step, \($rows[-1].step) visits/s"
          elif $first == 0 then "  the first step already failed: nothing to report as capacity"
          else "  carries \($rows[$first - 1].step) visits/s; first step to fail: \($rows[$first].step)" end ),
      "  rate limited (429): \($m.rate_limited.count // 0)  \(if ($m.rate_limited.count // 0) > 0 then "<-- the run is not valid" else "" end)",
      "  dropped visits (no free person): \($m.dropped_iterations.count // 0)"' "results/${TESTID}.json"
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
