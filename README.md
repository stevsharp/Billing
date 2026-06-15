# Billing API

A clean-architecture billing microservice built with ASP.NET Core 9 and Domain-Driven Design.

## Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 9 Minimal APIs |
| Application | MediatR · FluentValidation |
| Domain | DDD Aggregates · Domain Events |
| Persistence | EF Core 8 · SQL Server |
| Auth | JWT Bearer (HS256) |
| Messaging | Outbox Pattern |
| Tests | xUnit · FluentAssertions · Moq |

## Architecture

```
Billing.Api            → HTTP endpoints, middleware, DI wiring
Billing.Application    → Use cases (CQRS commands/queries), validators
Billing.Domain         → Aggregates, value objects, domain events
Billing.Infrastructure → EF Core contexts, JWT auth, outbox publisher
Billing.Tests          → Unit tests (domain + application layer)
```

The domain layer has zero dependencies on infrastructure or frameworks — all business rules live in plain C# classes.

## API Endpoints

All endpoints require a valid JWT (`Authorization: Bearer <token>`).

| Method | Path | Description |
|---|---|---|
| `POST` | `/invoices` | Issue a new invoice |
| `POST` | `/invoices/{id}/cancel` | Cancel an invoice |
| `GET` | `/invoices/pending` | List all issued (unpaid) invoices |

### Issue Invoice

```http
POST /invoices
Authorization: Bearer <token>
Content-Type: application/json

{
  "number": "INV-2024-001",
  "currency": "EUR",
  "lines": [
    { "description": "Consulting services", "amount": 1500.00 },
    { "description": "Travel expenses",     "amount":  250.00 }
  ]
}
```

### Cancel Invoice

```http
POST /invoices/{id}/cancel
Authorization: Bearer <token>
Content-Type: application/json

{ "reason": "Client request" }
```

## Invoice Lifecycle

```
Draft ──AddLine──► Draft
Draft ──Issue────► Issued ──Pay──► Paid
Draft ──Cancel───► Cancelled
Issued ──Cancel──► Cancelled
```

Transitions that violate this state machine throw a `DomainException` (HTTP 422).

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server (local or Docker)

### 1. Configure

Edit `Billing/appsettings.json` or use user secrets:

```json
{
  "ConnectionStrings": {
    "Primary": "Server=localhost;Database=Billing;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "billing-api",
    "Audience": "billing-client",
    "SecretKey": "<your-secret-key-min-32-chars>"
  }
}
```

> **Production:** inject `Jwt__SecretKey` as an environment variable — never commit a real secret to source control.

### 2. Apply Migrations

```bash
dotnet ef database update --project Billing.Infrastructure --startup-project Billing
```

### 3. Run

```bash
dotnet run --project Billing
```

OpenAPI docs are available at `/openapi/v1.json` when running in Development.

## Running Tests

```bash
dotnet test
```

```
Passed! - Failed: 0, Passed: 43, Skipped: 0
```

Test coverage includes:

- **Domain** — Invoice aggregate state machine, Money value object arithmetic and validation
- **Application** — `IssueInvoiceCommand` FluentValidation rules, `IssueInvoiceHandler` using EF Core InMemory

## JWT Authentication

The API uses symmetric HS256 JWT tokens. Tokens must carry:

| Claim | Purpose |
|---|---|
| `sub` | User identity |
| `tenant_id` | Tenant isolation (required — missing claim returns 401) |

Generate a development token with any JWT tool (e.g. [jwt.io](https://jwt.io)) using the secret key from `appsettings.json`.

## Project Highlights

- **Multi-tenant** — every query is filtered by `tenant_id` via an EF Core global query filter; the tenant context is never passed explicitly through application code.
- **Outbox pattern** — domain events are persisted in the same transaction as the aggregate, then published asynchronously by `OutboxPublisher`, preventing dual-write failures.
- **CQRS** — write model uses a tracked `BillingWriteContext`; read model uses a no-tracking `BillingReadContext` pointed at a read replica.
- **Pipeline behaviours** — MediatR pipelines handle validation (`ValidationBehavior`) and transactions (`TransactionBehavior`) cross-cutting, keeping handlers clean.


## Connect with Me

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Profile-blue)](https://www.linkedin.com/in/spyros-ponaris-913a6937/)
