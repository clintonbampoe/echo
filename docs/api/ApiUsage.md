# Api Usage

**Written by:** @clintonbampoe
**Last updated**: 2026-09-12 by @clintonbampoe

---

This is the official guide to using the Echo API.
>**New Developer?** Start with [Getting Started](./GettingStarted.md) to create your first admin and obtain a JWT Token.

## Infrastructure & Access

When running Echo in development mode via Docker, the API is accessible through two different ports.

| Port | Role | Description |
| :--- | :--- | :--- |
| `8080` | **Nginx Gateway** | The primary "Front Door". All requests should typically go here. |
| `5025` | **Direct Kestrel** | Direct access to the .NET API. Useful for debugging or bypassing the proxy. |

### Interactive API Documentation
Live endpoints can be accessed when the api is running in **development**:

- [Swagger UI](http://localhost:8080/swagger/index.html) - Interactive API docs with **Swagger**.
- [Scalar UI](http://localhost:8080/scalar) - Dynamic API tester with clean UI rendered in browser.

---

## Base Url & Versioning

All API endpoints are served under the `/api` path and are versioned.

- **Base route:** `/api/v{version}/[controllerName]`
- **Current version:** `v1`

The version is part of the URL path (e.g. `/api/v1/events`). This is configured in the `CoreBaseController`, which all core controllers inherit from.

---

## Authentication

The Echo API uses JWT (JSON Web Token) authentication.

### Obtaining a JWT Token
Send a valid login request to the `Sessions` endpoint with your valid credentials. If you don't have any valid credentials, see [Creating a User Account](./GettingStarted#creating-a-new-user-account)

**Endpoint:** `POST /api/auth/v1/sessions/login`

**Payload**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response**:
```json
{
  "accessToken":"eyJhbGci...",
  "accessTokenExpiresAt": "2026-07-31T20:30:04.1100972Z",
  "refreshToken": "xoJuoFW...",
  "refreshTokenExpiresAt": "2026-08-30T20:15:04.1546699Z"
}
```

### Using an `accessToken`
Include the `accessToken` in the `Authorization` header of subsequent requests as a Bearer token.

```text
Authorization: Bearer <your-jwt-token>
```

> Access Tokens are short-lived (about 15 minutes). Use the `refreshToken` endpoint (`POST /api/auth/v1/sessions/refresh`) to acquire a new pair.

### Tenant Isolation & Security
Echo is a multi-tenant platform. The `CongregationId` is encrypted within the JWT.
**Security Note:** The system automatically extracts the `CongregationId` from your token. You cannot pass a different `CongregationId` in the request body or query to access other tenants; this is strictly enforced at the service layer.

### Roles
The token carries the user's `UserRole` (`Admin`, `Accountant`, `Clerk`, `Member`). Some endpoints are restricted by role and return `403 Forbidden` when the signed-in user's role is not allowed.

| Endpoints | Admin | Accountant | Clerk | Member |
|---|---|---|---|---|
| Users: every endpoint (list, get, create, update, delete, search) | yes | no | no | no |
| Invitations | yes | no | no | no |

`GET /users/me` and `PUT /users/me` let any signed-in user read and edit their own profile (names and password; never the role or email). Changing the password requires the current one and ends all the user's sessions: refresh tokens stop working, and the access token in use expires on its own within about 15 minutes.

All other endpoints currently only require a signed-in user. Finer role-based access (for example for tithes and other finance data) is planned after the first deployment.

---

## Standard Response Formats

To prevent surprises, all API endpoints follow these explicit response shapes.

### 1. Standard Success Response
Most endpoints return data wrapped in a `data` object.
```json
{
  "data": {
    "id": "guid",
    "name": "Example Name",
    ...
  }
}
```

### 2. Paginated Response (`PagedResponse<T>`)
Used by all list endpoints.
```json
{
	"next": "opaque_base64_string",
	"hasMore": true,
	"data": [
		{ "id": "guid", "name": "Item 1" },
		{ "id": "guid", "name": "Item 2" }
	],
	"window": { "from": "2026-10-11", "to": "2026-10-14", "isDefault": true }
}
```
- `window`: The date range applied to the list, or `null` if none was. See [Default date windows](#3-cursor-based-pagination--filtering).
- `next`: An opaque string used to fetch the next page. If `null`, no more pages exist.
- `hasMore`: A boolean flag indicating if more records are available.

### 3. Standard Error Response (Problem Details)
Errors follow the RFC 7807 standard.
```json
{
  "status": 400,
  "title": "An unexpected error occured with your request.",
  "detail": "Specific error message explaining why the request failed.",
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736"
}
```
Unexpected errors (`500`) and malformed requests (`400`) include a `traceId` that matches the trace in the logs. Quote it when reporting a problem. Requests that break a validation rule (for example a password shorter than 8 characters) return `400` with the same fields as other errors (`status`, `type`, `errorCode: "VALIDATION_ERROR"`, `traceId`) plus an `errors` map listing each rejected field:
```json
{
  "type": "https://api.example.com/errors/validation-error",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errorCode": "VALIDATION_ERROR",
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "errors": { "Token": ["The Token field is required."] }
}
```
This applies to every endpoint, including the `/auth` ones.

---

## Common Endpoints

The API is organised by domain. Each domain has a dedicated controller. Below is an example with the `Events` domain. The same pattern applies to `Members`, `Assets`, `Attendance`, etc.

**Events Controller**
**Base route:** `/api/v1/events`

| Method   | Endpoint                 | Description                                                |
| -------- | ------------------------ | ---------------------------------------------------------- |
| `GET`    | `/api/v1/events/summary` | Get a summary of events *(in development)*                 |
| `GET`    | `/api/v1/events`         | Get a paginated list of events (supports cursor & filters) |
| `GET`    | `/api/v1/events/search`  | Search events by name using trigram similarity (Top 5)     |
| `GET`    | `/api/v1/events/{id}`    | Get a single event by ID                                   |
| `POST`   | `/api/v1/events`         | Create a new event                                         |
| `PUT`    | `/api/v1/events/{id}`    | Update an existing event                                   |
| `DELETE` | `/api/v1/events/{id}`    | Delete an event                                            |

---

## Using the API

### API Health Check
```bash
curl http://localhost:8080/api/health/ready
```

This endpoint is public and is available without authentication. It returns
`200` only when the database is reachable and every migration has been applied;
otherwise it returns `503` with the failing check. Use `/api/health/live` if you
only want to know whether the process is up. See
[Health endpoints](../Infrastructure.md#health-endpoints).
curl -H "Authorization: Bearer <your-token>" \
     "http://localhost:8080/api/v1/events?cursor=eyJpZ...&pageSize=20"
```
  
### 1. Get a token
See [Authentication](#authentication) above.

### 2. Call an endpoint
Include the token in the `Authorization` header. 

### 3. Cursor-Based Pagination & Filtering
Echo uses **opaque cursor-based pagination**. Do not use page numbers.

**Initial Request**
To get the first page, omit the `cursor` parameter.
```bash
curl -H "Authorization: Bearer <your-token>" \
     "http://localhost:8080/api/v1/events?pageSize=20"
```

**Fetching Subsequent Pages**
Take the `next` value from the previous response and pass it as the `cursor` parameter.


**Constraints & Behavior**
- **Page Size**: The `pageSize` is hard-clamped to a maximum of **24**. Requests for higher values will be automatically reduced to 24.
- **Malformed Cursors**: If a tampered or invalid cursor is provided, the system treats it as `null` and returns the first page of results.

**Filtering**
Filtering is achieved via query parameters. The available filters depend on the entity.
*Example: Fetching members filtered by status*
`GET /api/v1/members?status=Active&pageSize=20`

**Default date windows**
List and summary endpoints that filter by date apply a default window when both `from` and `to` are omitted. An explicit `from` or `to` always wins; the missing side is filled from the default.

| List | Default window | Date field |
|---|---|---|
| Attendance records | this week to today | session date |
| Tithes | this week to today | collection date |
| Transactions | this week to today | transaction date |
| Sessions | whole current week (Sunday to Saturday) | session date |
| Project contributions | this month to today | date contributed |
| Visitors | this year | created date (converted date when `converted=true`) |
| Projects | this year; running projects stay visible | start and end date |
| Members, Assets, Organizations, Events | this year | joined / purchase / created / start date |

Attendance (`sessionId`, `personId`), tithes (`memberId`, `year` or `month`, including `GET /tithes/member/{memberId}`) and contributions (`projectId`) apply **no default window** when scoped to a parent record.

A project is in the window when it started on or before `to` and has not ended before `from`; open-ended projects (no end date) always stay visible once started. This applies to explicit `from`/`to` too.

Paged list responses and summary responses include `window: { from, to, isDefault }`, or `null` when no window was applied. Dates use UTC and weeks start on Sunday.

**Counts on list responses**
Some list, detail and update responses carry a count so a screen does not need one extra request per row. Deleted records are never counted, and a newly created record always reports `0`.

| Response | Field | What it counts |
|---|---|---|
| Event | `registrationCount` | registrations for the event |
| Organization | `memberCount` | members assigned to the organization |
| Project | `raisedAmount` | sum of the project's contributions |
| Visitor | `visitCount`, `lastVisitedAt` | attendance records made while the person was a visitor, and the date of the latest session they attended (`null` when none) |
| Project category | `projectCount` | projects in the category |
| Asset category | `assetCount` | assets in the category |
| Transaction category | `transactionCount` | transactions in the category |

For categories this is the same number that blocks deleting a category with `409`.

**Moving records between categories**
A category that is still used cannot be deleted. To clear it, move its records first:

`POST /transaction-categories/{id}/reassign`, `/project-categories/{id}/reassign`, `/asset-categories/{id}/reassign`

```json
{ "targetCategoryId": 7, "deleteSource": false }
```

Every non-deleted record of the source category moves to the target in one database transaction, and the response is `{ "moved": 12, "sourceDeleted": false }`. With `deleteSource: true` the source category is soft-deleted in the same transaction. Errors: `400` if the target is the source (or, for transaction categories, has a different income/expense type), `404` if the source is missing, `404 FOREIGN_KEY_NOT_FOUND` if the target is missing.

**Transaction type and category type**
A transaction's `transactionType` must equal its category's `categoryType`. Creating or updating a transaction that breaks this returns `400`. An update that changes only `transactionType` is checked against the transaction's current category.

**Summaries**
Most modules have a `GET .../summary` that accepts the same filters as the list. Besides the totals, a few have extras:

- `GET /events/summary` returns `totalEvents`, `upcomingEvents`, `totalRegistered` and `totalAttended`. `upcomingEvents` counts events that start today or later (matching the name, organization and organizer filters) and is not limited by `from`/`to`.
- `GET /visitors/summary`: `totalVisitors` and `recurringVisitors` describe the visitor list (people who have not converted). `newVisitors` is everyone created in the window and `convertedVisitors` is everyone converted in the window, whatever their current status.
- `GET /tithes/summary`: `averagePerMember` is `totalCollected` divided by `uniqueTithers` (0 when nobody gave).
- `GET /members/summary`: `averageAge` is computed from the full date of birth, so someone whose birthday has not come yet this year is a year younger.
- `GET /transactions/summary` also returns `byCategory`: one row per category and type with `categoryId`, `categoryName`, `type`, `total` and `count`, largest total first. `mostActiveCategory` (by number of transactions) is unchanged.
- `GET /assets/summary` leaves out liquidated assets by default, because its totals describe what the church owns now. Pass `includeLiquidated=true` to include them. The asset list still shows liquidated assets; `includeLiquidated=false` hides them, and `status=Liquidated` always shows them.
- `GET /transactions/summary/monthly` and `GET /tithes/summary/monthly` return one row per calendar month so a chart needs a single request. Transactions return `{ year, month, totalIncome, totalExpenses, net }`; tithes return `{ year, month, totalCollected, uniqueTithers }`. Months with nothing in them are returned with zeros. `from` and `to` choose the months (any day in the first and last month); the default is the last 12 months, and the range may cover at most 24 months and `from` must not be after `to`, otherwise the API returns `400`. The first and last months are always counted whole, whichever day you pass. Tithes are grouped by the month they are **for**, not by the day they were collected.

**Invitations**
Admins invite people with a code. An invitation is **reusable until it expires or is revoked**, and each registration adds one to its `useCount`.

| Endpoint | Who | What it does |
|---|---|---|
| `POST /auth/invitations` | Admin | creates an invitation (`allowedRole`, optional `expiryDays` 1–30 and `label`); returns the code **once** |
| `GET /auth/invitations` | Admin | lists them, newest first; filter by `status` (`Active`, `Expired`, `Revoked`) |
| `PUT /auth/invitations/{id}` | Admin | changes the `label` and/or makes it expire `expiryDays` from now (not for a revoked one) |
| `POST /auth/invitations/{id}/revoke` | Admin | stops it working; it stays in the list as `Revoked` |
| `DELETE /auth/invitations/{id}` | Admin | removes it from the list and stops it working |
| `GET /auth/invitations/{code}` | anyone | shows the congregation (name, city, region), role and expiry, so the join screen can confirm before sign-up |

The code looks like `K7QX-92MD-PW4T`: 12 characters from an alphabet without I, L, O or U, so it is easy to read out and type. It can be entered in any case, with or without dashes or spaces, and `o` is read as `0` and `i`/`l` as `1`. The server stores only a hash, so a code cannot be shown again. The lookup is rate limited and answers `404` for a wrong, expired, revoked or deleted code alike.

`GET /congregation` returns the signed-in user's own congregation (name, type, contact details, location) for any role.

**Events and attendance**
- `GET /events` lists the oldest event first. Pass `order=desc` for the newest first (for a "Past" tab). A `next` cursor only continues a list in the direction it came from; with the other `order` the list starts again from the first page.
- `POST /event-registrations` returns `409` with `errorCode: "EVENT_FULL"` once an event has as many live registrations as its `capacity`. Events without a capacity are unlimited, lowering a capacity below the current count just keeps the event full, and check-ins are never limited. Two requests for the last seat are handled one after the other, so the event cannot be overbooked.
- `POST /attendance/bulk` marks up to 500 people present for a session in one request: `{ "sessionId": ..., "personIds": [...], "checkedInAt": ... }`. It returns `created` (the new records) and `skipped` (each with a `reason`: `AlreadyPresent` or `PersonNotFound`). Already-present people are skipped, not errors, an id sent twice counts once, and all the new records are saved together. If someone marks one of those people at the very same moment, the request returns `409` with `errorCode: "ATTENDANCE_CONFLICT"` and saves nothing; send it again and they come back as `AlreadyPresent`.

### 4. Search
Search endpoints are available for all searchable entities. They use **trigram similarity** to find the best matches regardless of exact spelling.
- **Behavior**: Returns the top 5 most relevant results. The term is matched literally, so `%` and `_` are not treated as wildcards.
- **Term**: the `q` query parameter is required and must be between 1 and 100 characters; otherwise the API returns `400`.
- **Example**: `GET /api/v1/events/search?q=Christmas`

### 5. Health Check
To verify the API is running:
```bash
curl http://localhost:8080/health/ready
```

---

## Related Documentation

- [GettingStarted](GettingStarted.md): Local development setup
- [Infrastructure](./../Infrastructure.md): Docker, containers, reverse proxy and environment variables
- [Front-end](frontend/README.md): Front-end documentation
