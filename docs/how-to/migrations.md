# Migrations

Schema changes are EF Core migrations in `backend/src/Echo.Data`. The
migrator applies them. It is its own image and container, so a migration is
never a side effect of the API starting.

## In production

You do nothing. The deploy runs `migrator --check=true` before starting the
new API:

- exit 0: the database is current, nothing to apply.
- exit 10: migrations are pending. The deploy takes a dump, then runs the
  migrator. If the dump or the migration fails, nothing else changes and the
  old release keeps running.
- any other exit: the deploy stops.

The server's `.env` has `RunMigrationsOnStartup=false`, and `deploy.sh`
refuses to run without it. The migrator is the only thing that migrates.

A rollback does not undo a migration. Write each migration so the previous
release still works against it: add a column or table in one release, stop
using the old one in the next, drop it in a later one.

## In development

`RunMigrationsOnStartup=true` (the default in `.env.example`) makes the API
migrate on startup. To run the migrator by hand:

```bash
docker compose --profile db-tools run --rm migrator
```

## Add a migration

`dotnet ef` needs a connection string named `Default`, which the app itself
does not use (it reads `ConnectionStrings__echodb`):

```bash
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=echo_db;Username=postgres;Password=<your DB_PASSWORD>"
dotnet ef migrations add "MigrationName" \
  --project backend/src/Echo.Data \
  --startup-project backend/src/Echo.Api
```

The development database publishes port 5432 on localhost. To apply the
migration, run the migrator as above, or `dotnet ef database update` with the
same two project flags.

The readiness check (`/api/health/ready`) fails with 503 while migrations are
pending.
