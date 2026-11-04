# Backend Architecture

**Written by:** @clintonbampoe
**Last updated:** 2026-10-22 by @clintonbampoe

---

Echo is a **modular monolith** built on **Clean Architecture** principles with ASP.NET Core 10.

---

## Solution Structure

Solution file: `backend/Echo.slnx`. Projects under `backend/src/`:

| Project | Dependencies | Role |
|---|---|---|
| `Echo.Domain` | none | Entities, EF entity configurations, enums, `AppDbContext`, domain interfaces. Pure domain — no outward deps. |
| `Echo.Data` | `Echo.Domain` | EF migrations, design-time `DbContextFactory`, health checks, `IUnitOfWork`. |
| `Echo.Shared` | `Echo.Domain` | Cross-cutting: HTTP results (`IOperationResult`), pagination, query, cursor encoding, shared services (hashing, email, generators, encoders), options validation. |
| `Echo.Application` | `Echo.Domain`, `Echo.Shared` | **Feature modules** — all domain logic organized by feature (Assets, Attendances, Congregations, Events, Members, Organizations, Projects, Tithes, Transactions, Users). Each feature has: DTOs, Endpoints, Repository, Service, Mapper, Log. Cross-cutting: `ApplicationInstrumentation`, `SummaryQueries`. |
| `Echo.Auth` | `Echo.Domain`, `Echo.Shared`, `Echo.Application` | Auth flows: Sessions (JWT), Registrations, Invitations, Passwords, Email Verifications, Profile. |
| `Echo.Api` | all | **Composition root** — `Program.cs`. Wires DI, OpenAPI, Swagger/Scalar, JWT, rate limiting, versioning, CORS, health checks, pipeline. |

> **Additional projects (Aspire integration):** `Echo.AppHost`, `Echo.ServiceDefaults`, `Echo.MigrationService`, `Echo.Seeder`.

---

## Module Layout (per feature in `Echo.Application`)

```
Assets/
  AssetDtos.cs
  AssetEndpoints.cs
  AssetLog.cs
  AssetMapper.cs
  AssetRepository.cs
  AssetService.cs
  IAssetMapper.cs
  IAssetCategoryMapper.cs
  ... (same pattern per feature)
```

**Every feature has:**
- `*Dtos.cs` — Request/Response DTOs, filters, cursors
- `*Endpoints.cs` — Minimal API endpoint definitions (`Map*Endpoints`)
- `*Log.cs` — Structured logging (source-generated `LoggerMessage`)
- `*Mapper.cs` — `I*Mapper` interface + implementation (Entity ↔ DTO)
- `*Repository.cs` — Data access (EF Core, `AsNoTracking`, `FilterDeleted`, pagination, cursor)
- `*Service.cs` — Business logic, returns `IOperationResult` / `IOperationResultGeneric<T>`
- `I*Mapper.cs` — Mapper interface for DI

**Cross-cutting in `Echo.Application`:**
- `ApplicationInstrumentation` — OpenTelemetry activity source, metrics
- `SummaryQueries` — Aggregated stats across features
- `ReassignRules.cs` — Cross-entity reassignment logic
- `Extensions.cs` — `AddApplicationServices()`, `MapApplicationEndpoints()`

---

## Extension Methods (called from `Echo.Api/Program.cs`)

| Method | Project | Registers |
|---|---|---|
| `AddApplicationServices()` | `Echo.Application` | All feature repositories, services, mappers, `ApplicationInstrumentation`, OpenTelemetry |
| `AddAuthServices(config)` | `Echo.Auth` | Auth services (Session, Registration, Invitation, Password, Verification, Profile), `JwtTokenGenerator`, `LinkBuilder`, `Resend`, `AuthInstrumentation` |
| `AddDataServices()` | `Echo.Data` | `DbHealthCheck`, `IUnitOfWork` |
| `AddSharedServices()` | `Echo.Shared` | `TimeProvider`, `IIdGenerator`, `ITokenGenerator`, `IPasswordHasher` (Bcrypt), `ITokenHasher` (Sha256), `IEncoder` (Cursor), `IEmailService` (Resend), `IEmailQueue` + `EmailDeliveryWorker` (background email sending), `FrontendOptions` validation |

---

## Cross-cutting Patterns

