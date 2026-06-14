# Setup & Configuration

## Prerequisites

- .NET 10 SDK
- PostgreSQL running locally

## Database Credentials

The connection string template is in `appsettings.json`:

```
Host=localhost;Port=5432;Database=echo_db;Username={0};Password={1}
```

The `{0}` and `{1}` placeholders are filled at runtime using:

- `Database:Username`
- `Database:Password`

These must be set via **user secrets** — they are not committed to source control. See `README_SECRETS.md` for details.

```bash
dotnet user-secrets set "Database:Username" "your_db_username"
dotnet user-secrets set "Database:Password" "your_db_password"
```

## Running the API

```bash
# Apply database migrations
dotnet ef database update

# Start the API
dotnet run
```

Swagger UI is available at `/swagger` when running in development mode.
