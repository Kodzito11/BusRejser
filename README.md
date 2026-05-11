# Dansk

# BusPlanen Backend (API)

Backend API for BusPlanen. Håndterer rejser, bookinger, brugere, Stripe-betaling, travel history, badges og progression.

---

## Features

- REST API for rejser, bookinger, brugere og busser
- JWT authentication med roller
- Stripe checkout og webhook-integration
- Seat reservation uden overselling
- Travel history
- Badge system
- Progression map
- Territory progression
- Municipality progression
- Quest progression
- GeoNames-baseret geo-normalisering
- Global exception handling
- Structured logging med Serilog
- Correlation ID per request

---

## Arkitektur

```text
Controller -> Service -> Repository -> Database
```

- Services indeholder business logic
- Repositories håndterer databaseadgang
- Controllers er tynde

Feature-strukturen følger cirka:

```text
BusRejser/
  Features/
    Auth/
    Bookings/
    Buses/
    Rejser/
    Payments/
    Progression/
    Badges/
    TravelHistory/
    Geo/
    Facilities/

  Common/
    DTOs/
    Security/
```

---

## Booking flow

```text
Frontend -> Stripe Checkout -> Webhook -> BookingService -> DB
```

- Booking oprettes kun efter verificeret betaling
- Webhook-flowet er gjort idempotent
- Checkout-status er read-only

---

## Progression og gamification

Backend håndterer brugerens rejseprogression gennem completed bookings.

Flowet er:

```text
Completed booking
-> SyncUserProgress
-> TravelHistory
-> VisitedLocation
-> Progression map
-> Badges / Quests
```

Progression består af:

- `TravelHistory` for gennemførte rejser
- `VisitedLocation` for aggregerede lokationer
- `ProgressionMapResponse` til frontend map/dashboard
- `TerritoryProgressResponse` til lande/territories
- `MunicipalityProgressResponse` til kommuner
- `QuestProgressResponse` til computed quests

Quests er v1 computed-only:

```text
VisitedLocation + Territories + Municipalities
-> QuestProgressService
-> QuestProgressResponse
```

Det betyder:

- ingen quest database endnu
- ingen quest migrations endnu
- backend ejer quest progress
- frontend renderer kun quest data

Geo-normalisering bruges til at oversætte destinationer til country, region, municipality og koordinater.

---

## Progression endpoints

```text
GET /api/Progression/map
GET /api/Progression/quests
```

`/api/Progression/map` bruges af frontend progression dashboard og kortet.

`/api/Progression/quests` returnerer brugerens computed quest progression.

---

## Badge flow

Badges evalueres backend-side som del af progression sync.

```text
SyncUserProgress
-> BadgeEngine
-> UserBadges
```

Badges er persistent data, mens quests i v1 er computed data.

---

## Krav

- .NET 8 SDK
- MySQL 8
- Stripe test keys

---

## Lokal setup uden committed secrets

Projektet er sat op til at bruge denne model:

1. `appsettings.json` til sikre defaults og struktur
2. `appsettings.Development.json` som lokal, ignoreret dev-fil
3. `dotnet user-secrets` eller environment variables til rigtige secrets
4. `BusRejser/appsettings.Example.json` som delt reference for den fulde shape

Det betyder i praksis:

- repoet viser hvilke felter der findes
- `appsettings.Development.json` må gerne indeholde lokale dev-værdier, fordi den er ignoreret
- production og CI må ikke bygge på lokale filer med secrets
- user-secrets eller environment variables er stadig den foretrukne vej til rigtige hemmeligheder

`BusRejser/appsettings.Example.json` kan kopieres som udgangspunkt, hvis du vil have en lokal dev-fil.

```powershell
Copy-Item .\BusRejser\appsettings.Example.json .\BusRejser\appsettings.Development.json
```

### Foretrukken model: Initialiser user-secrets

Kør fra repo-roden:

