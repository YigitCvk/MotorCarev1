# Project Rules - SaaS Backend Development

## Technology Stack
- **Backend**: .NET 8+, C# 12, Clean Architecture, DDD
- **Frontend**: removed from this repo; new frontend applications are built separately against the API
- **ORM**: Entity Framework Core (code-first)
- **CQRS**: MediatR
- **Validation**: FluentValidation
- **Endpoints**: Carter (NOT ControllerBase)
- **Database**: PostgreSQL
- **Testing**: xUnit, FluentAssertions, NSubstitute

## Architecture Rules
- Follow Clean Architecture: Domain -> Application -> Infrastructure/API
- Domain layer has ZERO external package dependencies
- Use DDD tactical patterns: Aggregate Roots, Value Objects, Domain Events
- Aggregate Roots are the ONLY entry point for state mutations
- Use Result pattern for domain operations, NOT exceptions
- One MediatR handler per use case; no god handlers
- No Dapper; use EF Core exclusively
- No caching layer unless explicitly requested

## Code Style
- `sealed` on classes/records that won't be inherited
- `sealed record` for DTOs, Commands, Queries
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- English for code, variable names, log messages, and XML docs
- Turkish for user-facing strings and validation messages
- Private fields with underscore prefix: `_repository`
- File-scoped namespaces
- Primary constructors where appropriate

## Naming Conventions
- Commands: `{Verb}{Noun}Command`
- Queries: `Get{Noun}Query`
- Handlers: `{Command/Query}Handler`
- Validators: `{Command/Query}Validator`
- Endpoints: Carter modules grouped by feature/bounded context
- Domain Events: `{Noun}{PastTenseVerb}Event`

## Git Conventions
- Commit messages in English, conventional commits format
- feat: / fix: / refactor: / docs: / test:
- One feature per branch

## Important
- Build MUST succeed before marking any task complete.
- Always run `dotnet build` and `dotnet test` after code changes.
- Do not add frontend projects back to this repo without an explicit architecture decision.
