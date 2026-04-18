# Skill: Write Technical Specification

## Objective
Transform domain analysis into a complete technical specification document.

## Instructions

1. **Read Domain Artifacts**: Load and review all files in `production_artifacts/specs/`.

2. **Define Architecture**: Document the system architecture:
   - Solution structure (Clean Architecture layers)
   - Technology stack: .NET 8+, EF Core, MediatR, FluentValidation, Carter
   - Frontend: .NET MAUI with CommunityToolkit.Mvvm
   - Database: PostgreSQL or SQL Server
   - Authentication: ASP.NET Identity or JWT-based
   - Include a Mermaid architecture diagram

3. **API Design**: Define the REST API surface:
   - Endpoints grouped by Bounded Context
   - Request/Response DTOs
   - HTTP methods and status codes
   - Versioning strategy (URL-based: /api/v1/)

4. **Data Model**: Design the persistence layer:
   - EF Core entity configurations
   - Migration strategy
   - Seed data requirements

5. **Sprint Plan**: Break the MVP into sprints:
   - Sprint 1: Domain + Infrastructure scaffold
   - Sprint 2: Core business logic + API
   - Sprint 3: MAUI frontend + integration
   - Sprint 4: Testing, polish, deployment

## Output
Save to `production_artifacts/Technical_Specification.md`

## Approval Gate
You MUST pause and present the specification to the user.
Ask explicitly: "Bu teknik spesifikasyonu onaylıyor musunuz? Değişiklik istediğiniz yer var mı?"
Do NOT proceed to code generation without explicit approval.
