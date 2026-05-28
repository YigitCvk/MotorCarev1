# Production Secrets

Production secrets are not stored in this repository. Use the Portainer stack environment UI or a root-owned server-side env file with restricted permissions. Never commit populated env files or copied Portainer exports containing real values.

This repository is backend-only. The production stack must contain only `postgres`, `api`, and `migrator`. The frontend is deployed as a separate app/repository and its browser origin must be added to `CORS_ALLOWED_ORIGINS`.

## Required Secrets

Production deploy is blocked unless these values are present and non-placeholder:

- `POSTGRES_PASSWORD`
- `JWT_KEY`
- `Email__SmtpUsername`
- `Email__SmtpPassword`

Optional external-service secrets, required only when the service is enabled:

- `ELASTIC_USERNAME`
- `ELASTIC_PASSWORD`

## Required Non-Secret Runtime Values

Review these before every production deploy:

- `ALLOWED_HOSTS`
- `API_HOST_PORT`
- `CORS_ALLOWED_ORIGINS`
- `Email__AppBaseUrl`
- `Email__FromEmail`
- `Email__FromName`
- `Email__SmtpHost`
- `Email__SmtpPort`
- `Email__EnableSsl`
- `Email__SendEmails`
- `Storage__AttachmentsPath`
- `API_IMAGE`
- `MIGRATOR_IMAGE`
- `MOTORCARE_COMMIT_SHA`
- `MOTORCARE_BUILD_TIME`

Backend-only note: there is no `APP_IMAGE`, `APP_HOST_PORT`, frontend, web, or static-site container in the production stack.

## CORS And Frontend URL

- `CORS_ALLOWED_ORIGINS` must include the production frontend browser origins, separated with semicolons.
- `Email__AppBaseUrl` must be the production frontend URL used for invite, verification, and password reset links.
- Do not set `Email__AppBaseUrl` to the API origin unless the frontend is actually served from that origin.

Example shape without real secrets:

```text
CORS_ALLOWED_ORIGINS=https://garajpass.com;https://www.garajpass.com
Email__AppBaseUrl=https://garajpass.com
```

## Production NO-GO Preflight

Do not deploy or promote production if any item below is true:

- Any real secret appears in a git-tracked file, shell history pasted into docs, issue comments, or deployment notes.
- Any required secret is blank, still a placeholder, or copied from staging.
- `JWT_KEY` is short, reused from another environment, or known to more people than the production operators.
- `Email__SmtpUsername` or `Email__SmtpPassword` is missing while `Email__SendEmails=true`.
- `Email__SmtpHost` still points to `mailpit`.
- Mailpit or any test-only email capture service is present in the production stack.
- Old frontend/app/web services still exist in the Portainer stack after backend-only migration.
- `CORS_ALLOWED_ORIGINS` omits the production frontend origin or includes broad/untrusted origins.
- `Email__AppBaseUrl` points to staging, localhost, or the API origin by mistake.
- `API_IMAGE` and `MIGRATOR_IMAGE` do not reference the intended production image tags.
- Database and attachment backups have not been verified for the release window.
- The migrator has not been run for a release with pending EF migrations.
- API `/health` does not pass after deployment.