```powershell
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "ConnectionStrings:DefaultConnection" "server=localhost;port=3307;database=busplanen;user=bususer;password=replace-me;"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Jwt:Secret" "replace-with-at-least-32-characters"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Jwt:Issuer" "BusPlanen.Api"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Jwt:Audience" "BusPlanen.Client"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Stripe:SecretKey" "replace-with-stripe-secret-key"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Stripe:WebhookSecret" "replace-with-stripe-webhook-secret"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:Host" "sandbox.smtp.mailtrap.io"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:Port" "587"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:Username" "replace-with-email-username"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:Password" "replace-with-email-password"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:From" "noreply@example.com"
```

### Alternativt: environment variables

ASP.NET Core mapper `__` til `:`.

```powershell
$env:ConnectionStrings__DefaultConnection="server=localhost;port=3307;database=busplanen;user=bususer;password=replace-me;"
$env:Jwt__Secret="replace-with-at-least-32-characters"
$env:Jwt__Issuer="BusPlanen.Api"
$env:Jwt__Audience="BusPlanen.Client"
$env:Stripe__SecretKey="replace-with-stripe-secret-key"
$env:Stripe__WebhookSecret="replace-with-stripe-webhook-secret"
$env:Email__Host="sandbox.smtp.mailtrap.io"
$env:Email__Port="587"
$env:Email__Username="replace-with-email-username"
$env:Email__Password="replace-with-email-password"
$env:Email__From="noreply@example.com"
```

---

## CORS og frontend-config

Følgende felter kan sættes i din lokale, ignorerede `appsettings.Development.json`:

- `Cors:AllowedOrigins`
- `Frontend:BaseUrl`
- `Frontend:PaymentSuccessPath`
- `Frontend:PaymentCancelPath`
- `Frontend:PasswordResetPath`
- `Auth:RefreshTokenLifetimeDays`
- `Auth:RequireConfirmedEmail`

Du kan også lægge lokale dev-secrets der, hvis du bevidst vælger den model. Det vigtige er, at filen ikke trackes i git.

Disse bruges til trusted frontend-origin og Stripe redirects.

---

## Rate limiting

Følsomme endpoints er beskyttet med ASP.NET Cores indbyggede rate limiter.

Beskyttede endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/stripe/create-checkout-session`
- `GET /api/stripe/checkout-status`

Limits styres via `RateLimiting`-sektionen i config og kan sættes forskelligt i development og production.

Webhook-endpointet er ikke rate limited, så Stripe ikke bliver blokeret af legitime retries.

---

## Database med Docker Compose

`docker-compose.yml` bruger environment variables i stedet for hårdkodede passwords.

1. Kopier `.env.example` til `.env`
2. Sæt dine egne værdier
3. Start databasen:

```powershell
docker compose up -d
```

---

## Database og migrations

Database-schemaændringer skal håndteres gennem EF Core migrations.

Data som badges, GeoNames-data, demo-rejser og testbrugere skal håndteres gennem seeders, importers eller kontrollerede scripts.

Undgå manuelt at oprette eller ændre tabeller direkte i MySQL/DBeaver, medmindre der er en specifik nødsituation.

De fulde regler er dokumenteret i:

```text
docs/DATABASE_RULES.md
```

Mental model:

```text
Migration = struktur
Seeder/importer = data
DBeaver = inspektion/backup, ikke schema-design
```

---

## Run

```powershell
dotnet run --project .\BusRejser\BusRejser.csproj
```

Swagger er tilgængelig i development:

```text
/swagger
```

---

## Startup validation

Applikationen failer ved startup hvis kritisk config mangler eller er ugyldig for:

- database connection string
- JWT secret, issuer og audience
- refresh token lifetime og auth policy
- Stripe secret og webhook secret
- email host/credentials/from address
- trusted CORS origins
- trusted frontend base URL

---

## Tests

Kør tests:

```powershell
dotnet test .\BusPlanen.Tests\BusPlanen.Tests.csproj
```

---

## Status

Backenden er under hardening frem mod deployment.

Nuværende status:

- Auth flow er strammet op
- Stripe flow virker
- Booking flow er idempotent
- Config validation er tilføjet
- Rate limiting er tilføjet
- Database-regler er dokumenteret
- Progression map virker
- Travel history virker
- Badge system virker
- Quest progression er computed-only v1

---

# English

# BusPlanen Backend (API)

Backend API for BusPlanen. Handles trips, bookings, users, Stripe payments, travel history, badges, and progression.

---

## Features

- REST API for trips, bookings, users, and buses
- JWT authentication with roles
- Stripe checkout and webhook integration
- Seat reservation without overselling
- Travel history
- Badge system
- Progression map
- Territory progression
- Municipality progression
- Quest progression
- GeoNames-based geo normalization
- Global exception handling
- Structured logging with Serilog
- Correlation ID per request

---

## Architecture

```text
Controller -> Service -> Repository -> Database
```

- Services contain business logic
- Repositories handle database access
- Controllers stay thin

The feature structure roughly follows:

```text
BusRejser/
  Features/
    Auth/
    Bookings/
    Buses/
    Rejser/
    Payments/
    Progression/
    Badges/
    TravelHistory/
    Geo/
    Facilities/

  Common/
    DTOs/
    Security/
