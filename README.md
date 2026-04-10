# BusPlanen Backend (API)

Backend API for BusPlanen. Haandterer rejser, bookinger, brugere og Stripe-betaling.

## Features

- REST API for rejser, bookinger, brugere og busser
- JWT authentication med roller
- Stripe checkout og webhook-integration
- Seat reservation uden overselling
- Global exception handling
- Structured logging med Serilog
- Correlation ID per request

## Arkitektur

```text
Controller -> Service -> Repository -> Database
```

- Services indeholder business logic
- Repositories haandterer databaseadgang
- Controllers er tynde

## Booking flow

```text
Frontend -> Stripe Checkout -> Webhook -> BookingService -> DB
```

- Booking oprettes kun efter verificeret betaling
- Webhook-flowet er gjort idempotent
- Checkout-status er read-only

## Krav

- .NET 8 SDK
- MySQL 8
- Stripe test keys

## Lokal setup uden committed secrets

Projektet er sat op til at bruge denne model:

1. `appsettings.json` til sikre defaults og struktur
2. `appsettings.Development.json` som lokal, ignoreret dev-fil
3. `dotnet user-secrets` eller environment variables til rigtige secrets
4. `BusRejser/appsettings.Example.json` som delt reference for den fulde shape

Det betyder i praksis:

- repoet viser hvilke felter der findes
- `appsettings.Development.json` maa gerne indeholde lokale dev-vaerdier, fordi den er ignoreret
- production og CI maa ikke bygge paa lokale filer med secrets
- user-secrets eller environment variables er stadig den foretrukne vej til rigtige hemmeligheder

`BusRejser/appsettings.Example.json` kan kopieres som udgangspunkt, hvis du vil have en lokal dev-fil.

```powershell
Copy-Item .\BusRejser\appsettings.Example.json .\BusRejser\appsettings.Development.json
```

### Foretrukken model: Initialiser user-secrets

Koer fra repo-roden:

```powershell
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "ConnectionStrings:DefaultConnection" "server=localhost;port=3307;database=busplanen;user=bususer;password=replace-me;"
dotnet user-secrets --project .\BusRejser\BusRejser.csproj set "Jwt:Secret" "replace-with-at-least-32-characters"
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
$env:Stripe__SecretKey="replace-with-stripe-secret-key"
$env:Stripe__WebhookSecret="replace-with-stripe-webhook-secret"
$env:Email__Host="sandbox.smtp.mailtrap.io"
$env:Email__Port="587"
$env:Email__Username="replace-with-email-username"
$env:Email__Password="replace-with-email-password"
$env:Email__From="noreply@example.com"
```

## CORS og frontend-config

Foelgende felter kan saettes i din lokale, ignorerede `appsettings.Development.json`:

- `Cors:AllowedOrigins`
- `Frontend:BaseUrl`
- `Frontend:PaymentSuccessPath`
- `Frontend:PaymentCancelPath`

Du kan ogsaa laegge lokale dev-secrets der, hvis du bevidst vaelger den model. Det vigtige er, at filen ikke trackes i git.

Disse bruges til trusted frontend-origin og Stripe redirects.

## Database med Docker Compose

`docker-compose.yml` bruger nu environment variables i stedet for haardkodede passwords.

1. Kopier `.env.example` til `.env`
2. Saet dine egne vaerdier
3. Start databasen:

```powershell
docker compose up -d
```

## Run

```powershell
dotnet run --project .\BusRejser\BusRejser.csproj
```

Swagger er tilgaengelig i development:

```text
/swagger
```

## Startup validation

Applikationen failer nu ved startup hvis kritisk config mangler eller er ugyldig for:

- database connection string
- JWT secret
- Stripe secret og webhook secret
- email host/credentials/from address
- trusted CORS origins
- trusted frontend base URL

## Tests

Koer tests:

```powershell
dotnet test .\BusPlanen.Tests\BusPlanen.Tests.csproj
```

## Status

Backenden er stadig under haardening frem mod deployment, men auth-, Stripe- og config-flow er blevet strammet op.
