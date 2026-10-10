# Echo.Seeder

Development data seeder for Echo. Deterministic — same seed produces identical
data on every machine, every run.

See `SEED.md` for admin logins, volume config, and verification queries.

---

## Where it runs

The seeder runs as part of the Aspire AppHost, or as a one-shot container
under Docker Compose. It is not intended to run directly on the host.

### Aspire

```bash
dotnet run --project src/Echo.AppHost
```

The seeder is a resource in the AppHost. It waits for the database to be
healthy, then runs. Restart the resource from the Aspire dashboard to re-run
it.

### Docker Compose

```bash
docker compose up -d db
docker compose run --rm seeder
```

First run takes roughly 30 to 60 seconds for the big congregation. Later runs
are faster because Postgres has warmed its caches.

---

## Architecture

```
Program.cs
└── SeederWorker (BackgroundService)
    ├── SeederContext      — shared state for the whole run
    ├── SeedFakers         — Bogus fakers, seeded for determinism
    └── Phases (in order)
        ├── SetupPhase         → congregations, users, lookup tables  [EF]
        ├── PeoplePhase        → persons, members, visitors           [COPY]
        ├── OrganizationsPhase → organizations, memberships           [COPY]
        ├── EventsPhase        → events, registrations, attendance    [COPY]
        ├── AttendancePhase    → service attendance                   [COPY]
        ├── FinancesPhase      → tithes, transactions, projects       [COPY]
        └── AssetsPhase        → assets                               [COPY]
```

---

## The two write paths

**EF Core** is used only in `SetupPhase`, for small structural data where the
DB generates integer primary keys (`AttendanceType`, `AssetCategory`, and so
on) and we need those IDs back. EF populates the IDs on the entities after
`SaveChangesAsync`, and later phases use them as foreign keys.

**Npgsql binary COPY** is used for everything with volume. It bypasses EF's
change tracker and writes directly to Postgres. The column order in each
`COPY` statement is the contract between the seeder and the database. If you
add a column in a migration, update the matching file in `Copy/`.

Each COPY batch runs inside its own transaction. If a batch fails, only that
batch rolls back. The exception propagates and the seeder stops.

---

## State flows through CongregationSeed

`SeederContext.State` is a `List<CongregationSeed>`. Each phase iterates over
it, reads what it needs, and writes its output back into the same object.

| Phase | Fills |
|---|---|
| `SetupPhase` | `Congregation`, `Admin`, `AttendanceTypes`, `AssetCategories`, `ProjectCategories`, `TransactionCategories` |
| `PeoplePhase` | `MemberIds`, `VisitorIds` |
| `OrganizationsPhase` | `OrganizationIds` |
| `EventsPhase` | `EventIds` |
| `FinancesPhase` | `ProjectIds` |
| `AssetsPhase` | `AssetIds` |

If a phase runs before the phase that populates something it depends on,
you'll get an empty collection and a warning log. The ordering in
`SeederWorker` is the correct one. Don't rearrange phases without checking
their dependencies.

---

## Determinism

Every random value comes from `Rng`, which wraps `System.Random` with a fixed
seed. Each phase gets its own `Rng` instance, seeded with
`HashCode.Combine(globalSeed, phaseName)`. This means:

- Same global seed produces the same data every time, on any machine.
- Editing one phase doesn't shift another phase's data.
- Adding a new phase doesn't shift existing phases.

Bogus fakers are also seeded from the global seed (`UseSeed(opts.Seed)`).
`FixedTimeProvider` pins "now" to `ReferenceNow` so all timestamps are stable.

### Rules for new code

- Never call `DateTime.UtcNow` or `DateTime.Now` in a phase. Use `ctx.Now`.
- Never call `new Random()` or `Random.Shared`. Use `rng`.
- Never use `Guid.NewGuid()` inside a phase that could be re-run with the
  same seed. Use `rng.NewGuid()` if it exists, or derive from existing IDs.

---

## Configuration

All options live in the `Seeder` section of `appsettings.json`:

