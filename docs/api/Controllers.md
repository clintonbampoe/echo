# Controllers

Controllers receive HTTP requests and pass them to the relevant service. They do no logic themselves — they just call the service and return the result.

All routes follow the pattern `/api/{resource}` (e.g. `/api/members`, `/api/events`).

## Standard Endpoints

Every controller exposes the same five base endpoints:
However controllers may expose other endpoints depending on the needs of the clients.

| Method | Route                  | What it does              |
| ------ | ---------------------- | ------------------------- |
| GET    | `/api/{resource}`      | Get a paginated list      |
| GET    | `/api/{resource}/{id}` | Get a single record by ID |
| POST   | `/api/{resource}`      | Create a new record       |
| PUT    | `/api/{resource}/{id}` | Update an existing record |
| DELETE | `/api/{resource}/{id}` | Soft-delete a record      |

## Query Parameters (list endpoints)

| Parameter    | Default     | Description                                        |
| ------------ | ----------- | -------------------------------------------------- |
| `pageNumber` | 1           | Which page to return                               |
| `pageSize`   | 25 (max 50) | How many records per page                          |
| `searchTerm` | —           | Filter by name (only on searchable entities)       |
| `startDate`  | —           | Only return records created on or after this date  |
| `endDate`    | —           | Only return records created on or before this date |

## Special Endpoints

