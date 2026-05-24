# Production Secrets

Production secrets are not stored in this repository. Use the Portainer stack env UI or a root-owned server-side env file with restricted permissions.

Required values:

- `POSTGRES_PASSWORD`
- `JWT_KEY`
- `Email__SmtpUsername`
- `Email__SmtpPassword`

Reviewed non-secret runtime values:

- `ALLOWED_HOSTS`
- `API_HOST_PORT`
- `CORS_ALLOWED_ORIGINS`
- `Email__AppBaseUrl`
- `Storage__AttachmentsPath`
- `API_IMAGE`
- `MIGRATOR_IMAGE`

Backend-only note: there is no `APP_IMAGE`, `APP_HOST_PORT`, or web container in the production stack. The new frontend must be deployed separately and its browser origin must be added to `CORS_ALLOWED_ORIGINS`.
