# Staging Deploy Checklist — BakımSuite / MotorCare

## Staging Server

- **IP:** 46.225.166.254
- **OS:** Ubuntu 24.04 LTS

---

## 1. DNS (BLOCKER — must be done before HTTPS)

DNS records must resolve before running certbot; HTTPS setup fails without them.

- [ ] A record: `staging.bakimsuite.com` → `46.225.166.254`
- [ ] A record: `staging-api.bakimsuite.com` → `46.225.166.254`

Verify propagation before continuing:

```bash
dig +short staging.bakimsuite.com
dig +short staging-api.bakimsuite.com
```

---

## 2. Required Env Values (Portainer / .env.staging)

Template: `src/deploy/portainer/staging.env.example`

| Variable | Description | Secret? | Required? |
|---|---|---|---|
| `POSTGRES_PASSWORD` | PostgreSQL password | YES | REQUIRED |
| `JWT_KEY` | JWT signing key (min 32 chars) | YES | REQUIRED |
| `JWT_ISSUER` | Token issuer (`https://staging-api.bakimsuite.com`) | no | yes |
| `JWT_AUDIENCE` | Token audience (`https://staging.bakimsuite.com`) | no | yes |
| `JWT_ACCESS_TOKEN_MINUTES` | Access token lifetime, default `60` | no | optional |
| `JWT_REFRESH_TOKEN_DAYS` | Refresh token lifetime, default `7` | no | optional |
| `POSTGRES_DB` | Database name, default `motorcare_staging` | no | optional |
| `POSTGRES_USER` | Database user, default `motorcare_user` | no | optional |
| `CORS_ALLOWED_ORIGINS` | Allowed origins, default `https://staging.bakimsuite.com` | no | yes |
| `NEXT_PUBLIC_APP_URL` | Public frontend URL (`https://staging.bakimsuite.com`) | no | yes |
| `NEXT_PUBLIC_API_BASE_URL` | Public API URL (`https://staging-api.bakimsuite.com`) | no | yes |
| `API_IMAGE` | API container image tag | no | yes |
| `WEB_IMAGE` | Web container image tag | no | yes |
| `MIGRATOR_IMAGE` | Migrator container image tag | no | yes |
| `Email__AppBaseUrl` | Base URL inserted into email links | no | yes |
| `Email__FromEmail` | Sender address, default `no-reply@bakimsuite.com` | no | optional |
| `Email__SmtpHost` | SMTP host, default `mailpit` (internal) | no | optional |
| `Email__SmtpPort` | SMTP port, default `1025` | no | optional |
| `Email__SendEmails` | Enable outbound email, default `true` | no | optional |
| `MAILPIT_WEB_PORT` | Mailpit UI host port, default `8025` | no | optional |
| `ELASTIC_URI` | Elasticsearch endpoint (leave blank to disable) | YES | optional |
| `ELASTIC_USERNAME` | Elasticsearch username | YES | optional |
| `ELASTIC_PASSWORD` | Elasticsearch password | YES | optional |
| `MOTORCARE_COMMIT_SHA` | Injected by CI; set manually if deploying without CI | no | optional |
| `MOTORCARE_BUILD_TIME` | Injected by CI; set manually if deploying without CI | no | optional |

> The two variables marked REQUIRED (`POSTGRES_PASSWORD`, `JWT_KEY`) have no defaults and will
> cause the stack to refuse to start if they are blank.

---

## 3. Docker Stack

Compose file: `src/docker-compose.staging.yml`

| Service | Container name | Image | Notes |
|---|---|---|---|
| `postgres` | `motorcare-staging-postgres` | `postgres:16` | Exposes no host port (internal only). Data persisted in `postgres_data` volume. Health-checked with `pg_isready`. |
| `mailpit` | `motorcare-staging-mailpit` | `axllent/mailpit:latest` | Email capture sidecar. UI on `127.0.0.1:8025`. Access via SSH tunnel: `ssh -L 8025:localhost:8025 user@46.225.166.254`. |
| `api` | `motorcare-staging-api` | `ghcr.io/yigitcvk/motorcare-api:staging` | Binds `127.0.0.1:5002:8080` (override with `API_HOST_PORT`). Health-checked via `/health`. Waits for postgres healthy. |
| `web` | `motorcare-staging-web` | `ghcr.io/yigitcvk/motorcare-web:staging` | Binds `127.0.0.1:3000:8080` (override with `WEB_HOST_PORT`). Health-checked via `/api/health`. Waits for api healthy. |
| `migrator` | `motorcare-staging-migrator` | `ghcr.io/yigitcvk/motorcare-migrator:staging` | Profile `tools` — run explicitly. Runs migrations then exits (`restart: "no"`). |

