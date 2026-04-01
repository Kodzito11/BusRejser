# 🚌 BusPlanen Backend (API)

Backend API for BusPlanen – håndterer rejser, booking, brugere og Stripe betaling.

---

## ✨ Features

- REST API for:
  - Rejser
  - Booking
  - Brugere
  - Busser
- JWT authentication (roller)
- Stripe checkout + webhook integration
- Seat reservation (ingen overselling)
- Global exception handling
- Structured logging (Serilog)
- Correlation ID pr request
- Unit tests for BookingService

---

## 🧠 Arkitektur

```

Controller → Service → Repository → Database

```

- Services indeholder business logic
- Repositories håndterer database
- Controllers er tynde

---

## 🔐 Auth

JWT baseret auth med roller:

- Admin
- Medarbejder
- Kunde

---

## 💳 Booking flow

```

Frontend → Stripe Checkout → Webhook → BookingService → DB

```

- Booking oprettes kun efter verificeret betaling
- Webhook er idempotent (ingen dubletter)
- Seats reserveres før booking
- Rollback hvis noget fejler

---

## 🧪 API endpoints (uddrag)

```

GET    /api/rejse
GET    /api/rejse/{id}
POST   /api/rejse        (Admin/Medarbejder)

GET    /api/booking/mine
GET    /api/booking/rejse/{id}
PUT    /api/booking/{id}/cancel
PUT    /api/booking/{id}/reactivate

POST   /api/stripe/create-checkout-session
POST   /api/stripe/webhook

```

---

## 🚀 Kør projektet lokalt

### Requirements

- .NET 8
- MySQL
- Stripe test keys

---

### Environment variables

```

ASPNETCORE_ENVIRONMENT=Development

DB_CONNECTION_STRING=your_connection_string
JWT_SECRET=your_jwt_secret

STRIPE_SECRET_KEY=your_key
STRIPE_WEBHOOK_SECRET=your_secret

````

---

### Run

```bash
dotnet run
````

Swagger:

```
/swagger
```

---

## 🧪 Tests

* Unit tests for BookingService
* Dækker:

  * seat reservation
  * rollback
  * cancel/reactivate
  * Stripe webhook cases

---

## 📌 Status

* Booking flow implementeret og testet
* Stripe integration virker
* Logging + middleware på plads
* Exception handling centraliseret

---

## 🧱 Næste skridt

* Flere tests (Auth + Rejse)
* Deployment setup
* Production config (CORS + env)

---

## ⚠️ Note

Kører pt. i development.
Production setup kommer senere.

```
