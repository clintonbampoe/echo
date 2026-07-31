# Api Usage

**Written by:** @clintonbampoe
**Last updated**: 2026-07-31 by @clintonbampoe

---

This is the official guide to using the Echo API.
>**New Developer?** Start with [Getting Started](./GettingStarted.md) to create your first admin and obtain a JWT Token.

For granular endpoint details, we have a myriad of options which you can choose from depending on your use case:

- [Swagger UI](./ApiDocsSwagger.html) - Interactive API docs
- [Bruno UI](./ApiDocsBruno.html) - Interactive API docs
- [OpenApi Spec](./OpenApi.json) - Open Api specifications in json format which you can import anywhere
- [Scalar UI](http://localhost:8080/scalar) - Dynamic API tester with clean UI rendered in browser. This URL only works as long as the Echo API containers are running.

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

Send a valid login request to the `Sessions` endpoint with your valid credentials. If you don't have any valid credentials yet, see [Creating a User Account](./GettingStarted#creating-a-new-user-account)

**Endpoint:** `POST /api/v1/auth/sessions/login`

**Payload**

```json
{
  "email": null,
  "password": null
}
```

**Response**: The API then returns a request response that looks something like this

```json
{
  "accessToken":"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwMTlmYjQ2OC05YWRmLTc3OGMtODAzZC1mNGZiYWUwMzYxN2EiLCJqdGkiOiJlYzhmNWZjMy1mZmU4LTRkNTYtOTQ2ZC1iYTNiNzdmYzdkNzkiLCJyb2xlIjoiQWRtaW4iLCJjb25ncmVnYXRpb25JZCI6IjAxOWZiNDY4LTlhZGMtN2RjNy1iNmVlLWMwMDUwOTlkMWYzOSIsImV4cCI6MTc4NTUyOTgwNCwiaXNzIjoiZWNoby1hcGkiLCJhdWQiOiJlY2hvLWNsaWVudHMifQ.ZKfR-sxvN9HrpeA1VqVcjnH5rU5mqNmeIBn6WLgFcyuqNeRrD77tltCNse5DNNgfk4vpaL3bJs_c67Mgnz0TuybSBj3KGpRz4g5F3Wr_YGjmMWm4P4US7LSejmygMwhS_hAneM9yhop_Ghp8ck63CbKaqNjl1XN1_7LrwoWtpL76Xkk7rRAckj6ABS7M6UPYrQ7_FCdhwhAQv8t5z3TP_BBbNBZg3o_K_J56thk6Q5yDEPg5gqq5Uvv7fQZs0ZP03gUrh9RIK2X_wmjUdIBPEDDTY_kMcon9Ww24nmzC1Fg8ergwKRFadOnKh8HEMmboVVCrUyxQooKVvSgrdXl5hg",
  "accessTokenExpiresAt": "2026-07-31T20:30:04.1100972Z",
  "refreshToken": "xoJuoFWBRSZm1HIhIzVL3eWYfTFRdt832Db0rJUgdyM",
  "refreshTokenExpiresAt": "2026-08-30T20:15:04.1546699Z"
}
```

### Using a token

Include the `accessToken` in the `Authorization` header of subsequent requests as a Bearer token.

```json
Authorization: Bearer <your-jwt-token>
```

> Access Tokens are short-lived (about 15 minutes), after that all permissions are revoked and you lose access to protected endpoints. When you happens you would need to refresh your session by acquiring a new access token.
> For this, we use the `refreshToken`

Use the **refresh** endpoint under sessions to refresh your access token.
**Endpoint:** `POST /api/v1/auth/sessions/refresh`

**Payload**

```json
{
"refreshToken": "xoJuoFWBRSZm1HIhIzVL3eWYfTFRdt832Db0rJUgdyM"
}
```

**Response**: This then returns a new `accessToken` + `refreshToken` pair. And the cycle repeats itself again. Refresh tokens have a life span of 30 days before they expire

```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwMTlmYjQ2OC05YWRmLTc3OGMtODAzZC1mNGZiYWUwMzYxN2EiLCJqdGkiOiIxOWQ3ZGNlMi0yNzlkLTQ3ODctODAyYS02YWE0ZTk5NjdmZTIiLCJyb2xlIjoiQWRtaW4iLCJjb25ncmVnYXRpb25JZCI6IjAxOWZiNDY4LTlhZGMtN2RjNy1iNmVlLWMwMDUwOTlkMWYzOSIsImV4cCI6MTc4NTUzMDUwOCwiaXNzIjoiZWNoby1hcGkiLCJhdWQiOiJlY2hvLWNsaWVudHMifQ.NaZIl1sGMHlooFbkLBfn0ryj1FfUZjnas5pfE_lHScBlcE6YJDaXZZLsIDESMziwLc_wwLdhsEGpEypqEIu0YWSHTjXTHsva5-2m4yC3YoUhL4bIgxBvMouileiSBpzXGUZJZ2Vz0fqQBCiWPuINGP-FRjifWhfQReH36Nq33EUKEMH1tdliLLBVRSLke5Mi_8ofyo7mLZQyWFlSFfDGB_GVoaBmcF1CXt9PWi-pOD8cXNWPH0_iDWaXHHWg5ANkRDXj4IWCEKoSZh54Qzg_njc98-yySUCgu5hunCILK88cW9Vi3VyMMVszwj8DVDze5hO-TweLLTbriRBca_g0QQ",
  "accessTokenExpiresAt": "2026-07-31T20:41:48.492284Z",
  "refreshToken": "87OPpXTKLehJxJSMvigHAteEIr-IGxj7da5VpzkYZVg",
  "refreshTokenExpiresAt": "2026-08-30T20:26:48.4576525Z"
}
```

**NB**: All authenticated requests must include a valid token. The token includes encrypted details about the user, the resources they can access and their congregation, which is used used for tenant isolation.

---

## Common Endpoints

The API is organised by domain. Each domain has a dedicated controller that exposes standard CRUD and summary operations. Below is an example with the `Events` domain. The same pattern applies to `Members`, `Assets`, `Attendance`, and other domain entities.

**Events Controller**
**Base route:** `/api/v1/events`

| Method   | Endpoint                 | Description                                                |
| -------- | ------------------------ | ---------------------------------------------------------- |
| `GET`    | `/api/v1/events/summary` | Get a summary of events                                    |
| `GET`    | `/api/v1/events`         | Get a paginated list of events (supports query parameters) |
| `GET`    | `/api/v1/events/{id}`    | Get a single event by ID                                   |
| `POST`   | `/api/v1/events`         | Create a new event                                         |
| `PUT`    | `/api/v1/events/{id}`    | Update an existing event                                   |
| `DELETE` | `/api/v1/events/{id}`    | Delete an event                                            |

**Other Controllers**
Other core domains follow the same pattern. Controllers are located in `Echo.Core/Controllers/` some of which are:

- `AssetCategoriesController`
- `AssetsController`
- `AttendanceContextsController`
- `AttendanceController`
- `AttendanceTypesController`
- `EventAttendanceController`
- `EventRegistrationsController`
- and many others

Auth-related endpoints (login, registration, password reset, invitations) are in `Echo.Auth/Controllers/`.

---

## Using the API

**1. Get a token**
Send a `POST` request to the login endpoint (e.g., `/api/v1/sessions`) with your credentials. The response will include an access token (and likely a refresh token).

> **New developer?** See [Getting Started](./GettingStarted.md) for a step-by-step guide to creating your first credentials.

**2. Call an endpoint**
Include the token in the `Authorization` header. For example, to get a list of events:

``` bash
curl -H "Authorization: Bearer <your-token>" \
     "http://localhost:8080/api/v1/events?page=1&pageSize=20"
```

**3. Pagination & Filtering**
List endpoints support pagination and query filtering via query parameters. The exact parameters are defined in `PaginationParameters` and `QueryParameters`.

**4. Health Check**
To verify the API is running, use the health endpoint:

```bash
curl http://localhost:8080/health/ready
```

This endpoint is public and is available without authentication.

### Interactive API Documentation (Scalar)

When running in development mode, Scalar/OpenAPI documentation is available at:

```markdown
http://localhost:5025/scalar
```

or through Nginx at:

```markdown
http://localhost:8080/scalar
```

This URLs provides an interactive UI to explore and test all endpoints.

---

## Error Handling

All API responses follow a consistent format using HTTP result wrappers (defined in `Echo.Application`). Successful responses return the requested data; errors return appropriate HTTP status codes with a structured error message.

---

## Related Documentation

- [Setup](./../Setup.md): Local development setup
- [Infrastructure](./../Infrastructure.md): Docker, containers, reverse proxy and environment variables
- [Front-end](./../client_app/README.md): Front-end documentation
