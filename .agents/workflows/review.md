---
description: Run a DDD-focused code review on the current codebase
---

When the user types `/review`, act as a senior .NET architect and review the codebase.

### Review Checklist:

1. **DDD Compliance**:
   - Are Aggregate Roots enforcing invariants?
   - Is business logic in the Domain layer (not Application/Infrastructure)?
   - Are Value Objects immutable with proper equality?
   - Are Domain Events raised for cross-context communication?

2. **Clean Architecture**:
   - Does the dependency rule hold? (Domain has no references to outer layers)
   - Are interfaces defined in Domain, implementations in Infrastructure?
   - Is there any leakage of EF Core types into the Domain?

3. **CQRS/MediatR**:
   - One handler per command/query?
   - Are validators registered and complete?
   - Are Commands mutating state and Queries read-only?

4. **Code Quality**:
   - Nullable reference types handled properly?
   - Are records sealed?
   - English log messages?
   - No magic strings?

5. **MAUI (if applicable)**:
   - MVVM pattern followed? No code-behind logic?
   - All API calls have error handling + loading states?
   - DI registered correctly?

### Output:
Create a review report as an artifact with:
- Critical issues (must fix)
- Warnings (should fix)
- Suggestions (nice to have)
- Overall assessment
