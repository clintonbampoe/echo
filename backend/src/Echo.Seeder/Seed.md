# Seed Data Reference

What you need to sign in, tune the volume and check the seeded database.

**Password for every seeded admin:** `admin@123`

**Big congregation:** Grace Chapel International, about 50,000 members and
5,000 visitors, 15 years old. Use it for anything about performance. The small
congregations are 3 to 12 years old and several hundred people each.

---

## Admin accounts

Every congregation has one admin. The email is `admin@` plus the congregation
name in lower case without spaces, then `.org`.

| Congregation | Email |
|---|---|
| Grace Chapel International (big) | `admin@gracechapelinternational.org` |
| Bethel Chapel | `admin@bethelchapel.org` |
| Zoe Fellowship | `admin@zoefellowship.org` |

The small congregations are a random pick of stem and suffix (see below), so
the list depends on `Seed`. To see yours:

```sql
SELECT c."Name", u."EmailAddress"
FROM "Congregations" c
JOIN "Users" u ON u."CongregationId" = c."Id" AND u."DeletedAt" IS NULL
WHERE c."DeletedAt" IS NULL
ORDER BY c."Name";
```

---

## Options

Defaults in `appsettings.json`. Everything is deterministic: the same options
give the same data.

| Option | Value | What it does |
|---|---|---|
| `Seed` | `42` | Seeds every random stream |
| `Reset` | `true` | Empties the seeded tables first. When `false`, the seeder stops if a congregation exists |
| `BatchSize` | `2000` | Rows per COPY batch |
| `ReferenceNow` | `2026-10-10T00:00:00Z` | "Today" for the data. Fixed so runs match |
| `HistoryYears` | `15` | Age of the big congregation. Small ones are 3 to `HistoryYears - 3` |
| `AttendanceWeeks` | `52` | Weeks of attendance before today. A year is about 2 million rows |
| `FutureWeeks` | `4` | Weeks of scheduled sessions after today |
| `TitheMonths` | `36` | Months of tithes, offerings, expenses and contributions |
| `EventMonths` | `36` | Months of events before today (the next few months are planned too) |
| `BigMembers` | `50000` | Members in the big congregation |
| `BigVisitors` | `5000` | Visitors in the big congregation |
| `SmallCount` | `10` | Number of small congregations (code default is 5) |
| `SmallMembers` | `800` | Typical members in a small congregation |
| `SmallVisitors` | `250` | Typical visitors in a small congregation |
| `SmallSizeSpread` | `0.6` | How far sizes vary; `0.6` is 40% to 160% of typical |

---

## Running the seeder

The seeder runs in the Aspire AppHost, or as a one-shot container with Docker
Compose.

### Aspire

```bash
dotnet run --project src/Echo.AppHost
```

Restart the `seeder` resource from the dashboard to run it again.

### Docker Compose

```bash
docker compose up -d db
docker compose run --rm seeder

# Re-seed
docker compose run --rm seeder Seeder:Reset=true

# Another seed, or a smaller database
docker compose run --rm seeder Seeder:Seed=12345
docker compose run --rm seeder Seeder:BigMembers=1000 Seeder:AttendanceWeeks=12
```

Options can also be set with environment variables such as
`Seeder__BigMembers=1000`.

---

## Verify the seed

Run these after seeding. Every query should return zero rows.

```sql
-- Every converted visitor has a Members row
SELECT v."PersonId"
FROM "Visitors" v
LEFT JOIN "Members" m ON m."PersonId" = v."PersonId"
WHERE v."ConvertedAt" IS NOT NULL AND m."PersonId" IS NULL;

-- Person.Kind agrees with the side tables
SELECT p."Id", p."Kind"
FROM "People" p
LEFT JOIN "Members" m ON m."PersonId" = p."Id"
WHERE (p."Kind" = 'Member'  AND m."PersonId" IS NULL)
   OR (p."Kind" = 'Visitor' AND m."PersonId" IS NOT NULL);

-- No duplicate organization names within a congregation
SELECT "CongregationId", "Name", COUNT(*)
FROM "Organizations"
GROUP BY "CongregationId", "Name"
HAVING COUNT(*) > 1;

-- At most one tithe per member per month
SELECT "MemberId", "ForYear", "ForMonth", COUNT(*)
FROM "Tithes"
WHERE "DeletedAt" IS NULL
GROUP BY "MemberId", "ForYear", "ForMonth"
HAVING COUNT(*) > 1;

-- Nobody is a member before their congregation began
SELECT m."PersonId"
FROM "Members" m
JOIN "Congregations" c ON c."Id" = m."CongregationId"
WHERE m."JoinedDate" < c."CreatedAt"::date;

-- No tithe before the member joined
SELECT t."Id"
FROM "Tithes" t
JOIN "Members" m ON m."PersonId" = t."MemberId"
WHERE t."CollectionDate" < m."JoinedDate";

-- No member attendance before the member joined
SELECT a."Id"
FROM "Attendance" a
JOIN "Sessions" s ON s."Id" = a."SessionId"
JOIN "Members" m ON m."PersonId" = a."PersonId"
WHERE a."PersonKindAtTime" = 'Member' AND s."Date" < m."JoinedDate";
```

Congregation count:

```sql
SELECT COUNT(*) FROM "Congregations" WHERE "DeletedAt" IS NULL;
-- expect 1 + SmallCount (11 with the defaults)
```

---

## Reference data

Small congregation names are `{stem} {suffix}`. Each pair is used at most
once, so the names never repeat.

- **Stem:** Cornerstone, Redeemer, Zoe, Living Faith, Harvest, Bethel,
  Shalom, Trinity, Rock of Ages, New Covenant
- **Suffix:** Chapel, Assembly, Fellowship

That gives 30 possible names, so `SmallCount` can go up to 20 without running
out.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `Database already has N congregation(s)` | `Reset` is `false` and data exists | `docker compose run --rm seeder Seeder:Reset=true` |
| `duplicate key value violates unique constraint` | A generator broke a uniqueness rule | Report it; do not retry |
| FK violation | A child written before its parent | Report it; do not retry |
| `DateTime with Kind=... cannot be written` | Kind mismatch or wrong `NpgsqlDbType` | Check `Copy/*.cs` |
| Seeder hangs on a COPY | Batch size too large | Set `Seeder__BatchSize=500` |

Last resort (Docker):

```bash
docker compose down -v
docker compose up -d db
docker compose run --rm seeder Seeder:Reset=true
```

Last resort (Aspire): stop the AppHost, remove the containers and volumes it
manages, then restart it.