```

---

## Booking flow

```text
Frontend -> Stripe Checkout -> Webhook -> BookingService -> DB
```

- Bookings are only created after verified payment
- The webhook flow is idempotent
- Checkout status is read-only

---

## Progression and gamification

The backend handles user travel progression through completed bookings.

The flow is:

```text
Completed booking
-> SyncUserProgress
-> TravelHistory
-> VisitedLocation
-> Progression map
-> Badges / Quests
```

Progression consists of:

- `TravelHistory` for completed trips
- `VisitedLocation` for aggregated location data
- `ProgressionMapResponse` for the frontend map/dashboard
- `TerritoryProgressResponse` for countries/territories
- `MunicipalityProgressResponse` for municipalities
- `QuestProgressResponse` for computed quests

Quests are v1 computed-only:

```text
VisitedLocation + Territories + Municipalities
-> QuestProgressService
-> QuestProgressResponse
```

This means:

- no quest database yet
- no quest migrations yet
- the backend owns quest progress
- the frontend only renders quest data

Geo normalization is used to translate destinations into country, region, municipality, and coordinates.

---

## Progression endpoints

```text
GET /api/Progression/map
GET /api/Progression/quests
```

`/api/Progression/map` is used by the frontend progression dashboard and map.

`/api/Progression/quests` returns the user’s computed quest progression.

---

## Badge flow

Badges are evaluated backend-side as part of progression sync.

```text
SyncUserProgress
-> BadgeEngine
-> UserBadges
```

Badges are persistent data, while quests in v1 are computed data.

---

## Requirements

- .NET 8 SDK
- MySQL 8
- Stripe test keys

---

## Local setup without committed secrets

The project is configured to use the following model:

1. `appsettings.json` for safe defaults and structure
2. `appsettings.Development.json` as a local ignored development file
3. `dotnet user-secrets` or environment variables for real secrets
4. `BusRejser/appsettings.Example.json` as a shared reference for the full config shape

In practice this means:

- the repository shows which config fields exist
- `appsettings.Development.json` may contain local development values because it is ignored
- production and CI must not rely on local secret files
- user-secrets or environment variables are still the preferred approach for real secrets

`BusRejser/appsettings.Example.json` can be copied as a starting point for a local development file.

```powershell
Copy-Item .\BusRejser\appsettings.Example.json .\BusRejser\appsettings.Development.json
```

### Preferred model: Initialize user-secrets

Run from the repository root:

```powershell
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "ConnectionStrings:DefaultConnection" "server=localhost;port=3307;database=busplanen;user=bususer;password=replace-me;"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Jwt:Secret" "replace-with-at-least-32-characters"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Jwt:Issuer" "BusPlanen.Api"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Jwt:Audience" "BusPlanen.Client"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Stripe:SecretKey" "replace-with-stripe-secret-key"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Stripe:WebhookSecret" "replace-with-stripe-webhook-secret"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:Host" "sandbox.smtp.mailtrap.io"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:Port" "587"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:Username" "replace-with-email-username"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:Password" "replace-with-email-password"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Email:From" "noreply@example.com"
```

### Alternative: environment variables

ASP.NET Core maps `__` to `:`.

```powershell
$env:ConnectionStrings__DefaultConnection="server=localhost;port=3307;database=busplanen;user=bususer;password=replace-me;"
$env:Jwt__Secret="replace-with-at-least-32-characters"
$env:Jwt__Issuer="BusPlanen.Api"
$env:Jwt__Audience="BusPlanen.Client"
$env:Stripe__SecretKey="replace-with-stripe-secret-key"
$env:Stripe__WebhookSecret="replace-with-stripe-webhook-secret"
$env:Email__Host="sandbox.smtp.mailtrap.io"
$env:Email__Port="587"
$env:Email__Username="replace-with-email-username"
$env:Email__Password="replace-with-email-password"
$env:Email__From="noreply@example.com"
```

---

## CORS and frontend config

The following fields can be configured in your local ignored `appsettings.Development.json`:

- `Cors:AllowedOrigins`
- `Frontend:BaseUrl`
- `Frontend:PaymentSuccessPath`
- `Frontend:PaymentCancelPath`
- `Frontend:PasswordResetPath`
- `Auth:RefreshTokenLifetimeDays`
- `Auth:RequireConfirmedEmail`

You may also store local development secrets there if you intentionally choose that model. The important part is that the file is not tracked in git.

These values are used for trusted frontend origins and Stripe redirects.

---

## Rate limiting

Sensitive endpoints are protected using ASP.NET Core's built-in rate limiter.

Protected endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/stripe/create-checkout-session`
- `GET /api/stripe/checkout-status`

