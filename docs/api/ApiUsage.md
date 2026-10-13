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
| Tithes: list, summary, by member, get by ID, update, delete | yes | yes | no | no |
| Tithes: create (`POST /tithes`) | yes | yes | yes | no |
| Users: every endpoint (list, get, create, update, delete, search) | yes | no | no | no |
| Invitations | yes | no | no | no |

All other endpoints currently only require a signed-in user.

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
	"nextCursor": "opaque_base64_string",
	"hasMore": true,
	"data": [
		{ "id": "guid", "name": "Item 1" },
		{ "id": "guid", "name": "Item 2" }
	],
	"window": { "from": "2026-10-11", "to": "2026-10-14", "isDefault": true }
}
```
- `window`: The date range applied to the list, or `null` if none was. See [Default date windows](#3-cursor-based-pagination--filtering).
- `nextCursor`: An opaque string used to fetch the next page. If `null`, no more pages exist.
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
Unexpected errors (`500`) and malformed requests (`400`) include a `traceId` that matches the trace in the logs. Quote it when reporting a problem. Requests that break a validation rule (for example a password shorter than 8 characters) return `400` with a validation problem listing each field under `errors`.

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
Take the `nextCursor` value from the previous response and pass it as the `cursor` parameter.


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

Attendance (`sessionId`, `personId`), tithes (`memberId`, including `GET /tithes/member/{memberId}`) and contributions (`projectId`) apply **no default window** when scoped to a parent record.

A project is in the window when it started on or before `to` and has not ended before `from`; open-ended projects (no end date) always stay visible once started. This applies to explicit `from`/`to` too.

Paged list responses and summary responses include `window: { from, to, isDefault }`, or `null` when no window was applied. Dates use UTC and weeks start on Sunday.

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
