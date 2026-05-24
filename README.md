# MotorCare / BakimSuite Backend

This repository is backend-only. The previous in-repo frontend has been removed, and the new frontend will be built, deployed, and versioned as a separate app/repository that calls this API.

Bu repo şu an backend-only base olarak düzenlenmiştir. Eski Blazor frontend devreden çıkarılmıştır. Frontend repository/application yeniden oluşturulacaktır.

## Backend Structure

- `src/MotorCare.Api`: Carter-based .NET 8 API for auth, tenant, public QR, health, and smoke endpoints.
- `src/MotorCare.Application`: CQRS/MediatR use cases, DTOs, and validators.
- `src/MotorCare.Domain`: DDD aggregates, value objects, and domain behavior.
- `src/MotorCare.Infrastructure`: EF Core persistence, migrations, email, security, import, and repository infrastructure.
- `tests/`: Domain and application unit tests.

## Local Commands

```powershell
dotnet restore .\src\MotorCare.sln
dotnet build .\src\MotorCare.sln -c Release
dotnet test .\src\MotorCare.sln -c Release
```

Local backend stack:

```powershell
docker compose up -d postgres api
docker compose --profile tools run --rm migrator
```

## Deployment

- Local compose: `docker-compose.yml`
- Staging compose: `src/docker-compose.staging.yml`
- Production compose: `src/docker-compose.production.yml`
- Portainer env examples: `src/deploy/portainer/*.env.example`
- Staging Portainer runbook: `src/docs/ops/portainer-staging.md`
- Production secrets runbook: `src/docs/ops/production-secrets.md`
- Deployment checklist: `src/docs/deployment-checklist.md`
- Staging auth/email smoke: `src/docs/ops/staging-email-smoke.md`

Staging Portainer stack is backend-only and contains `postgres`, `api`, `mailpit`, and `migrator` under the `tools` profile. Production contains `postgres`, `api`, and `migrator`; production must not run Mailpit.

When redeploying Portainer stacks, enable orphan cleanup or use the equivalent `--remove-orphans` compose behavior so old frontend/app/web containers from previous deployments are removed.

## Frontend Integration

- The frontend is deployed separately and uses the deployed API base URL, for example `https://staging-api.bakimsuite.com`.
- Browser access is controlled by `CORS_ALLOWED_ORIGINS` / `Cors__AllowedOrigins`.
- Auth email links use `Email__AppBaseUrl`, which must point to the separate frontend URL, not the API URL.
- Login returns access and refresh tokens; refresh-token rotation remains a backend API contract.
- Public QR endpoints remain in the backend API; the frontend owns routing and presentation only.

No frontend container belongs in this repository's compose files.
