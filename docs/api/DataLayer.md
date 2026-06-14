# Data Layer

## AppDbContext

`AppDbContext` is the EF Core database context. It defines all the tables and applies configuration.

Two global conventions apply to every entity automatically:

- All `enum` properties are stored as **strings** (not integers)
- All `string` properties have a **max length of 255 characters**

Entity configurations are loaded automatically from the assembly — any class implementing `IEntityTypeConfiguration<T>` in the project is picked up without needing to be registered manually.

## Entity Configurations

Each entity has a dedicated configuration file under `Data/EntityConfigurations/`. These define:

- Primary keys
- Foreign key relationships
- Indexes
- Computed columns (e.g. the `Name` field on `Member` is computed from `FirstName + LastName`)

`CongregationEntityConfigurationBase` is a shared base configuration applied to all entities that extend `ICongregationEntity`. It sets up the `CongregationId` foreign key on every such entity automatically.

## Migrations

Database migrations are in the `Migrations/` folder and are managed with EF Core tools.

To apply all pending migrations:

```bash
dotnet ef database update
```

To add a new migration after changing an entity:

```bash
dotnet ef migrations add <MigrationName>
```
