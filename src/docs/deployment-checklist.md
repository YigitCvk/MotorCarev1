# Backend Deployment Checklist

This repository is backend-only. Frontend deployment is handled by a separate app/repository and must not add services back into these compose files.

## Preflight

- [ ] `dotnet restore ./src/MotorCare.sln`
- [ ] `dotnet build ./src/MotorCare.sln -c Release`
- [ ] `dotnet test ./src/MotorCare.sln -c Release`
- [ ] `docker compose -f docker-compose.yml config --quiet`
- [ ] `docker compose -f src/docker-compose.staging.yml config --quiet`
- [ ] `docker compose -f src/docker-compose.production.yml config --quiet`
- [ ] Confirm no real secrets are present in git-tracked files.
- [ ] Confirm frontend/app/web containers are not present in backend compose files.

## Staging Portainer Stack

- [ ] Stack file is `src/docker-compose.staging.yml`.
- [ ] Services are backend-only: `postgres`, `api`, `mailpit`, and `migrator` under the `tools` profile.
- [ ] Portainer environment is based on `src/deploy/portainer/staging.env.example`.
- [ ] `POSTGRES_PASSWORD` and `JWT_KEY` are set in Portainer or a restricted env file, not committed.
- [ ] `CORS_ALLOWED_ORIGINS` includes the separate staging frontend origin.
- [ ] `Email__AppBaseUrl` points to the separate staging frontend URL.
- [ ] Mailpit uses `Email__SmtpHost=mailpit`, `Email__SmtpPort=1025`, and `Email__SendEmails=true`.
- [ ] Mailpit web UI is reachable only through localhost or SSH tunnel.
- [ ] Deploy uses orphan cleanup / `--remove-orphans` so old frontend services are removed.
- [ ] Migrator runs once when migrations are pending.
- [ ] API `/health` passes from host-local and routed staging origins.
- [ ] Auth/email smoke passes using `src/docs/ops/staging-email-smoke.md`.

Staging smoke commands:

```bash
curl -fsS http://127.0.0.1:${API_HOST_PORT:-5102}/health
curl -fsS https://staging-api.bakimsuite.com/health
ssh -L 8025:localhost:8025 user@staging.bakimsuite.com
bash src/scripts/smoke/staging-auth-email-smoke.sh
```

## Production Portainer Stack

- [ ] Stack file is `src/docker-compose.production.yml`.
- [ ] Services are backend-only: `postgres`, `api`, and `migrator` under the `tools` profile.
- [ ] Portainer environment is based on `src/deploy/portainer/production.env.example`.
- [ ] Required production secrets are set outside git:
  - `POSTGRES_PASSWORD`
  - `JWT_KEY`
  - `Email__SmtpUsername`
  - `Email__SmtpPassword`
- [ ] `CORS_ALLOWED_ORIGINS` includes only trusted production frontend origins.
- [ ] `Email__AppBaseUrl` points to the production frontend URL.
- [ ] Production SMTP is configured and does not point to Mailpit.
- [ ] `API_IMAGE` and `MIGRATOR_IMAGE` point to intended production tags.
- [ ] Database and attachment backups are available and restore procedure is known.
- [ ] Deploy uses orphan cleanup / `--remove-orphans`.
- [ ] Migrator runs once when migrations are pending.
- [ ] API `/health` passes after deployment.

## Production NO-GO Preflight

Do not deploy production if any item below is true:

- [ ] A real secret is present in this repository or in a committed env file.
- [ ] Any required production secret is blank, placeholder, copied from staging, or unknown to the operator.
- [ ] Mailpit or another test email capture service is present in production.
- [ ] Old frontend/app/web services remain in the production Portainer stack.
- [ ] `CORS_ALLOWED_ORIGINS` is missing the frontend origin or includes broad/untrusted origins.
- [ ] `Email__AppBaseUrl` points to staging, localhost, or the API origin by mistake.
- [ ] `Email__SmtpUsername` or `Email__SmtpPassword` is missing while `Email__SendEmails=true`.
- [ ] `API_IMAGE` and `MIGRATOR_IMAGE` are not the intended release images.
- [ ] Backups are missing for the release window.
- [ ] Migrator has not run for pending EF migrations.
- [ ] API `/health` fails.

## New Frontend Integration

- API base URL is the deployed API origin, for example `https://staging-api.bakimsuite.com`.
- Browser access is configured with `CORS_ALLOWED_ORIGINS`.
- Auth uses bearer access tokens plus refresh-token rotation.
- Public QR data remains exposed by backend public API endpoints; the frontend owns routing and presentation.