Deploy commands:

```bash
# Pull latest images and start the stack
docker compose -f src/docker-compose.staging.yml --env-file .env.staging up -d

# Run database migrations (one-shot)
docker compose -f src/docker-compose.staging.yml --env-file .env.staging \
  --profile tools run --rm migrator

# Check container health
docker compose -f src/docker-compose.staging.yml ps
```

---

## 4. Nginx Config

Full reference: `docs/ops/nginx-staging.md`

| Step | Command |
|---|---|
| Save config | `sudo cp nginx-staging.conf /etc/nginx/sites-available/bakimsuite-staging` |
| Enable site | `sudo ln -s /etc/nginx/sites-available/bakimsuite-staging /etc/nginx/sites-enabled/bakimsuite-staging` |
| Test config | `sudo nginx -t` |
| Reload | `sudo systemctl reload nginx` |

Port mapping used by the Nginx config:

| Container | Host port | `proxy_pass` target |
|---|---|---|
| `motorcare-staging-web` (Next.js) | `3000` → Nginx sees `8080` | `http://127.0.0.1:8080` |
| `motorcare-staging-api` (.NET) | `5002` → Nginx sees `8081` | `http://127.0.0.1:8081` |

> Note: The compose file exposes the web container on host port `3000` and the api container on
> `5002`. The Nginx config in `docs/ops/nginx-staging.md` uses `8080` / `8081` respectively.
> Align the host ports in `docker-compose.staging.yml` (via `WEB_HOST_PORT` / `API_HOST_PORT`)
> with the `proxy_pass` ports in the Nginx config before deploying.

Add the WebSocket upgrade map inside the top-level `http {}` block in `/etc/nginx/nginx.conf`
(or `/etc/nginx/conf.d/upgrade-map.conf`):

```nginx
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      close;
}
```

---

## 5. Let's Encrypt SSL

DNS (section 1) must resolve before running these commands.

```bash
# Issue certificates for both domains in a single certbot invocation
sudo certbot certonly --nginx \
  -d staging.bakimsuite.com \
  -d staging-api.bakimsuite.com \
  --email ops@bakimsuite.com \
  --agree-tos --non-interactive
```

Certbot installs a systemd timer for automatic renewal. Verify it is active:

```bash
sudo systemctl status certbot.timer
```

Add a deploy hook so Nginx reloads after each renewal:

```bash
echo 'systemctl reload nginx' | \
  sudo tee /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
sudo chmod +x /etc/letsencrypt/renewal-hooks/deploy/reload-nginx.sh
```

---

## 6. GitHub Actions CI/CD

Workflow file: `.github/workflows/staging-ci.yml`

The pipeline runs on every push to `main` and on pull requests. Docker images are pushed to the
registry only on pushes to `main`.

**Required GitHub repository secrets:**

| Secret | Used for |
|---|---|
| `REGISTRY_URL` | Container registry hostname (used in image tags and `docker login`) |
| `REGISTRY_USERNAME` | Registry login username |
| `REGISTRY_PASSWORD` | Registry login password / token |

Images pushed on merge to `main`:

- `$REGISTRY_URL/motorcare-api:<sha>` and `$REGISTRY_URL/motorcare-api:staging-latest`
- `$REGISTRY_URL/motorcare-web:<sha>` and `$REGISTRY_URL/motorcare-web:staging-latest`

> The migrator image is listed in `staging.env.example` as `MIGRATOR_IMAGE` but is not currently
> built/pushed by `staging-ci.yml`. Add a build step for `src/MotorCare.Api/Dockerfile.migrator`
> if automated migrator deploys are needed.

---

## 7. Pre-Deploy Smoke Checklist

Run these checks manually after the stack is up and Nginx is configured.

### DNS & TLS

