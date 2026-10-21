# Database Migrations

**Written by:** @clintonbampoe
**Last updated:** 2026-10-21 by @clintonbampoe

---

## Paths

### Automatic (default)
Set `RunMigrationsOnStartup=true` in `.env` (default). API runs `Database.MigrateAsync()` on startup. Used by `migrator` profile in compose.

### Manual
```bash
docker compose run --rm migrator
```

### Local Development (CLI)
```bash
# Add migration
dotnet ef migrations add "MigrationName" \
  --project backend/src/Echo.Data \
  --startup-project backend/src/Echo.Api

# Apply
dotnet ef database update \
  --project backend/src/Echo.Data \
  --startup-project backend/src/Echo.Api
```

> **Note:** `DesignTimeDbContextFactory` reads connection string `"Default"` while runtime uses `"echodb"` — `dotnet ef` is currently broken design-time. Use the migrator container for reliable migrations.

## Compose Profile
```yaml
migrator:
  profiles: [db-tools]
```
Run with: `docker compose --profile db-tools run --rm migrator`