# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Summary

Echo is a **multi-tenant church management platform**. Each congregation is an isolated tenant with members, events, attendance, tithes, finance, assets, and projects. The monorepo has three parts:

- `backend/` — .NET 10 ASP.NET Core modular monolith with PostgreSQL/EF Core
- `frontend/` — React 19 + TypeScript + Vite SPA
- `docs/` — API, DB schema, architecture reference

## Development Workflow (Linear + GitHub Draft PRs)

- All work is tracked in Linear. Branch names follow `prefix/scope-identifier-number-description`, e.g. `feat/frontend-clnt-11-implement-client-app-login-page`. Get the branch name from Linear with `Ctrl+Shift+.` (issue identifiers use a 4-letter prefix like `CLNT-N`).
- Open a **Draft PR** on GitHub from the initial commit to signal work-in-progress and auto-move the Linear card. Use markdown checkboxes for the technical plan and put `Closes {Identifier}-{Number}` at the bottom.
- Check off boxes as you go. Click **Ready for Review** only after unit tests pass.
- **Update technical docs, schemas, and diagrams immediately** when making structural changes.

## Common Commands

All commands run from the repo root unless stated otherwise.

### Backend

```bash
# Build
dotnet build backend/Echo.slnx

# Run (uses .env at repo root for secrets)
dotnet run --project backend/src/Echo.Api
# Swagger/Scalar at /swagger in Development

# EF migrations (project = Infrastructure, startup = Api)
dotnet ef migrations add "MIGRATION_NAME" --project backend/src/Echo.Infrastructure --startup-project backend/src/Echo.Api
dotnet ef database update --project backend/src/Echo.Infrastructure --startup-project backend/src/Echo.Api

# Run a single test project
dotnet test backend/src/Echo.Api/Echo.Api.csproj
```

There is currently no separate test project — `backend/tests/` is empty. Tests are wired through the API csproj and CI uses `dotnet test backend/src/Echo.Api/Echo.Api.csproj --no-build -c Release` (continue-on-error, see `.github/workflows/ci.yml`).

### Frontend

```bash
cd frontend
npm install
npm run dev       # http://localhost:5173
npm run build     # tsc -b && vite build
npm run lint      # ESLint
npm run preview   # serve dist/
```

API base URL is read from `VITE_API_URL` env var, defaulting to `http://localhost:5000/api` (see `frontend/src/services/api.ts`).

### Docker

`docker-compose.yml` defines `api`, `db` (postgres:18), `nginx`, `client`, `backup`, and a `migrator` (profile: `tools`). Override files: `docker-compose.override.yml` (dev), `docker-compose.prod.yml` (prod).

### Env / Secrets

`.env` lives at the repo root. The API's `EnvLoader` walks up from the running assembly to find it — **if `.env` is missing at the repo root, the app throws and exits**. The `DefaultConnection` template in `appsettings.json` uses `__DB_USER__` / `__DB_PASSWORD__` placeholders substituted from `.env` at startup.

## Backend Architecture (Modular Monolith + Clean Architecture)

Solution file: `backend/Echo.slnx`. Projects under `backend/src/`:

| Project | Dependencies | Role |
| --- | --- | --- |
| `Echo.Domain` | none | Entities, EF entity configurations, enums, `AppDbContext`, domain interfaces. Pure domain — no outward deps. |
| `Echo.Infrastructure` | `Echo.Domain` | EF migrations, design-time `DbContextFactory`, health checks. |
| `Echo.Shared` | `Echo.Domain` | (Per `backend/README.md`) module-agnostic shared assets. |
| `Echo.Application` | `Echo.Domain`, `Echo.Shared` | Cross-module services: hashing (Bcrypt, Sha256 keyed), email (Resend keyed), JWT `TokenGenerator`, `LinkBuilder`, `Clock`, `HttpResults`, `Pagination`, `Query`. |
| `Echo.Core` | `Echo.Domain`, `Echo.Shared`, `Echo.Application` | Core domain: controllers, services, repositories, DTOs, AutoMapper profiles. The "feature" module that the rest of the app extends. |
| `Echo.Auth` | `Echo.Domain`, `Echo.Shared`, `Echo.Application` | Auth flows: controllers, services, repositories, DTOs. |
| `Echo.Api` | all | **Composition root** — `Program.cs`. Wires DI, Swagger, JWT, rate limiting, versioning, OpenAPI. |