- [ ] `dig +short staging.bakimsuite.com` returns `46.225.166.254`
- [ ] `dig +short staging-api.bakimsuite.com` returns `46.225.166.254`
- [ ] TLS certificate valid for `staging.bakimsuite.com` (no browser warning)
- [ ] TLS certificate valid for `staging-api.bakimsuite.com` (no browser warning)

### Frontend

- [ ] `https://staging.bakimsuite.com` → HTTP 200
- [ ] `https://staging.bakimsuite.com/login` → HTTP 200
- [ ] `https://staging.bakimsuite.com/_next/static/` chunks return 200 (check browser DevTools Network)
- [ ] No unhandled JS exceptions in browser console
- [ ] No raw SQL or stack traces visible anywhere in the UI

### API

- [ ] `https://staging-api.bakimsuite.com/api/version` → HTTP 200 with JSON body
- [ ] `https://staging-api.bakimsuite.com/health` → HTTP 200
- [ ] CORS preflight (`OPTIONS`) from `https://staging.bakimsuite.com` → 204 with correct `Access-Control-Allow-Origin` header

### Auth flow

- [ ] Login with seeded credentials succeeds and redirects to dashboard
- [ ] Auth token is stored (cookie / localStorage) and subsequent API calls succeed
- [ ] Logout clears the session

---

## 8. Post-Deploy Mailpit Smoke

Mailpit captures all outbound email in staging. Its web UI is not publicly exposed; access it
via an SSH tunnel:

```bash
ssh -L 8025:localhost:8025 user@46.225.166.254
# then open http://localhost:8025 in your browser
```

### Run the auth + email smoke test

```bash
# From the repo root — requires: curl, python3, running Mailpit on port 8025
API_BASE=http://127.0.0.1:5002 \
MAILPIT_BASE=http://127.0.0.1:8025 \
  bash src/scripts/smoke/staging-auth-email-smoke.sh
```

### Run the password-reset smoke test

```bash
API_BASE=http://127.0.0.1:5002 \
MAILPIT_BASE=http://127.0.0.1:8025 \
  bash src/scripts/smoke/staging-password-reset-mailpit-smoke.sh
```

Both scripts default to `API_BASE=http://127.0.0.1:5102` (the `API_HOST_PORT` from
`staging.env.example`). Override with the actual host port set in your `.env.staging`.

Dependencies: `curl`, `python3`. Both must be installed on the machine running the scripts.

---

## 9. NO-GO Conditions (block deployment if any of these fail)

Do not sign off on a staging release if any of the following are true:

- [ ] Frontend container not healthy (`docker compose ps` shows unhealthy or restarting)
- [ ] API container not healthy (`docker compose ps` shows unhealthy or restarting)
- [ ] CORS preflight fails (browser blocks XHR/fetch to the API)
- [ ] `/_next/static/` CSS or JS chunks return 404
- [ ] Browser console shows unhandled exceptions
- [ ] Raw SQL query text or .NET stack traces visible anywhere in the UI
- [ ] HTTPS certificate invalid or expired (browser shows security warning)
- [ ] `JWT_KEY` left blank or set to `CHANGE_ME` placeholder
- [ ] `POSTGRES_PASSWORD` left blank or set to `CHANGE_ME` placeholder
- [ ] Database migrations failed (check migrator container exit code)

---

## 10. Current Known Blockers

- [ ] **DNS records not configured** — `staging.bakimsuite.com` and `staging-api.bakimsuite.com`
  A records must be pointed to `46.225.166.254` before HTTPS / certbot can proceed.
- [ ] **Inspection customer search crash** — known regression, fix in progress (Codex).
- [ ] **GitHub Actions registry secrets not set** — `REGISTRY_URL`, `REGISTRY_USERNAME`,
  `REGISTRY_PASSWORD` must be added to the GitHub repository secrets before CI can push images.
- [ ] **Migrator image not built by CI** — `staging-ci.yml` does not currently build or push the
  migrator image (`src/MotorCare.Api/Dockerfile.migrator`). Migrations must be run manually or
  a CI step must be added.
- [ ] **Nginx host port mismatch** — compose default (`WEB_HOST_PORT=3000`, `API_HOST_PORT=5002`)
  differs from the Nginx config in `docs/ops/nginx-staging.md` (expects `8080` / `8081`). Align
  before first deploy.
