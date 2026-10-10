# Echo.Seeder

A dev data seeder for Echo. Deterministic — same seed produces identical data
on every machine, every run.

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

**EF Core** is used only in `SetupPhase` for small structural data where we
need the DB to generate int PKs (AttendanceType, AssetCategory etc.) and give
them back. EF populates those IDs on the entities after `SaveChangesAsync`, and
later phases use them as FKs.

**Npgsql binary COPY** is used for everything with volume. It bypasses EF's
change tracker and writes directly to Postgres. Column order in each `COPY`
statement is the contract between the seeder and the DB — if you add a column
in a migration, update the matching `Copy/*.cs` file.

Each COPY batch runs inside its own transaction. If a batch fails, only that
batch rolls back. The exception propagates and the seeder stops.

---

## State flows through CongregationSeed

`SeederContext.State` is a `List<CongregationSeed>`. Each phase iterates over
it, reads what it needs, and writes its output back into the same object.

```
SetupPhase       fills: Congregation, Admin, AttendanceTypes, AssetCategories,
                         ProjectCategories, TransactionCategories
PeoplePhase      fills: MemberIds (HashSet), VisitorIds (HashSet)
OrganizationsPhase fills: OrganizationIds
EventsPhase      fills: EventIds
FinancesPhase    fills: ProjectIds
AssetsPhase      fills: AssetIds
```

If a phase runs before the phase that populates something it needs, you'll get
an empty collection and a warning log. The ordering in `SeederWorker` is the
correct one — don't rearrange phases without checking their dependencies.

---

## Determinism

Every random value comes from `Rng`, which wraps `System.Random` with a fixed
seed. Each phase gets its own `Rng` instance seeded with
`HashCode.Combine(globalSeed, phaseName)`. This means:

- Same global seed → same data, every time, on any machine.
- Editing one phase doesn't shift another phase's data.
- Adding a new phase doesn't shift existing phases.

Bogus fakers are also seeded from the global seed (`UseSeed(opts.Seed)`).
`FixedTimeProvider` pins "now" to `ReferenceNow` so all timestamps are stable.

---

## Configuration

All options live in the `Seeder` section of `appsettings.json`:

```json
{
  "Seeder": {
    "Seed": 42,
    "Reset": false,
    "BatchSize": 2000,
    "ReferenceNow": "2025-01-01T00:00:00+00:00",
    "BigMembers": 50000,
    "BigVisitors": 5000,
    "SmallCount": 5,
    "SmallMembers": 200,
    "SmallVisitors": 50
  }
}
```

Override from CLI: `dotnet run -- Seeder:Reset=true Seeder:Seed=99`
Override from env: `Seeder__Reset=true Seeder__Seed=99`

---

## Running

```bash
# First run
dotnet run --project src/Echo.Seeder

# Reset and re-seed
dotnet run --project src/Echo.Seeder -- Seeder:Reset=true

# Custom seed
dotnet run --project src/Echo.Seeder -- Seeder:Seed=12345
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

The COPY writers in `Copy/*.cs` are the only thing that needs updating. Each
file maps exactly one entity to its table. The column list in the `COPY`
statement and the `WriteAsync` calls below it must stay in sync. They're
intentionally explicit and verbose so a mismatch is obvious.

The TRUNCATE list in `SeederWorker.ResetAsync` also needs updating when tables
are added or renamed.