- **Multi-tenant by `CongregationId`**: Every primary entity implements `IPrimaryEntity` (has `CongregationId`). Services take `Guid congregationId` parameter. Extracted via `ClaimsPrincipal.GetCongregationId()`. Routes versioned: `/api/v{version:apiVersion}/[feature]`.

- **Repository pattern**: Each feature has a repository (`*Repository.cs`) taking `AppDbContext`. Uses `AsNoTracking()` for reads, `FilterDeleted()` (soft-delete), cursor-based pagination via `Echo.Shared.Query` extensions.

- **Service pattern**: Each feature has a service (`*Service.cs`) taking repository, `IUnitOfWork`, mapper, `IIdGenerator`, `ApplicationInstrumentation`, `LinkGenerator`, `DateWindows`, logger. Returns `IOperationResult` / `IOperationResultGeneric<T>`.

- **HTTP results** (`Echo.Shared/HttpResults`): Not raw types. Controllers/endpoints return:
  - `SuccessResult<T>`, `CreatedResult<T>`, `NotFoundResult`, `ConflictResult`, `BadRequestResult`, `InternalServerError`, `NoContentResult`, `ForeignKeyEntityNotFound`, `InvalidTokenResult`.
  - `PagedResponse<T>` for cursor-paged lists (hasMore, nextCursor, data, window).

- **Query & pagination** (`Echo.Shared/Pagination`, `Echo.Shared/Query`): `PaginationRequest` (cursor, pageSize), `PaginationParams`, `PagedResponse<T>`. Cursor encoding via `IEncoder` (base64 URL-safe).

- **AutoMapper**: Not used. Manual mappers per feature (`I*Mapper` interface + implementation) for explicit control.

- **DI lifetimes**: All repositories, services, `AppDbContext`, `IUnitOfWork` are `AddScoped`. Mappers are `AddSingleton`. Instrumentation, generators, encoders are `AddSingleton`.

- **API versioning**: `AddApiVersioningSetup` → `[ApiVersion(1.0)]` on route group `/api/v{version:apiVersion}`.

- **JWT auth**: `AddJwtAuthentication` → `JwtTokenGenerator` (singleton), `JwtTokenService` (scoped), `SessionService`, `SessionRepository`. Access tokens 15 min, refresh tokens rotated.

- **Auth instrumentation**: each auth service has one span for the operation and records every outcome in one call, `instrumentation.Record(counter, span, result, congregationId?)`, usually through a small private wrapper such as `RecordLogin(span, "success", congregationId)`. It sets `auth.result` on the span and increments the counter with `auth.result` (+ `congregation.id` when given). Never put a user id in a label. Do not wrap a single repository call in its own span: EF Core and Npgsql spans already appear under the operation. One-time token checks (expired / used / invalidated) live in `TokenValidity.Problem`, shared by email verification and password reset.
- **Summary cache**: every summary endpoint goes through `SummaryCache.Serve`, which keeps the answer for the congregation and request for 5 minutes (`CacheOptions.DefaultTtl`, so an answer is at most 5 minutes old). Once an answer is three quarters through its life (3 min 45 s), the next request still gets it at once and starts one background refresh (one per key, across requests), so people who keep using the app do not wait for a rebuild; an answer nobody asks for expires. The refresh runs after the request has ended, so it resolves its own service from a new scope: `Serve` takes the asking service and a `(service, token) => service.ComputeX(...)` lambda, never a lambda over the request's own fields. A failed refresh is logged and the old answer stays until it expires. A cold miss (first request, or after a restart) is still computed by the request that asks.
- **Attendance totals**: the attendance summary, its monthly chart and the sessions summary add up the `attendance_per_session` materialized view (one row per session: total, members, visitors, first-time) instead of counting every check-in. EF Core cannot create a materialized view, so the `AttendancePerSessionView` migration holds the SQL, and `SessionAttendanceTotals` is a keyless entity mapped with `ToView`. `AttendanceTotalsRefresher` (a hosted service) runs `REFRESH MATERIALIZED VIEW CONCURRENTLY` every 5 minutes under a Postgres advisory lock, so only one API instance refreshes and readers are never blocked. A new check-in therefore reaches these summaries up to 5 minutes late, on top of the cache. A summary that filters by person, session, kind or name still reads the check-ins. The live check-in and session screens always read the check-ins. After a bulk load (the seeder does this itself), run `REFRESH MATERIALIZED VIEW attendance_per_session`.
- **Email**: auth code never calls Resend. It publishes an `EmailMessage` (recipient, `IEmailContent`, kind) with `IEmailQueue.TryEnqueue` and returns. `EmailDeliveryWorker` (a `BackgroundService` in `Echo.Shared`) sends them through `IEmailService`, retrying network errors, 429 and 5xx up to 3 times (2 s, 4 s, 8 s back-off) and logging an error if it gives up. The queue is in memory and bounded (1000): a full queue drops the message with a warning, and emails still queued when the API stops are lost. Users can re-request verification or reset links; invitation codes are always returned to the admin. Every email is built as an `EmailPage` (plain text: heading, paragraphs, button, optional rows, steps, callout, code, note) and rendered by the one `EmailLayout` in `Echo.Shared`, which encodes all values and handles dark mode and phones; no email writes its own HTML. Emails carry no images (the "Echo" wordmark is text), so nothing depends on remote images loading or on the app's domain. The emails and what sends them: verification (`email_verification`), password reset (`password_reset`), invitation (`invitation`), password changed (`password_changed`, after a reset or a password change in Profile), `welcome` (sent at sign-up to someone joining with an invitation code) and `set_up_church` (sent at church creation to its admin). The last two go out right after the verification email, and a failure to queue either is logged without failing the sign-up. Hashing uses `IPasswordHasher` (Bcrypt) + `ITokenHasher` (Sha256).

