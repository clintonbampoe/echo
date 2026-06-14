# Overview

Echo is a church management system.
This back-end API handles members, attendance, tithes, transactions, events, projects, assets, and organizations.
These are all scoped to a specific congregation.

Built with **ASP.NET Core (.NET 10)**, **Entity Framework Core**, and **PostgreSQL**
## Tech Stack

| Concern        | Tool                                     |
| -------------- | ---------------------------------------- |
| Framework      | ASP.NET Core 10                          |
| Database       | PostgreSQL (via Npgsql EF Core provider) |
| ORM            | Entity Framework Core 10                 |
| Object mapping | AutoMapper                               |
| API docs       | Swagger / OpenAPI                        |

## Project Structure

```
Backend.Api.Core/
├── Controllers/          # HTTP endpoints
├── Services/             # Business logic
├── Repositories/         # Database access
├── Entities/             # Database models
├── Dtos/                 # Request and response shapes
├── Data/                 # DbContext and EF configurations
├── Common/               # Shared utilities (pagination, filtering, HTTP result types)
├── Enums/                # Shared enum definitions
├── Migrations/           # EF Core database migrations
└── Program.cs            # App entry point and dependency registration
```

## How a Request Flows

Every request follows the same path:

```
HTTP Request → Controller → Service → Repository → Database
												      ↓
HTTP Response ← Controller ← Service ← Repository ←──┘
```

1. The **Controller** receives the HTTP request and passes it to the relevant **Service**.
2. The **Service** handles the logic — checking if a record exists, building responses, etc.
3. The **Repository** talks to the database.
4. The result comes back as an `IOperationResult`, which the controller turns into an HTTP response.
