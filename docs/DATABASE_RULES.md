# Database Rules

This file defines the database rules for BusPlanen/BusRejser.

The goal is simple: avoid schema drift between Entity Framework migrations and MySQL.

## Core rule

Schema changes are handled by EF Core migrations.

Data changes are handled by seeders, importers, or controlled SQL scripts.

Do not mix those two responsibilities.

## Schema

Use EF Core migrations for:

- New tables
- New columns
- Renamed columns
- Foreign keys
- Indexes
- Required/nullable changes
- Entity relationship changes

Allowed workflow:

```bash
dotnet ef migrations add DescriptiveMigrationName
dotnet ef database update
```

If multiple projects are involved:

```bash
dotnet ef migrations add DescriptiveMigrationName --project BusRejserLibrary --startup-project BusRejserAPI
dotnet ef database update --project BusRejserLibrary --startup-project BusRejserAPI
```

## Data

Use seeders/importers/scripts for:

- Badges
- Dummy trips
- GeoNames places
- Geo alternate names
- Test users
- Demo data

Examples:

- `GeoImporter`
- `GeoAlternateNameImporter`
- Badge seeder
- Dev data scripts

## Forbidden

Do not do this unless there is a very specific emergency reason:

- Manually `CREATE TABLE` in DBeaver/MySQL
- Manually `ALTER TABLE` in DBeaver/MySQL
- Delete migration files after they have been shared or applied
- Create a new `InitialCreate` on top of an existing database
- Use `EnsureCreated()` together with migrations
- Run migrations against one database while the app points at another database
- Patch production schema manually without an approved migration script

## Dev reset

For local development only, a full reset is allowed when the database is already inconsistent.

Preferred reset flow:

```sql
DROP DATABASE busplanen;
CREATE DATABASE busplanen;
```

Then:

```bash
dotnet ef database update --project BusRejserLibrary --startup-project BusRejserAPI
```

Then run seed/import steps:

```txt
GeoImporter
GeoAlternateNameImporter
Badge seeder
Dummy/test data scripts
```

Do not drop single tables one by one unless you are intentionally fixing a known foreign key chain.

## Production

Never drop the production database.

Never use local reset logic in production.

Production migration flow:

```bash
dotnet ef migrations script --idempotent -o migration.sql --project BusRejserLibrary --startup-project BusRejserAPI
```

Then review the generated SQL before running it on production.

Production rules:

- Always take backup before schema changes
- Review generated SQL
- Prefer idempotent migration scripts
- Keep schema changes small
- Do not mix large data imports with schema migrations
- Run geo imports and badge seeders separately from schema migrations

## Mental model

```txt
Migration = structure
Seeder/importer = data
DBeaver = inspect/backup, not schema design
```

If the database and migrations disagree, stop and inspect `__EFMigrationsHistory` before changing anything.
