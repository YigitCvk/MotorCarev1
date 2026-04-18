# Skill: Domain Analysis & Requirements

## Objective
Analyze the SaaS product idea and produce a DDD-aligned domain model with user stories.

## Instructions

1. **Discover Bounded Contexts**: Identify the major business domains. For each context:
   - Name it with Ubiquitous Language
   - List its core responsibilities
   - Identify its Aggregates and Entities

2. **Map Domain Relationships**: Create a Context Map showing:
   - Upstream/Downstream relationships
   - Integration patterns (Anti-Corruption Layer, Shared Kernel, etc.)
   - Use Mermaid diagram syntax

3. **Write User Stories**: For each Bounded Context, write stories:
   ```
   As a [role]
   I want to [action]
   So that [benefit]

   Acceptance Criteria:
   - Given [context], When [action], Then [outcome]
   ```

4. **Identify Domain Events**: List events that cross context boundaries:
   ```
   Event: OrderPlaced
   Source: Order Context
   Consumers: Payment Context, Notification Context
   Payload: { orderId, customerId, totalAmount, items[] }
   ```

5. **Define Aggregates**: For each aggregate:
   - Root entity and its invariants
   - Value Objects it contains
   - Business rules it enforces

## Output
Save all artifacts to:
- `production_artifacts/specs/domain_model.md`
- `production_artifacts/specs/user_stories.md`
- `production_artifacts/specs/context_map.md`

## Approval Gate
You MUST pause and ask the user to review the domain model before proceeding.
