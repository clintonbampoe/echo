# Echo backend

A modular monolith on ASP.NET Core 10 with minimal APIs. The solution is
`backend/Echo.slnx`; the projects are in `backend/src/` and their tests in
`backend/tests/`. What each project is for, and how a feature is laid out, is in
[Backend architecture](../docs/backend/architecture.md).

| Project | Role |
|---|---|
| `Echo.Domain` | entities, enums, `AppDbContext`; depends on nothing |
| `Echo.Data` | EF migrations, `IUnitOfWork`, health checks |
| `Echo.Shared` | results, pagination, hashing, email, options validation |
| `Echo.Application` | the feature modules: endpoints, services, repositories |
| `Echo.Auth` | sessions, registration, invitations, passwords, verification |
| `Echo.Api` | composition root: `Program.cs` |
| `Echo.MigrationService`, `Echo.Seeder` | the migrator and the fake-data seeder |
| `Echo.AppHost`, `Echo.ServiceDefaults` | .NET Aspire host and shared defaults |

## Run it

Create `.env` at the repository root, not in `backend/`
([Getting started](../docs/getting-started.md)). Then:

```bash
dotnet build backend/Echo.slnx
dotnet run --project backend/src/Echo.Api
dotnet test --solution backend/Echo.slnx --filter "Category=Unit"
```

The API reference (Scalar) is at `/scalar` and Swagger at `/swagger` in
development.

## Migrations

[Migrations](../docs/how-to/migrations.md) covers adding one and how the
migrator applies them in production.
