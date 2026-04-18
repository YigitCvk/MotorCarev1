# SaaS Application — AI Agent Development Pipeline

## Project Overview
Multi-agent SaaS geliştirme projesi. DDD + Clean Architecture + .NET 8+ backend, .NET MAUI frontend.

## Tech Stack
- **Backend**: .NET 8+, C# 12, Clean Architecture, DDD
- **Frontend**: .NET MAUI, MVVM, CommunityToolkit.Mvvm
- **ORM**: Entity Framework Core (code-first, PostgreSQL)
- **CQRS**: MediatR
- **Validation**: FluentValidation
- **Endpoints**: Carter (NOT ControllerBase)
- **Testing**: xUnit, FluentAssertions, NSubstitute

## Architecture
Clean Architecture layers:
- `src/Domain/` — Entities, Value Objects, Aggregates, Domain Events (ZERO external deps)
- `src/Application/` — MediatR Handlers, DTOs, Validators
- `src/Infrastructure/` — EF Core, Repositories, External Services
- `src/Api/` — Carter endpoints, Middleware
- `src/MauiApp/` — MAUI Views, ViewModels, Services

## Code Conventions
- `sealed record` for DTOs, Commands, Queries
- Nullable reference types enabled
- English: code, logs, XML docs
- Turkish: user-facing UI strings
- One MediatR handler per use case
- Result pattern (not exceptions) for domain operations
- No Dapper, no ControllerBase, no anemic domain models

## Agent Config
See `.agents/agents.md` for role definitions.
See `.agents/skills/` for detailed skill instructions.
See `.agents/workflows/` for pipeline workflows.
See `GEMINI.md` for Antigravity-specific rules.
