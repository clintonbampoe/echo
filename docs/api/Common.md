# Common Utilities

Shared code used across the whole application.

## HTTP Result Types

Services return an `IOperationResult` instead of raw `ActionResult` objects. Each result type knows how to convert itself into the correct HTTP response via `.ToActionResult()`.

| Type               | HTTP Status | Use case                        |
| ------------------ | ----------- | ------------------------------- |
| `OkResult`         | 200         | Success with a message          |
| `SuccessResult<T>` | 200         | Success with data               |
| `NotFoundResult`   | 404         | Record doesn't exist            |
| `BadRequestResult` | 400         | Operation failed                |
| `ConflictResult`   | 409         | Duplicate or conflicting record |

This keeps controllers clean — they just call the service and return `.ToActionResult()` without any conditional logic.

## Pagination

**`PaginationParameters`** — passed as pagination params on list requests:

- `PageNumber` — which page to return (default: 1)
- `PageSize` — records per page (default: 25, max: 50, min: 1)

**`PagedResponse<T>`** — the shape returned by all list endpoints:

- `Data` — the list of records
- `PageNumber` — current page
- `PageSize` — records per page
- `TotalRecordCount` — total records in the database (not just this page)

## Query Filtering

**`QueryParameters`** — optional filters passed as query params:

- `SearchTerm` — filters by `Name` on searchable entities
- `StartDate` / `EndDate` — filters by `CreatedAt`

**Extension methods** apply these filters to EF Core queries:

- `ApplyPagination` — skips and takes the right records for the requested page; orders by `Id` if no other ordering is set
- `ApplySearchFilter` — applies the `searchTerm` name filter
- `ApplyDateFilters` — applies `startDate` / `endDate` filters
- `ApplySoftDeleteFilter` — excludes records where `DeletedAt` is not null
