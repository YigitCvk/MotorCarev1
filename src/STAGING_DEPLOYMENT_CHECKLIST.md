# BakimSuite Staging Deployment Checklist

This document prepares MotorCare / BakimSuite for staging deployment without changing business logic.

## 1. Runtime Overview

Projects:

- `MotorCare.Api`
- `MotorCare.App`

Primary runtime dependencies:

- .NET 8 ASP.NET Core runtime
- PostgreSQL
- optional Elasticsearch / Kibana for centralized logs
- reverse proxy such as Nginx

## 2. Configuration Strategy

### API

Configuration files:

- `MotorCare.Api/appsettings.json`
- `MotorCare.Api/appsettings.Development.json`
- `MotorCare.Api/appsettings.Staging.json`
- `MotorCare.Api/appsettings.Production.json`

Guidelines:

- keep non-secret defaults in `appsettings.json`
- keep local-only values in `appsettings.Development.json`
- keep staging and production structure in environment-specific files
- provide secrets from environment variables or deployment secret store

### App

Configuration files:

- `MotorCare.App/appsettings.json`
- `MotorCare.App/appsettings.Development.json`
- `MotorCare.App/appsettings.Staging.json`
- `MotorCare.App/appsettings.Production.json`

Guidelines:

- app-side main runtime setting is `ApiBaseUrl`
- point staging UI to staging API
- point production UI to production API

## 3. Required Environment Variables

Set these for staging and production instead of committing secrets:

### API

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`
- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Jwt__AccessTokenMinutes`
- `Jwt__RefreshTokenDays`
- `Elastic__Uri`
- `Elastic__Username`
- `Elastic__Password`
- `Elastic__IndexFormat`

Optional but recommended:

- `Serilog__MinimumLevel__Default`
- `AllowedHosts`

### App

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`
- `ApiBaseUrl`

## 4. Server Prerequisites

Recommended Linux target:

- Ubuntu 22.04 LTS or newer
- .NET 8 ASP.NET Core runtime
- PostgreSQL 15+
- Nginx
- systemd

Install .NET runtime:

```bash
dotnet --info
```

Verify PostgreSQL:

```bash
psql --version
```

## 5. Recommended Folder Layout

```text
/opt/bakimsuite/
  api/
  app/
  shared/
  logs/
/etc/bakimsuite/
  api.env
  app.env
```

Recommended:

- publish outputs under `/opt/bakimsuite`
- keep environment variables in root-owned files under `/etc/bakimsuite`
- do not store secrets inside repo checkout

## 6. PostgreSQL Setup

Create:

- database
- application user
- strong password

Example connection string shape:

```text
Host=127.0.0.1;Port=5432;Database=bakimsuite_staging;Username=bakimsuite;Password=<secret>
```

## 7. Migration Command

Run before first staging start and after schema changes:

```bash
dotnet ef database update \
  --project src/MotorCare.Infrastructure/MotorCare.Infrastructure.csproj \
  --startup-project src/MotorCare.Api/MotorCare.Api.csproj
```

If deploying from published output only, run migrations during release pipeline before switching traffic.

## 8. Publish Commands

### API

```bash
dotnet publish src/MotorCare.Api/MotorCare.Api.csproj -c Release -o /opt/bakimsuite/api
```

### App

```bash
dotnet publish src/MotorCare.App/MotorCare.App.csproj -c Release -o /opt/bakimsuite/app
```

## 9. Run Commands

### API

```bash
ASPNETCORE_ENVIRONMENT=Staging \
ASPNETCORE_URLS=http://127.0.0.1:5099 \
dotnet /opt/bakimsuite/api/MotorCare.Api.dll
```

### App

```bash
ASPNETCORE_ENVIRONMENT=Staging \
ASPNETCORE_URLS=http://127.0.0.1:5100 \
dotnet /opt/bakimsuite/app/MotorCare.App.dll
```

## 10. systemd Service Names

Recommended service names:

- `bakimsuite-api.service`
- `bakimsuite-app.service`

Recommended service responsibilities:

- app starts after network is ready
- app restarts on failure
- env file loaded from `/etc/bakimsuite/*.env`
- working directory points to publish folder

## 11. Reverse Proxy Notes

Recommended split:

- UI: `https://staging.bakimsuite.com`
- API: `https://staging-api.bakimsuite.com`

Example upstream paths:

- app upstream -> `127.0.0.1:5100`
- api upstream -> `127.0.0.1:5099`

Suggested proxy headers:

- `Host`
- `X-Forwarded-For`
- `X-Forwarded-Proto`
- `X-Request-Id`

## 12. Nginx Path Example

```text
/etc/nginx/sites-available/bakimsuite-staging-app
/etc/nginx/sites-available/bakimsuite-staging-api
```

Use separate server blocks for UI and API for simpler CORS, TLS and troubleshooting.

## 13. SSL Notes

Recommended:

- terminate TLS at Nginx
- use Let's Encrypt or managed certificate
- redirect HTTP to HTTPS
- keep Kestrel bound to localhost only

## 14. Health Check Verification

API health endpoint:

- `GET /health`

Expected response:

```json
{
  "status": "Healthy",
  "service": "MotorCare.Api"
}
```

Verify after deployment:

1. Nginx can reach API upstream
2. `/health` returns 200
3. app can call configured `ApiBaseUrl`

## 15. Logging Verification

Verify:

1. API starts with Serilog enabled
2. logs reach stdout / journal
3. if Elastic is configured, logs arrive in Elasticsearch
4. Kibana shows:
   - `ApplicationName`
   - `Environment`
   - `EventId`
   - `EventName`
   - `CorrelationId`
   - `TenantId`
   - `UserId`

See also:

- `OBSERVABILITY_DEPLOYMENT_CHECKLIST.md`

## 16. Print Page Verification

Verify these render under staging:

- `/inspections/{id}/print`
- `/service-orders/{id}/print`

Check:

- print layout loads
- browser print preview opens
- company header and details are visible
- no auth redirect loop

## 17. Short Staging Smoke Checklist

After deployment, verify:

1. login works
2. create customer
3. create vehicle
4. create appointment
5. create service order
6. create motorcycle inspection
7. open inspection print page
8. open service order print page
9. `/health` returns healthy
10. logs are visible in Kibana

## 18. Deployment Sign-off

Staging is ready when:

- API and App run under `Staging`
- PostgreSQL connection is healthy
- migrations are applied
- reverse proxy and TLS work
- `/health` is healthy
- core smoke flow passes
- print pages render
- logs are visible in Kibana with correlation and tenant context
