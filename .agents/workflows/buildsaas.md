---
description: Start the full SaaS development pipeline from idea to working prototype
---

When the user types `/buildsaas <idea>`, orchestrate the full development cycle using `.agents/agents.md` and `.agents/skills/`.

### Execution Sequence:

1. Act as the **@analyst** and execute the `analyze_requirements.md` skill using the `<idea>`.
   - Produce domain model, user stories, and context map
   - Save all outputs to `production_artifacts/specs/`
   - **PAUSE**: Ask the user to review and approve the domain model

2. After user approval, act as the **@pm** and execute the `write_specs.md` skill.
   - Read the approved specs from `production_artifacts/specs/`
   - Produce the Technical Specification with architecture, API design, and sprint plan
   - Save to `production_artifacts/Technical_Specification.md`
   - **PAUSE**: Ask the user to review and approve the technical specification

3. After user approval, act as the **@backend** and execute the `implement_backend.md` skill.
   - Read the Technical Specification
   - Scaffold the .NET 8 solution with Clean Architecture
   - Implement Domain layer first, then Application, Infrastructure, and API
   - Run `dotnet build` to verify compilation
   - Run `dotnet test` to verify domain tests pass

4. After backend is complete, act as the **@frontend** and execute the `implement_frontend.md` skill.
   - Read the Technical Specification and the implemented API endpoints
   - Scaffold the MAUI project
   - Implement views, view models, and API integration
   - Run `dotnet build -t:SaaSApp.Maui` to verify compilation

### Handover Rules:
- Each agent reads the output of the previous agent — no skipping steps
- If any agent encounters ambiguity, PAUSE and ask the user instead of guessing
- All code must compile before proceeding to the next phase
- Save all intermediate artifacts to `production_artifacts/`
