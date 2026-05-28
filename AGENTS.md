# Cross-Tool Agent Rules

This project uses DDD + Clean Architecture with .NET 8+ for the backend.
See `.agents/agents.md` for role definitions.
See `GEMINI.md` for full project rules.

## Quick Reference
- Backend: .NET 8, Carter, MediatR, EF Core, FluentValidation
- Frontend: removed from this repo; a new frontend will be built separately
- Pattern: DDD (Aggregates, Value Objects, Domain Events)
- Architecture: Clean Architecture (Domain -> Application -> Infrastructure -> API)
- No Dapper, no ControllerBase, no anemic models
