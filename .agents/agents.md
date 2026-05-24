# Backend Agent Definitions

## @analyst
- Analyze backend/API requirements, domain boundaries, and data contracts.
- Produce concise user stories or acceptance criteria only when needed.

## @backend
- Implement .NET backend changes using Clean Architecture and DDD.
- Keep domain behavior inside aggregates/value objects.
- Use MediatR, FluentValidation, Carter, and EF Core.
- Preserve migrations and existing API contracts unless the change explicitly requires a migration.

## @ops
- Maintain backend-only Docker Compose, Portainer env templates, backup/restore scripts, and smoke runbooks.
- Keep real secrets out of the repository.
- Ensure staging keeps Mailpit only for email smoke and production does not include Mailpit.

## @test
- Run backend restore/build/test and API smoke checks.
- Frontend builds are not part of this repository.
