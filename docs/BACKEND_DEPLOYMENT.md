# Backend Deployment

## Deployment order

1. Build and test the backend from the repository root.
2. Provision or verify the database and runtime configuration.
3. Apply pending database migrations before routing traffic to the new backend.
4. Deploy the backend artifact or container.
5. Start the application and verify smoke tests.

## Required configuration

Set production values through environment variables or the hosting platform secret store. Do not commit real secrets.

- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__AccessTokenLifetimeMinutes`
- `Auth__RefreshTokenLifetimeDays`
- `Auth__RequireConfirmedEmail`
- `Stripe__SecretKey`
- `Stripe__WebhookSecret`
- `Cors__AllowedOrigins__0`
- `Frontend__BaseUrl`
- `Frontend__PaymentSuccessPath`
- `Frontend__PaymentCancelPath`
- `Frontend__PasswordResetPath`
- `Email__Provider`
- `Email__From`
- `Email__ApiKey`

Rate limiting can be configured with the `RateLimiting__<Policy>__PermitLimit`, `RateLimiting__<Policy>__WindowSeconds`, and `RateLimiting__<Policy>__QueueLimit` keys.

## Database

The backend expects a MySQL-compatible database from `ConnectionStrings__DefaultConnection`. The database user must be able to read and write application tables, and migration execution requires schema-change permissions.

## Migrations

Run Entity Framework migrations against the target database before deploy traffic is enabled. Validate that the connection string points at the intended environment before running migrations.

## Runtime notes

Run with `ASPNETCORE_ENVIRONMENT=Production` for production deployments. Configure CORS origins to the deployed frontend URL only. Configure the frontend base URL so payment redirects and password reset links point to the deployed frontend.

## Smoke tests

After deployment, verify:

- `GET /health` returns a successful health response.
- Swagger is not exposed unless the environment intentionally enables it.
- API responses include the expected CORS behavior for the deployed frontend origin.

## Auth checks

Exercise the critical auth endpoints after deploy:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/auth/verify-email`
- `POST /api/auth/resend-verification-email`

## Secret handling

Never commit production secrets, API keys, tokens, passwords, or connection strings. Keep real values in the deployment platform secret store and keep repository examples as placeholders only.
