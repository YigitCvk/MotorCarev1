# Backend Deployment Checklist

## Preflight

- `dotnet restore ./src/MotorCare.sln`
- `dotnet build ./src/MotorCare.sln -c Release`
- `dotnet test ./src/MotorCare.sln -c Release`
- `docker compose -f docker-compose.yml config --quiet`
- `docker compose -f src/docker-compose.staging.yml config --quiet`
- `docker compose -f src/docker-compose.production.yml config --quiet`

## Backend Stack

- Staging services: `postgres`, `mailpit`, `api`, `migrator`.
- Production services: `postgres`, `api`, `migrator`.
- No frontend/app/web static container is part of this repo.
- Run migrator before API image promotion when migrations are pending.
- Do not remove database or attachment volumes during deploy.

## New Frontend Integration

- API base URL is the deployed API origin, for example `https://staging-api.bakimsuite.com`.
- Configure browser access with `CORS_ALLOWED_ORIGINS`.
- Auth uses bearer access tokens plus refresh-token rotation.
- Public QR data remains exposed by backend public API endpoints; the frontend owns only routing and presentation.
