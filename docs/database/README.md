# Setting Up Local Database

Quick reference for setting up, generating and running database migrations locally.
*(basically, everything you'd need to set up the database with the least amount of work possible #GodBlessCsharp)*

## 1. Local Environment Setup

- Create a local PostgreSQL database named `echo_db` and note your login credentials.

- To initialize and store your local database credentials safely outside of source control (Git), Run the follow commands in your terminal from the project folder `src/(Backend.Api.Core)` and replace `your_postres_username` and `your_postgres_password` with your local db username and password.

```bash
dotnet user-secrets set "Database:Username" "your_postgres_username"
dotnet user-secrets set "Database:Password" "your_posgtres_password"
```

- Ensure the global Entity Framework command-line tool is installed and up to date by running this script in your terminal:

```bash
dotnet tool install --global dotnet-ef || dotnet tool update --global dotnet-ef
```

## 2. Run Database Migration

- Navigate to the project directory (`src/Backend.Api.Core`) in your terminal before running these commands.

- Since the database history is kept as code in the `src/Backend.Api.Core/Migrations` directory, pull the latest changes from Git to get the newest migration files.

- Apply the schema changes directly to your local database by running:

```bash
dotnet ef database update
```

- EF Core and C# would automatically track the differences between your local database and the latest migration. It would then apply the latest changes to the schema on top of your local db.
