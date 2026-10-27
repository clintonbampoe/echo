# Backend Testing

**Written by:** @clintonbampoe
**Last updated:** 2026-10-22 by @clintonbampoe

---

Conventions for the `backend/tests/*` projects. Runner is **xunit.v3** on the
**Microsoft.Testing.Platform** (configured in the root `global.json`).

```
dotnet test --solution backend/Echo.slnx
```

---

## Packages

Central Package Management (`backend/Directory.Packages.props`).

| Package | Version | Why |
|---|---|---|
| `xunit.v3` | 4.0.0 | Test framework + MTP runner |
| `AutoFixture` / `AutoFixture.Xunit3` | 4.18.1 / 4.19.0 | Test data generation |
| `Verify.XunitV3` | 32.0.0 | Snapshot assertions |

> `Verify.XunitV3` is pinned to **32.0.0** deliberately: it is the last release before
> Verify's 2026-09-01 maintenance-fee cutoff. Do not bump past 32.x without reading
> <https://github.com/VerifyTests/Verify/blob/main/docs/maintenance-fee.md>.

---

## Shared test infrastructure

Both files live in `Echo.Application.Tests`.

### `TestData.NewFixture()`

Returns an `AutoFixture.Fixture` wired for **deterministic** output. AutoFixture has no
seed API (upstream #670, closed won't-fix) and its defaults are random, so every generator
is a deterministic sequence. Snapshots are only stable because of this.

| Type | Handling |
|---|---|
| `string` | per-fixture counter (`v1`, `v2`, …) — AutoFixture's random GUID strings are not scrubbed by Verify |
| `int`, `decimal` (+ nullable) | counters — Verify does not scrub numbers |
| `DateOnly`, `TimeOnly` | registered sequences — AutoFixture 4.x has no generator: `DateOnly` **throws**, `TimeOnly` yields garbage |
| `Guid`, `DateTime` | no registration needed — Verify renders them as `Guid_1` / `DateTime_1` |

`TestData.Shared` is a process-wide fixture for tests that just need a plausible graph
(query, window, ordering tests). Mapper tests must use `NewFixture()` per test, because
snapshot content depends on per-test generator state.

> **`[Range]` fields must be pinned.** AutoFixture honours `[Range]` via an engine-level
> `DataAnnotationsSupportNode` that fixture customizations cannot out-prioritise, so such
> fields come back random and the snapshot changes every run. Pin them with
> `.With(x => x.Prop, <fixed value>)`.

> **`Fixture` is not thread safe.** Test classes run in parallel, so `Shared` must never be
> touched directly — go through `TestAggregates` (`fixture.CreateX()`) or `TestData.Build(...)`,
> both of which take the fixture's lock. Per-test fixtures need no locking.

### `TestData.Many(count, create)`

Builds N values from **one** fixture. Sharing the fixture is what makes rows distinct inside a
single snapshot — a fresh fixture per row would restart every generator and produce identical
rows.

### `TestAggregates` — `fixture.CreateX()` extension methods

Builds aggregates with their **navigation properties populated**, because mappers flatten
through them and a null navigation silently yields an empty string instead of exercising the
mapping. Everything else is auto-generated.

Each helper ends with `.Live()`, which clears `DeletedAt`. AutoFixture populates every
writable property, so `DeletedAt` would otherwise be a non-null timestamp and helpers such
as `Live()` would filter the row out.

---

## Mapper tests

Pattern, using `Events/EventMapperTests.cs` as the reference.

```csharp
[Fact]
public Task ToDto_ShouldMapAllFieldsAndFlattenNavigations_FromEntity()
{
    var entity = CreateEvent(NewFixture());

    return Verify(_mapper.ToDto(entity));
}
```

Rules:

1. **Never pass a generated object as a test method parameter.** Verify derives snapshot
   filenames from test method arguments, so the file would be renamed on every run. Build
   inputs inside the test body.
2. **Pin flattened navigation values** to readable literals (`.With(p => p.Name, "Jane Doe")`)
   so the snapshot shows intent. `Person.Name` / `User.Name` are `get; init;` precisely so
   tests can populate them — at runtime they are Postgres computed columns.
3. **`ToEntity` keeps explicit asserts** alongside the snapshot for the
   `[MapperIgnoreTarget]` properties (usually `Id`, `CongregationId`, navigations). Verify
   scrubs Guids to `Guid_N` and cannot distinguish "left empty" from "assigned".
4. **All-null patch DTOs** are just `new XUpdateDto()` — every member is nullable.
5. Accept a new snapshot by renaming `*.received.*` to `*.verified.*` after reviewing the diff.

---

## Performance

Test collections run in parallel (`parallelizeTestCollections` is **enabled** in
`xunit.runner.json`). Serial execution was measured at ~12s for `Echo.Application.Tests`;
parallel brings it to ~5s. Correctness does not depend on serialisation — snapshot determinism
comes from the per-test fixture, and the one process-wide fixture is lock-guarded.

Run the formatter before committing — the repo is CSharpier-formatted:

```bash
csharpier format backend/tests/Echo.Application.Tests
csharpier check backend/tests/Echo.Application.Tests
```

`parallelizeTestCollections` is **disabled** in `xunit.runner.json` because the generators use
per-fixture counters.

---

## Adding a mapper test

1. Cover every public method on the `I*Mapper` interface.
2. Use `TestData.NewFixture()`; use `TestAggregates.CreateX()` when the mapper flattens a
   navigation, or build the aggregate inline with readable pinned values.
3. Pin every `[Range]` and every enum you want to assert on.
4. Run once, review each `*.received.*` file, then rename to `*.verified.*`.