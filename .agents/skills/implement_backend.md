# Skill: Implement .NET Backend

## Objective
Implement the backend solution following Clean Architecture + DDD using .NET 8+.

## Instructions

1. **Read Specification**: Load `production_artifacts/Technical_Specification.md` and all specs.

2. **Scaffold Solution**:
   ```bash
   dotnet new sln -n SaaSApp
   dotnet new classlib -n SaaSApp.Domain -f net8.0
   dotnet new classlib -n SaaSApp.Application -f net8.0
   dotnet new classlib -n SaaSApp.Infrastructure -f net8.0
   dotnet new web -n SaaSApp.Api -f net8.0
   dotnet new xunit -n SaaSApp.Domain.Tests -f net8.0
   dotnet new xunit -n SaaSApp.Application.Tests -f net8.0
   ```
   Add project references following dependency rule:
   - Api → Application → Domain
   - Infrastructure → Application → Domain
   - Api → Infrastructure

3. **Domain Layer** (`src/Domain/`):
   ```csharp
   // Base classes
   public abstract class Entity<TId> { ... }
   public abstract class AggregateRoot<TId> : Entity<TId> { ... }
   public abstract record ValueObject;
   public interface IDomainEvent { DateTime OccurredOn { get; } }

   // Result pattern
   public sealed record Result<T>(bool IsSuccess, T? Value, string? Error);
   ```
   - Implement Aggregates with private setters, factory methods
   - Business rules as domain methods, NOT in handlers
   - Rich Value Objects with validation in constructor

4. **Application Layer** (`src/Application/`):
   ```csharp
   // CQRS with MediatR
   public sealed record CreateOrderCommand(
       Guid CustomerId,
       List<OrderItemDto> Items
   ) : IRequest<Result<Guid>>;

   public sealed class CreateOrderCommandHandler
       : IRequestHandler<CreateOrderCommand, Result<Guid>> { ... }

   // Validators
   public sealed class CreateOrderCommandValidator
       : AbstractValidator<CreateOrderCommand> { ... }
   ```
   - One handler per use case
   - Pipeline behaviors for validation, logging
   - No business logic — only orchestration

5. **Infrastructure Layer** (`src/Infrastructure/`):
   - EF Core DbContext with entity configurations
   - Repository implementations
   - Unit of Work pattern via DbContext.SaveChangesAsync

6. **API Layer** (`src/Api/`):
   - Carter modules for endpoint organization
   - Global exception handling middleware
   - Swagger/OpenAPI documentation
   - Health checks

## NuGet Packages
```xml
<!-- Application -->
MediatR, FluentValidation, FluentValidation.DependencyInjectionExtensions

<!-- Infrastructure -->
Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL

<!-- API -->
Carter, Swashbuckle.AspNetCore

<!-- Domain -->
No external dependencies!
```

## Rules
- Domain layer has ZERO external dependencies
- Nullable reference types enabled everywhere
- Use `sealed` classes/records where possible
- English log messages
- XML documentation on public APIs
- Build must succeed with `dotnet build` before marking task complete
