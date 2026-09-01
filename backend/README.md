# Echo Backend

## Stucture

The backend is built with a fusion of clean architecture and a modular monolith.
It has shared modules (domain, core, shared) that host the core of the app. After that, there are self contained pluggable modules that add more functionality and features to the backend.

The direction of direction flows inwards from outward. Hence, the lower level modules have no idea about the high level code that depend on them.

| Module | Dependencies |
| --- | --- |
| Echo.Domain | none |
| Echo.Infrastructure | Echo.Domain |
| Echo.Shared | Echo.Domain |
| Echo.Core | Echo.Domain, Echo.Shared |
| Echo.Auth | Echo.Domain, Echo.Shared, Echo.Core |
| Echo.Api | depends on all projects and modules |

## Modules

- **Echo.Domain** : Hold Entities, EF Configuration, DbContext and core domain entities
- **Echo.Infrastructure** : Contains database migrations and the design-time DbContext factory to build the database context at runtime
- **Echo.Shared** : Contains module agnostic shared assets across the code base base. Example: HTTP result wrappers, pagination, query filters, the `BaseController` class, etc.
- **Echo.Core** : Contains controllers, services, repositories, DTOs, mapping and extensions for the core domain entities (`User`, `Member`, `Congregation`, etc.)
- **Echo.Auth** : Contains services and logic for auth flows.
- **Echo.Api** : This is the composition root. Holds `Program.cs`, the entry point of the backend. Handles DI wiring, configurations, etc.

### Module Structure

Each module follows the same internal layout:

```mardown
Controllers/
Services/
Repositories/
Dtos/
Mapping/
Extensions/         <- DI registration and other extension methods
```

New modules reference `Echo.Domain` (+ `Echo.Shared`, and `Echo.Core` if they need core operations) and register their services via one extension method called from `Echo.Api/Program.cs`.
When in doubt look at the example in [`Echo.Core/Extensions/CoreServiceExtensions`.](/backend/src/Echo.Core/Extensions/CoreServiceExtensions.cs)

## Running Locally

1. Retrieve api keys and credential from an authorized code owner or admin. Then create a `.env` at the repo root (not `backend/`), fill in the credentials and keys given to you.
2. run

```bash
dotnet build backend/Echo.slnx
dotnet run --project backend/src/Echo.Api
```

Swagger/OpenApi is available at `/swagger` in development.

## Migrations

Migrations live in `Echo.Infrastructure`, not `Echo.Domain`. Run all EF commands with `--project` pointing to the infra module and `--startup-project` pointing at the program entry point (Echo.Api):

Run this command at the repository root to:

- Add New Migrations

```bash
dotnet ef migrations add "MIGRATION_NAME" --project backend/src/Echo.Infrastructure --startup-project backend/src/Echo.Api
```

- Update Database

```bash
dotnet ef database update --project backend/src/Echo.Infrastructure --startup-project backend/src/Echo.Api
```

## Config

The connection string is a template in `appsettings.json` (`DefaultConnection`), with `__DB_USER__` and `__DB_PASSWORD__`  placeholders substituted at startup from the .env in the repository.
The `.env` file must live at the git repo root as the `EnvLoader` method walks up from the running assembly to find it.
If there is no .env file at the repo root, the application will throw an error and gracefully close.
