# BusRejser API

Backend for bus booking system (Rejser, Booking, Stripe betaling).

---

## 🚀 Run locally

### 1. Requirements
- .NET 8
- MySQL
- Stripe account (test keys)

---

### 2. Environment variables

Set environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Development

DB_CONNECTION_STRING=your_connection_string_here
JWT_SECRET=your_jwt_secret_here

STRIPE_SECRET_KEY=your_stripe_secret_key
STRIPE_WEBHOOK_SECRET=your_webhook_secret
````

---

### 3. Database

* Ensure MySQL database exists
* Apply schema / migrations
* Connection string must match database

---

### 4. Run project

```bash
dotnet run
```

API runs on:

```
https://localhost:xxxx
```

Swagger:

```
/swagger
```

---

## 🔐 Auth

* JWT based authentication
* Roles:

  * Admin
  * Medarbejder
  * Kunde

---

## 💳 Booking & Stripe flow

```
Frontend → Stripe Checkout → Webhook → BookingService → DB
```

* Booking oprettes kun ved verificeret betaling
* Webhook er idempotent (samme event → ingen dubletter)
* Seat reservation håndteres i service layer

---

## 🧪 Key endpoints

```
POST   /api/booking
PUT    /api/booking/{id}/cancel
PUT    /api/booking/{id}/reactivate
GET    /api/booking/mine
GET    /api/booking/rejse/{id}
```

---

## 🧠 System notes

* Clean architecture:

  ```
  Controller → Service → Repository → DB
  ```
* Global exception handling
* Structured logging + correlation id
* Seat reservation er konsistent (ingen overselling)
* Rollback ved fejl i booking flow

---

## 🛠️ Deployment (short)

Required environment variables:

```
DB_CONNECTION_STRING
JWT_SECRET
STRIPE_SECRET_KEY
STRIPE_WEBHOOK_SECRET
```

Before production:

* CORS skal være låst til frontend domain
* Secrets må ikke være i repo
* Production environment skal være sat korrekt

---

## 🧪 Smoke test (after deploy)

Minimum test:

1. Login virker
2. Hent rejser virker
3. Opret booking virker
4. Cancel booking virker
5. Reactivate booking virker
6. Stripe webhook virker
7. Admin endpoints virker korrekt
8. Frontend kan ramme API

---

## 📌 Status

* Booking flow implementeret og testet
* Logging + middleware på plads
* Exception handling centraliseret
* Unit tests for BookingService (inkl. edge cases)
