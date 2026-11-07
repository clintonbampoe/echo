#!/bin/sh
# Creates the read-only role Alloy reads database metrics with. Postgres runs it when it creates a new database;
# on an old one: docker exec database /docker-entrypoint-initdb.d/monitor-role.sh
set -eu
: "${MONITOR_PASSWORD:?}"

psql -v ON_ERROR_STOP=1 -U "${POSTGRES_USER:-postgres}" -d "${POSTGRES_DB:-postgres}" \
  -v pw="$MONITOR_PASSWORD" <<'SQL'
SELECT format('CREATE ROLE monitor LOGIN PASSWORD %L', :'pw')
 WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'monitor') \gexec
SELECT format('ALTER ROLE monitor PASSWORD %L', :'pw') \gexec
GRANT pg_monitor TO monitor;
SQL