```json
{
  "Seeder": {
    "Seed": 42,
    "Reset": false,
    "BatchSize": 2000,
    "ReferenceNow": "2026-10-10T00:00:00+00:00",
    "SmallCount": 10,
    "BigMembers": 50000,
    "BigVisitors": 5000,
    "SmallMembers": 200,
    "SmallVisitors": 50
  }
}
```

Override with environment variables:

```
Seeder__Reset=true
Seeder__Seed=99
Seeder__BatchSize=500
Seeder__BigMembers=1000
```

In Aspire, add these to the seeder resource's environment in `AppHost.cs`.
In Docker Compose, add them under the `seeder` service's `environment` key,
or pass them on the command line:

```bash
docker compose run --rm seeder Seeder:Reset=true Seeder:Seed=99
```

---

## Adding a new phase

1. Create `Phases/YourPhase.cs` implementing `ISeedPhase`.
2. If it produces volume data, add a `Copy/YourCopy.cs` with the COPY writer.
3. If it needs new state, add a property to `CongregationSeed`.
4. Add it to the phases array in `SeederWorker` at the right position.

That's it. No registration, no DI wiring beyond constructor injection.

---

## When the schema changes

The COPY writers in `Copy/*.cs` are the only files that need updating. Each
file maps exactly one entity to its table. The column list in the `COPY`
statement and the `WriteAsync` calls below it must stay in sync. They are
intentionally explicit and verbose so a mismatch is obvious.

The TRUNCATE list in `SeederWorker.ResetAsync` also needs updating when
tables are added or renamed.

---

## Common pitfalls

**DateTimeKind must match the column type.**
Audit columns (`CreatedAt`, `DeletedAt`, `ConvertedAt`) are `timestamptz` and
require `Kind = Utc`. Calendar columns (`DateOfBirth`, `JoinedDate`) are
`date` and require `Kind = Unspecified`, or use `DateOnly`, which has no Kind
at all. The COPY writers must also declare `NpgsqlDbType.TimestampTz` for
`timestamptz` columns. `NpgsqlDbType.Timestamp` (without `Tz`) means
`timestamp without time zone` and will be rejected.

**Faker date rules return Local by default.**
`f.Date.Past(...)`, `f.Date.Recent(...)`, and friends always produce
`DateTimeKind.Local`. Wrap with `DateTime.SpecifyKind(..., DateTimeKind.Utc)`
when the target is an audit column. Do not use `.ToUniversalTime()` — it
shifts the value by the machine's UTC offset and breaks seed reproducibility.

**Unique-indexed fields need to be unique by construction.**
Random draws from a finite pool collide. This is the birthday paradox, and it
happens faster than you'd expect. Use `rng.Shuffle(pool).Take(n)` or
`rng.PickSubset(pool, n)` to sample without replacement. Appending a
per-congregation index is also fine.

**Foreign key ordering matters within a batch.**
Write `People` before `Members` and `Visitors`. Write `Organizations` before
`OrganizationMembers`. Converted visitors need their `Members` row written
before their `Visitors` row, or the FK will fail.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `duplicate key value violates unique constraint` on a rerun | Existing data without `Reset=true` | `docker compose run --rm seeder Seeder:Reset=true` |
| `DateTime with Kind=... cannot be written` | Kind mismatch or wrong `NpgsqlDbType` | Check `Copy/*.cs` and the faker rule |
| FK violation on `Visitors` or `Members` | Bad FK target | Fix the faker to reference a real member |
| FK violation on `OrganizationMembers` | Batch ordering | Write `Organizations` before `OrganizationMembers` |
| Seeder hangs | Batch size too large | Set `Seeder__BatchSize=500` |
| `Cannot load library libgssapi_krb5.so.2` | Npgsql Kerberos warning | Harmless if the DB is reachable |

Last resort (Docker):

```bash
docker compose down -v
docker compose up -d db
docker compose run --rm seeder Seeder:Reset=true
```

Last resort (Aspire): stop the AppHost, remove the containers and volumes it
created, then restart it.
