-- This contains all the extensions our database relies on

CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Query statistics. The library is preloaded via
-- infra/postgres/conf/observability.conf; this creates the extension
-- itself. On a database that was initialised before this file existed, run
-- it by hand:
--   CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
