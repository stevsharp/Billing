Billing.Api (.NET 8, EF Core, architecture, license) — swap the placeholder repo URL/license as needed
Why this exists — the DDD/clean-architecture pitch
Features table — every capability we built (multi-tenancy, auditing, soft delete, outbox, CQRS, no-repos)
Architecture + project layout diagrams showing the dependency rule
Request lifecycle ASCII flow (MediatR → interceptor → outbox → publisher)
Getting started — prerequisites, Docker SQL Server one-liner, EF migration commands, run command
API reference with a sample request/response
Testing — the two suites and what each proves
Key design decisions — collapsible <details> sections explaining the record VOs, no-repository choice, shadow properties, outbox, and execution-strategy transactions
