# Portainer Staging Runbook

This repository deploys only backend services. The frontend is a separate app/repository and must be deployed independently against the API origin.

## Stack File

Use `src/docker-compose.staging.yml` in Portainer.

Expected backend-only services:

- `postgres`
- `api`
- `mailpit`
- `migrator` under the `tools` profile

There must be no frontend, app, web, nginx static site, or SPA container in this stack.

## Environment

Start from `src/deploy/portainer/staging.env.example`, then fill values in Portainer or in a restricted server-side env file. Do not commit populated env files.

Required staging secrets:

- `POSTGRES_PASSWORD`
- `JWT_KEY`

Staging Mailpit defaults:

- `Email__SmtpHost=mailpit`
- `Email__SmtpPort=1025`
- `Email__SendEmails=true`
- `MAILPIT_WEB_PORT=8025`

Frontend integration values:

- `CORS_ALLOWED_ORIGINS` is the semicolon-separated browser origin allowlist for the separate frontend app.
- `Email__AppBaseUrl` is the separate frontend URL used in invite, verification, and password reset links.

Example CORS shape without real secrets:

```text
CORS_ALLOWED_ORIGINS=https://staging.bakimsuite.com;https://preview.example.com
Email__AppBaseUrl=https://staging.bakimsuite.com
```

## Deploy Steps

1. In Portainer, update the stack file from `src/docker-compose.staging.yml`.
2. Update environment variables, keeping real secrets only in Portainer or a restricted env file.
3. Pull the intended `API_IMAGE` and `MIGRATOR_IMAGE` tags.
4. Deploy with orphan cleanup enabled, or run the equivalent compose command with `--remove-orphans`.
5. Run the migrator profile once before validating the API when migrations are pending.
6. Verify the API health endpoint and auth/email smoke.

Orphan cleanup matters because this repository used to include frontend services. If old `app`, `web`, or frontend containers remain after deploy, remove them from the staging stack instead of keeping them as unmanaged containers.

Equivalent CLI shape:

```bash
docker compose -f src/docker-compose.staging.yml --env-file /path/to/staging.env up -d --remove-orphans postgres mailpit
docker compose -f src/docker-compose.staging.yml --env-file /path/to/staging.env --profile tools run --rm migrator
docker compose -f src/docker-compose.staging.yml --env-file /path/to/staging.env up -d --remove-orphans api
```

## Mailpit Access

Mailpit is bound to `127.0.0.1` on the staging host. Use an SSH tunnel from your workstation:

```bash
ssh -L 8025:localhost:8025 user@staging.bakimsuite.com
```

Then open `http://localhost:8025`.

Quick API check:

```bash
curl -s http://localhost:8025/api/v1/messages | jq .
```

## API Smoke Commands

Health from the staging host:

```bash
curl -fsS http://127.0.0.1:${API_HOST_PORT:-5102}/health
```

Health from outside through the routed staging API:

```bash
curl -fsS https://staging-api.bakimsuite.com/health
```

Auth/email smoke:

```bash
bash src/scripts/smoke/staging-auth-email-smoke.sh
```

Manual Mailpit-backed auth smoke is documented in `src/docs/ops/staging-email-smoke.md`.

## NO-GO Checks

Do not promote staging if any of these are true:

- Any real secret is written into git-tracked files.
- The Portainer stack still contains frontend/app/web services from the old full-stack deployment.
- `CORS_ALLOWED_ORIGINS` does not include the deployed frontend origin.
- `Email__AppBaseUrl` points to the API origin instead of the frontend origin.
- Mailpit is exposed publicly instead of bound to localhost.
- Migrator has not run after a deployment with pending EF migrations.
- `/health` fails for the API container or routed staging API.