### Module Layout (applied per feature module)

```
Controllers/
Services/
Repositories/
Dtos/
Mapping/                 (in Core)
Extensions/              <- DI registration
```

Each module exposes **one extension method** called from `Echo.Api/Program.cs` (e.g. `AddCoreServices`, `AddAuthServices`, `AddApplicationServices`, `AddInfrastructureServices`). New modules should reference `Echo.Domain` (+ `Echo.Shared`/`Echo.Application` if needed) and follow `Echo.Core` as the reference example (`backend/src/Echo.Core/Extensions/CoreServiceExtensions.cs`).

### Cross-cutting Patterns

- **Multi-tenant by `CongregationId`**: every primary entity implements `IPrimaryEntity`. Services take a `Guid congregationId` parameter. The base controller `CoreBaseController` extracts it via `User.GetCongregationId()`. Routes are versioned: `/api/v{version:apiVersion}/[controller]`.
- **Base classes** (`Echo.Core/Services/Base/`): `PrimaryServiceBase<T>` for primary entities, `ReferenceServiceBase<T>` for reference data. They wire `PrimaryRepositoryBase<T>` + `AppDbContext` + `IMapper` and return `IOperationResult` (`Echo.Application/HttpResults`).
- **HTTP results** are not raw types — controllers return `IOperationResult` / `IOperationResultGeneric<T>` implementations (`OkResult`, `CreatedAtResult`, `NotFoundResult`, `ConflictResult`, `BadRequestResult`, `InternalServerError`, `SuccessResult`).
- **Query & pagination** are passed as `PaginationParameters` + `QueryParameters` (`Echo.Application/Pagination`, `Echo.Application/Query`). Query helpers live in `Echo.Application/Extensions/QueryMethods/` (currently modified in working tree).
- **AutoMapper** is registered inside `AddCoreServices` and used inside the service base classes.
- **DI lifetimes**: all repos, services, and EF context are `AddScoped`.
- **API versioning**: enabled via `AddApiVersioningSetup`; `[ApiVersion(1.0)]` on `CoreBaseController`.
- **JWT auth** is configured via `AddJwtAuthentication`. `RunMigrationsOnStartup` flag triggers `Database.MigrateAsync()` at startup (used in `docker-compose.yml`'s `migrator` profile).
- **Keyed services** are used for pluggable implementations: `IHashService` (Bcrypt/Sha256), `IEmailService` (Resend).

### Domain Entities (`Echo.Domain/Entities/Core`)

`User`, `Congregation`, `Organization`/`OrganizationMember`, `Member`, `Event`/`EventRegistration`/`EventAttendance`, `Attendance`/`AttendanceContext`/`AttendanceType`, `Tithe`, `Transaction`/`TransactionCategory`, `Asset`/`AssetCategory`, `Project`/`ProjectCategory`/`ProjectContribution`. Auth entities live under `Echo.Domain/Entities/Auth`.

## Frontend Architecture

Entry: `frontend/src/main.tsx` → `App.tsx`. Top-level state via React Context (`AuthContext`, `LayoutContext`) wrapped by `App`. Single-page tabbed UI (Sidebar + Topbar + page content). Tabs: Dashboard, Finance, Attendance, Tithe, Projects, Contributions, Events, Members.

```
frontend/src/
  components/         Page-level + presentational components (Dashboard, Finance, ...)
    common/           Shared UI (DeleteConfirmModal, etc.)
  context/            AuthContext, LayoutContext
  services/           api.ts (apiFetch wrapper), titheService, attendanceService
  types/              Domain TypeScript types
  utils/              exportUtils
  styles/             CSS
  assets/             Static assets
```

`api.ts` centralizes fetch with `Content-Type: application/json` and surfaces `errorData.message` on non-2xx responses.

## CI

`.github/workflows/ci.yml` runs on push/PR to `main` for paths under `backend/**`: restore → build → test against `dotnet 10.0.x`. Dependabot is configured in `.github/dependabot.yml`. No CI for `frontend/**` yet.

## Conventions

- Follow existing architectural conventions and naming patterns.
- Keep logic decoupled — business rules, UI, and data access stay separate.
- Write unit tests for all new logic (preventing regressions).
- Connection-string template in `appsettings.json` uses double-underscore placeholders for `.env` substitution.
