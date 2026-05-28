# Staging Email Smoke Guide

This repository is backend-only. Frontend code and frontend deployment live in a separate app/repository. `Email__AppBaseUrl` must point to that separately deployed frontend URL, while the smoke commands below call backend API endpoints directly.

## Scope

Staging uses Mailpit to capture auth emails without sending real email. Production must not run Mailpit.

Staging Portainer stack services:

- `postgres`
- `api`
- `mailpit`
- `migrator` under the `tools` profile

## Mailpit Access

Mailpit is bound to localhost by the staging compose file:

```bash
ssh -L 8025:localhost:8025 user@staging.bakimsuite.com
```

Open `http://localhost:8025` after the tunnel is active.

Mailpit REST API:

```bash
curl -s http://localhost:8025/api/v1/messages | jq .
curl -s http://localhost:8025/api/v1/message/MESSAGE_ID | jq -r '.Text'
curl -s -X DELETE http://localhost:8025/api/v1/messages
```

## API Smoke Defaults

Use the public staging API origin when testing from your workstation:

```bash
API="https://staging-api.bakimsuite.com"
MAILPIT="http://localhost:8025"
```

Use the host-local API port only when running directly on the staging server:

```bash
API="http://127.0.0.1:${API_HOST_PORT:-5102}"
MAILPIT="http://127.0.0.1:${MAILPIT_WEB_PORT:-8025}"
```

## Email Verification Smoke

```bash
TENANT="smoke-$(date +%s)"
EMAIL="owner@${TENANT}.test"
PASSWORD="Smoke2026!"

curl -s -X DELETE "$MAILPIT/api/v1/messages" >/dev/null

curl -s -X POST "$API/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"tenantName\":\"Smoke Garage\",\"ownerFullName\":\"Smoke Owner\",\"ownerEmail\":\"$EMAIL\",\"ownerPassword\":\"$PASSWORD\"}" | jq .

sleep 3
MSG_ID=$(curl -s "$MAILPIT/api/v1/messages" | jq -r '.messages[0].ID')
CODE=$(curl -s "$MAILPIT/api/v1/message/$MSG_ID" | jq -r '.Text' | grep -oE '\b[0-9]{6}\b' | head -1)
echo "Verification code: $CODE"

curl -s -X POST "$API/api/auth/verify-email-code" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\",\"code\":\"$CODE\"}" | jq .

curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}" | jq .
```

Expected checks:

- Register returns success and Mailpit receives one verification email.
- Email body contains a 6 digit verification code.
- `verify-email-code` returns HTTP 200.
- Login returns an `accessToken` and `refreshToken`.

## Invite Smoke

Run this after the verification smoke so `TENANT`, `EMAIL`, and `PASSWORD` are still set.

```bash
OWNER_TOKEN=$(curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}" \
  | jq -r '.accessToken')

INVITE_EMAIL="invited-$(date +%s)@smoke.test"

curl -s -X DELETE "$MAILPIT/api/v1/messages" >/dev/null

curl -s -X POST "$API/api/users/invite" \
  -H "Authorization: Bearer $OWNER_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$INVITE_EMAIL\",\"role\":4,\"fullName\":\"Smoke Technician\"}" \
  -w "\nHTTP %{http_code}\n"

sleep 3
MSG_ID=$(curl -s "$MAILPIT/api/v1/messages" | jq -r '.messages[0].ID')
INVITE_URL=$(curl -s "$MAILPIT/api/v1/message/$MSG_ID" | jq -r '.Text' | grep -oE 'https?://[^[:space:]]+accept-invite\?token=[^[:space:]]+' | head -1)
INVITE_TOKEN=$(printf '%s' "$INVITE_URL" | sed -E 's/.*token=([^&[:space:]]+).*/\1/')

curl -s -X POST "$API/api/auth/accept-invite" \
  -H "Content-Type: application/json" \
  -d "{\"token\":\"$INVITE_TOKEN\",\"fullName\":\"Smoke Technician\",\"password\":\"SmokeInvite2026!\",\"confirmPassword\":\"SmokeInvite2026!\"}" | jq .

curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$INVITE_EMAIL\",\"password\":\"SmokeInvite2026!\"}" | jq .
```

Expected checks:

- Invite email contains an `accept-invite?token=` URL.
- Invited user can accept once and then login.
- Reusing the same invite token fails.

## Password Reset Smoke

```bash
curl -s -X DELETE "$MAILPIT/api/v1/messages" >/dev/null

curl -s -X POST "$API/api/auth/forgot-password" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\"}" | jq .

sleep 3
MSG_ID=$(curl -s "$MAILPIT/api/v1/messages" | jq -r '.messages[0].ID')
RESET_CODE=$(curl -s "$MAILPIT/api/v1/message/$MSG_ID" | jq -r '.Text' | grep -oE '\b[0-9]{6}\b' | head -1)

curl -s -X POST "$API/api/auth/reset-password" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\",\"code\":\"$RESET_CODE\",\"newPassword\":\"NewSmoke2026!\",\"confirmPassword\":\"NewSmoke2026!\"}" | jq .
```

Expected checks:

- Password reset email lands in Mailpit.
- Reset with the code returns success.
- Login with the old password fails and login with the new password succeeds.

## CORS And Frontend URL

- `CORS_ALLOWED_ORIGINS` controls browser origins allowed to call the API.
- `Email__AppBaseUrl` controls links placed in auth emails and should be the separate frontend app URL.
- For multiple origins, use semicolon-separated values, for example `https://staging.bakimsuite.com;https://preview.example.com`.
- Do not add frontend containers to this backend repository stack.

## Troubleshooting

```bash
docker ps --filter name=motorcare-staging
docker logs --tail 100 motorcare-staging-api
docker exec motorcare-staging-api env | grep -E 'Cors__|Email__'
curl -fsS http://127.0.0.1:5102/health
curl -s http://127.0.0.1:8025/api/v1/messages | jq '.messages[] | {id: .ID, subject: .Subject, to: .To}'
```

If Mailpit is empty, confirm `Email__SmtpHost=mailpit`, `Email__SmtpPort=1025`, and `Email__SendEmails=true` in the Portainer stack environment.