Limits are controlled through the `RateLimiting` config section and can differ between development and production.

The webhook endpoint is not rate limited to avoid blocking legitimate Stripe retries.

---

## Database with Docker Compose

`docker-compose.yml` uses environment variables instead of hardcoded passwords.

1. Copy `.env.example` to `.env`
2. Set your own values
3. Start the database:

```powershell
docker compose up -d
```

---

## Database and migrations

Database schema changes must be handled through EF Core migrations.

Data such as badges, GeoNames data, demo trips, and test users must be handled through seeders, importers, or controlled scripts.

Do not manually create or alter tables in MySQL/DBeaver unless there is a specific emergency reason.

Full rules are documented in:

```text
docs/DATABASE_RULES.md
```

Mental model:

```text
Migration = structure
Seeder/importer = data
DBeaver = inspection/backup, not schema design
```

---

## Run

```powershell
dotnet run --project .\BusRejser\BusRejser.csproj
```

Swagger is available in development:

```text
/swagger
```

---

## Startup validation

The application fails during startup if critical configuration is missing or invalid for:

- database connection string
- JWT secret, issuer, and audience
- refresh token lifetime and auth policy
- Stripe secret and webhook secret
- email host/credentials/from address
- trusted CORS origins
- trusted frontend base URL

---

## Tests

Run tests:

```powershell
dotnet test .\BusPlanen.Tests\BusPlanen.Tests.csproj
```

---

## Status

The backend is being hardened towards deployment.

Current status:

- Auth flow has been tightened
- Stripe flow works
- Booking flow is idempotent
- Config validation has been added
- Rate limiting has been added
- Database rules are documented
- Progression map works
- Travel history works
- Badge system works
- Quest progression is computed-only v1

---
