# MotorCare Backend Agent Context

## Project Overview

MotorCare / BakimSuite is currently maintained as a backend-only base repository. Legacy Blazor and Angular frontend projects have been removed. New frontend applications must be created separately and integrate through the API.

## Tech Stack

- Backend: .NET 8+, C# 12, Clean Architecture, DDD
- API: Carter endpoints, JWT bearer auth, rate limiting, correlation IDs
- Persistence: EF Core code-first migrations, PostgreSQL
- CQRS: MediatR
- Validation: FluentValidation
- Testing: xUnit, FluentAssertions, NSubstitute

## Architecture

- `src/MotorCare.Domain`: aggregates, value objects, domain behavior, repository interfaces
- `src/MotorCare.Application`: MediatR commands/queries, DTOs, validators
- `src/MotorCare.Infrastructure`: EF Core, migrations, repositories, email/security services
- `src/MotorCare.Api`: Carter modules, middleware, auth, Dockerfiles
- `tests`: backend unit tests

## Rules

- Keep frontend code out of this repository unless there is an explicit new architecture decision.
- Preserve migrations and backend API contracts.
- Do not commit populated env files, secrets, local agent settings, build artifacts, or runtime data.
- After backend changes, run `dotnet build` and `dotnet test`.
