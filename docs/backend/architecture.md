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
| `AddSharedServices()` | `Echo.Shared` | `TimeProvider`, `IIdGenerator`, `ITokenGenerator`, `IPasswordHasher` (Bcrypt), `ITokenHasher` (Sha256), `IEncoder` (Cursor), keyed `IEmailService` (Resend), `FrontendOptions` validation |

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

- **Keyed services**: `IEmailService` keyed "Resend" (for `ResendEmailService`). Hashing uses `IPasswordHasher` (Bcrypt) + `ITokenHasher` (Sha256).

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