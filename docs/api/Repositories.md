# Repositories

Repositories are the only layer that talks directly to the database.
Services call repositories; nothing else should.

## RepositoryBase\<T\>

All repositories extend `RepositoryBase<T>`, which provides standard database operations out of the box:

| Method         | What it does                                                |
| -------------- | ----------------------------------------------------------- |
| `GetPageAsync` | Fetch a paginated list of records                           |
| `GetByIdAsync` | Fetch a single record by ID                                 |
| `CreateRecord` | Insert a new record                                         |
| `UpdateRecord` | Update an existing record in place                          |
| `DeleteRecord` | Soft-delete a record (sets `DeletedAt`, does not remove it) |

All read queries use `.AsNoTracking()` — this tells EF Core not to track the returned objects, which improves performance since we're only reading, not modifying.

Soft-delete filtering (`ApplySoftDeleteFilter`) is applied automatically on reads, so deleted records never appear in results.

## RelationshipRepositoryBase\<T\>

Extends `RepositoryBase<T>` for entities that link two other entities (e.g. `OrganizationMember`, `EventAttendance`). Adds:

- `GetPageByForeignKeyPropertyId` — fetch a paginated list filtered by a foreign key value (e.g. all registrations for a specific event)

## Specialized Repositories

Some repositories override base behavior or add custom queries:

**`AttendanceRepository`**

- Overrides `GetPageAsync` and `GetByIdAsync` to include the related `Member`
- Adds `GetSummaryAsync` — returns a count of members, visitors, and guests present at a given service on a specific date

**`TitheRepository`**

- Overrides `GetPageAsync` and `GetByIdAsync` to include the related `Member`
- Adds `GetAggregatedSummaryByMonth` — groups tithe records by month and sums the totals for a given year

**`TransactionRepository`**

- Overrides `GetPageAsync` and `GetByIdAsync` to include the related `Category`
- Adds `GetStreamsAsync` — groups transactions by category and type, returning the total amount per group
