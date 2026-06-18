# Services

Services sit between controllers and repositories.
They contain business logic, coordinate calls to the repository, and use AutoMapper to translate between entities and DTOs.

## ServiceBase\<T\>

All services extend `ServiceBase<T>`, which handles standard CRUD operations:

| Method            | What it does                                            |
| ----------------- | ------------------------------------------------------- |
| `GetPagedAsync`   | Fetch a paginated list; maps results to a list DTO      |
| `GetByIdAsync`    | Fetch a single record; returns 404 if not found         |
| `CreateNewRecord` | Map the create DTO to an entity and save it             |
| `UpdateRecord`    | Find the existing record, apply the update DTO, save it |
| `DeleteRecord`    | Soft-delete the record via the repository               |

## RelationshipServiceBase\<T\>

Extends `ServiceBase<T>` for join-type entities. Adds:

- `GetPageByForeignKeyPropertyId` — get a paginated list of records filtered by a related entity's ID

## Specialized Services

**`AttendanceService`**

- Adds `GetSummaryAsync` — returns a breakdown of who was present (members, visitors, guests) for a given date and service type

**`TitheService`**

- Adds `GetSummaryAsync` — returns monthly tithe totals for a given year

**`TransactionService`**

- Adds `GetSummaryAsync` — returns total income, total expenditure, net balance, and a per-category breakdown with each category's percentage share of its type
