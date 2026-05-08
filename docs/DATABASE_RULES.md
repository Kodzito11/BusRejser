# Dansk
# Database Regler

Denne fil definerer database-reglerne for BusPlanen/BusRejser.

Målet er simpelt: undgå schema drift mellem Entity Framework migrations og MySQL.

## Kerne-regel

Schemaændringer håndteres gennem EF Core migrations.

Data håndteres gennem seeders, importers eller kontrollerede SQL-scripts.

De to ansvar må ikke blandes sammen.

## Schema

Brug EF Core migrations til:

- Nye tabeller
- Nye kolonner
- Omdøbte kolonner
- Foreign keys
- Indexes
- Required/nullable ændringer
- Ændringer i relationer mellem entities

Tilladt workflow:

```bash
dotnet ef migrations add BeskrivendeMigrationNavn
dotnet ef database update
```

Hvis flere projekter er involveret:

```bash
dotnet ef migrations add BeskrivendeMigrationNavn --project BusRejserLibrary --startup-project BusRejserAPI
dotnet ef database update --project BusRejserLibrary --startup-project BusRejserAPI
```

## Data

Brug seeders/importers/scripts til:

- Badges
- Dummy-rejser
- GeoNames steder
- Geo alternate names
- Testbrugere
- Demo-data

Eksempler:

- `GeoImporter`
- `GeoAlternateNameImporter`
- Badge seeder
- Dev data scripts

## Forbudt

Undgå dette medmindre der er en meget specifik nødsituation:

- Manuelt `CREATE TABLE` i DBeaver/MySQL
- Manuelt `ALTER TABLE` i DBeaver/MySQL
- Slette migrations-filer efter de er delt eller kørt
- Oprette en ny `InitialCreate` ovenpå en eksisterende database
- Bruge `EnsureCreated()` sammen med migrations
- Køre migrations mod én database mens appen peger på en anden database
- Patche production-schema manuelt uden godkendt migration-script

## Dev reset

Kun til lokal udvikling er et fuldt reset tilladt, hvis databasen allerede er inkonsistent.

Foretrukket reset-flow:

```sql
DROP DATABASE busplanen;
CREATE DATABASE busplanen;
```

Derefter:

```bash
dotnet ef database update --project BusRejserLibrary --startup-project BusRejserAPI
```

Kør derefter seed/import-trin:

```txt
GeoImporter
GeoAlternateNameImporter
Badge seeder
Dummy/test data scripts
```

Undgå at droppe enkelte tabeller én efter én, medmindre du bevidst håndterer en kendt foreign key-kæde.

## Production

Drop aldrig production-databasen.

Brug aldrig lokal reset-logik i production.

Production migration-flow:

```bash
dotnet ef migrations script --idempotent -o migration.sql --project BusRejserLibrary --startup-project BusRejserAPI
```

Gennemgå altid den genererede SQL før den køres mod production.

Production-regler:

- Tag altid backup før schemaændringer
- Gennemgå genereret SQL
- Foretræk idempotent migration scripts
- Hold schemaændringer små
- Bland ikke store data-importer med schema migrations
- Kør geo-importer og badge-seeders separat fra schema migrations

## Mental model

```txt
Migration = struktur
Seeder/importer = data
DBeaver = inspektion/backup, ikke schema-design
```

Hvis databasen og migrations er uenige, så stop og undersøg `__EFMigrationsHistory` før noget ændres.

# English
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
