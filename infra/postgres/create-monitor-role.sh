#!/bin/sh
# Creates the read-only role Alloy's Postgres exporter signs in as. Safe to run again (it resets the password).
#
#   MONITOR_PASSWORD=... ./infra/postgres/create-monitor-role.sh
#
# Runs psql inside the `database` container, as the database's own user. pg_monitor can read the statistics views
# (including pg_stat_statements) and nothing else: it cannot read a single row of the church's data.
set -eu
: "${MONITOR_PASSWORD:?set MONITOR_PASSWORD}"

docker exec -i database sh -c 'psql -v ON_ERROR_STOP=1 -U "${POSTGRES_USER:-postgres}" -d "${POSTGRES_DB:-postgres}" -v pw="$1"' _ "$MONITOR_PASSWORD" <<'SQL'
SELECT format('CREATE ROLE monitor LOGIN PASSWORD %L', :'pw')
 WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'monitor') \gexec
SELECT format('ALTER ROLE monitor PASSWORD %L', :'pw') \gexec
GRANT pg_monitor TO monitor;
SQL
echo "monitor role ready"
