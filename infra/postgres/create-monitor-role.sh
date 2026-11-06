#!/bin/sh
# Creates the read-only Postgres role the exporter uses.
set -eu
: "${MONITOR_PASSWORD:?set MONITOR_PASSWORD}"

docker exec -i database sh -c 'psql -v ON_ERROR_STOP=1 -U "${POSTGRES_USER:-postgres}" -d "${POSTGRES_DB:-postgres}" -v pw="$1"' _ "$MONITOR_PASSWORD" <<'SQL'
SELECT format('CREATE ROLE monitor LOGIN PASSWORD %L', :'pw')
 WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'monitor') \gexec
SELECT format('ALTER ROLE monitor PASSWORD %L', :'pw') \gexec
GRANT pg_monitor TO monitor;
SQL
echo "monitor role ready"