- **Soft deletes**: All primary entities implement `ISoftDeletable` (`DeletedAt`). Repositories use `FilterDeleted()` extension. `IUnitOfWork` wraps `SaveChangesAsync`.

- **Structured logging**: Source-generated `LoggerMessage` in `*Log.cs` files per feature (e.g., `AssetLog`, `MemberLog`).

- **OpenTelemetry**: `ApplicationInstrumentation` + `AuthInstrumentation` configure tracing/metrics per module.

---

## Domain Entities (`Echo.Domain`)

### Core (`Echo.Domain/*`)

| Area | Entities |
|---|---|
| **Assets** | `Asset`, `AssetCategory`, `AssetStatus` |
| **Attendances** | `Attendance`, `Session`, `SessionLabel`, `SessionCategory`, `SessionStatus` |
| **Congregations** | `Congregation`, `CongregationType`, `CongregationDefaults` (lookup seeds) |
| **Events** | `Event`, `EventRegistration`, `EventAttendance` |
| **Members** | `Person` (base), `Member`, `Visitor`, `PersonKind`, `Gender`, `MaritalStatus`, `Region` |
| **Organizations** | `Organization`, `OrganizationMember`, `MemberRole` |
| **Projects** | `Project`, `ProjectCategory`, `ProjectContribution`, `ProjectStatus` |
| **Tithes** | `Tithe`, `MonthOfYear` |
| **Transactions** | `Transaction`, `TransactionCategory`, `TransactionType`, `PaymentMethod` |
| **Users** | `User`, `UserRole` |

### Auth (`Echo.Domain/Auth`)

`RefreshToken`, `EmailVerificationToken`, `InvitationToken`, `PasswordResetToken`

### Interfaces

- `IPrimaryEntity` — `CongregationId` (multi-tenancy)
- `ISoftDeletable` — `DeletedAt` (soft delete)
- `ISearchable` — search support

---

## Skill Routing (dotnet-skills plugin)

Load only the skill that matches the task; don't preload.

| Task | Skill |
|---|---|
| EF Core queries, migrations, N+1, `AsNoTracking` | `efcore-patterns`, `database-performance` |
| New/changed C# code style, records, async | `csharp-coding-standards`, `csharp-nullable-reference-types` |
| DI registration (`Add*Services`) | `microsoft-extensions-dependency-injection` |
| Aspire AppHost / ServiceDefaults | `aspire-configuration`, `aspire-service-defaults` |
| Integration tests with real Postgres | `testcontainers`, `aspire-integration-testing` |
| NuGet changes (Central Package Management) | `package-management` |
| After any code change | `slopwatch` (catch disabled tests, suppressed warnings, empty catches) |