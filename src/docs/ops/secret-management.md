# GarajPass Secret Management

## Secret Inventory

Required deployment secrets:

- `POSTGRES_PASSWORD`
- `JWT_KEY`
- `Email__SmtpUsername` and `Email__SmtpPassword` when outbound SMTP is enabled

Optional secrets when the integration is enabled:

- `ELASTIC_USERNAME`
- `ELASTIC_PASSWORD`
- `PORTAINER_STAGING_WEBHOOK`
- registry or CI credentials such as `GITHUB_TOKEN`

Non-secrets such as ports, image tags, issuer names, base URLs, and SEO settings stay in env templates so deploys remain reviewable.

## Storage Rules

- Keep real values in Portainer or in a root-owned env file with `chmod 600`.
- Use `deploy/portainer/*.env.example` only as templates; the required secret fields are intentionally blank.
- Keep populated `.env*` files out of the repo, tickets, screenshots, shell history, and terminal transcripts.
- Prefer `docker compose ... config --quiet` for validation. Plain `docker compose config` renders expanded env values and can leak secrets into output or temp files.
- Database dumps are sensitive artifacts. Store them outside the checkout, restrict access, encrypt them when leaving the host, and apply retention.

## Local Guardrails

- `.gitignore` excludes populated env files and dump artifacts.
- `.dockerignore` excludes populated env files and dump artifacts from build contexts.
- Staging defaults `Email__SendEmails=false`; turn it on only after SMTP credentials and recipient expectations are reviewed.

## Rotation Notes

- `POSTGRES_PASSWORD`: rotate with a coordinated database-user update and app/migrator restart.
- `JWT_KEY`: rotate during a planned window; existing access tokens become invalid.
- SMTP credentials: create the replacement credential first, update deploy secrets, smoke mail delivery, then revoke the old credential.
- Elastic credentials: update both the upstream service and deploy secret before restarting the API.
- Portainer webhooks and CI tokens: rotate after exposure, maintainer changes, or scope changes.

## Review Checklist

Before a deploy:

1. Required secret fields are populated outside git.
2. Env examples still contain blanks, not real or reusable values.
3. `docker compose ... config --quiet` passes for the target stack.
4. Backup location is outside the repo and has restricted permissions.
5. No secret values were copied into docs, chat, screenshots, or release notes.
