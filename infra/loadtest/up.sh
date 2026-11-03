#!/usr/bin/env bash
# Brings the app up the way the load test needs it: the production setup (HTTPS, memory limits) plus the load test's own
# file (nginx trusts the addresses k6 sends, the app is pinned to two cores). Then run ./run.sh <label>.
#
# Run from anywhere. It recreates api, database and nginx, so do not run it during a test.
set -euo pipefail
cd "$(dirname "$0")/../.."

export APP_DOMAIN=${APP_DOMAIN:-echo.localhost}
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f infra/loadtest/docker-compose.loadtest.yml \
  up -d --build api db nginx

# The dev stack's Vite server, if it was up: idle, but the baseline should have nothing else running.
docker stop frontend >/dev/null 2>&1 || true

echo -n "waiting for the API"
for _ in $(seq 1 40); do
  [ "$(docker inspect -f '{{.State.Health.Status}}' api 2>/dev/null)" = healthy ] && break
  echo -n .; sleep 3
done
echo
curl -fsS -o /dev/null "https://${APP_DOMAIN}/api/health/live" && echo "ready: https://${APP_DOMAIN}" \
  || { echo "https://${APP_DOMAIN} did not answer: is the certificate in place (infra/nginx/local-certs.sh)?" >&2; exit 1; }
