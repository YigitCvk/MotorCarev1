#!/usr/bin/env bash
# Focused Mailpit-backed password reset smoke.
#
# Usage:
#   bash scripts/smoke/staging-password-reset-mailpit-smoke.sh
#
# Env overrides:
#   API_BASE=http://127.0.0.1:5102 MAILPIT_BASE=http://127.0.0.1:8025 bash scripts/smoke/staging-password-reset-mailpit-smoke.sh

set -euo pipefail

API_BASE="${API_BASE:-http://127.0.0.1:5102}"
MAILPIT_BASE="${MAILPIT_BASE:-http://127.0.0.1:8025}"
MAIL_WAIT_SECONDS="${MAIL_WAIT_SECONDS:-20}"
STAMP="$(date -u +%Y%m%d%H%M%S)-$$"
TENANT="reset-smoke-${STAMP}"
OWNER_EMAIL="reset.${STAMP}@smoke.test"
OLD_PASSWORD="ResetOld${STAMP}!"
NEW_PASSWORD="ResetNew${STAMP}!"
REUSE_PASSWORD="ResetReuse${STAMP}!"

require_cmd() {
    command -v "$1" >/dev/null 2>&1 || {
        echo "FAIL missing dependency: $1"
        exit 1
    }
}

ok() {
    echo "OK $*"
}

fail() {
    echo "FAIL $*"
    exit 1
}

api_post() {
    local path="$1"
    local payload="$2"
    curl -sS -w $'\n__STATUS__%{http_code}' \
        -X POST "${API_BASE}${path}" \
        -H "Content-Type: application/json" \
        -d "$payload"
}

response_status() {
    printf '%s\n' "$1" | awk -F'__STATUS__' '/__STATUS__/ { print $2 }' | tail -n 1
}

expect_status() {
    local response="$1"
    local expected="$2"
    local label="$3"
    local actual
    actual="$(response_status "$response")"

    if [ "$actual" != "$expected" ]; then
        fail "${label}: expected HTTP ${expected}, got ${actual:-missing}"
    fi

    ok "${label}: HTTP ${actual}"
}

mailpit_delete_all() {
    curl -fsS -X DELETE "${MAILPIT_BASE}/api/v1/messages" >/dev/null
}

mailpit_latest_text() {
    local deadline=$((SECONDS + MAIL_WAIT_SECONDS))
    local message_id

    while [ "$SECONDS" -lt "$deadline" ]; do
        message_id="$(
            curl -fsS "${MAILPIT_BASE}/api/v1/messages" |
                python3 -c 'import json, sys; data = json.load(sys.stdin); messages = data.get("messages") or []; print(messages[0].get("ID", "") if messages else "")'
        )"

        if [ -n "$message_id" ]; then
            curl -fsS "${MAILPIT_BASE}/api/v1/message/${message_id}" |
                python3 -c 'import json, sys; print((json.load(sys.stdin).get("Text") or ""), end="")'
            return 0
        fi

        sleep 1
    done

    return 1
}

extract_numeric_code() {
    python3 -c 'import re, sys; match = re.search(r"\b\d{6}\b", sys.stdin.read()); print(match.group(0) if match else "", end="")'
}

require_cmd curl
require_cmd python3

curl -fsS "${API_BASE}/health" >/dev/null
ok "api health"
curl -fsS "${MAILPIT_BASE}/api/v1/messages" >/dev/null
ok "mailpit health"

mailpit_delete_all
response="$(
    api_post "/api/auth/register" \
        "{\"tenantIdentifier\":\"${TENANT}\",\"tenantName\":\"Password Reset Smoke\",\"ownerFullName\":\"Password Reset Smoke Owner\",\"ownerEmail\":\"${OWNER_EMAIL}\",\"ownerPassword\":\"${OLD_PASSWORD}\"}"
)"
expect_status "$response" "201" "register owner"

verification_text="$(mailpit_latest_text)" || fail "verification email was not captured"
verification_code="$(printf '%s' "$verification_text" | extract_numeric_code)"
[ -n "$verification_code" ] || fail "verification email did not contain a numeric code"
ok "verification email captured"

response="$(
    api_post "/api/auth/verify-email-code" \
        "{\"tenantIdentifier\":\"${TENANT}\",\"email\":\"${OWNER_EMAIL}\",\"code\":\"${verification_code}\"}"
)"
expect_status "$response" "200" "verify owner email"

response="$(
    api_post "/api/auth/login" \
        "{\"tenantIdentifier\":\"${TENANT}\",\"email\":\"${OWNER_EMAIL}\",\"password\":\"${OLD_PASSWORD}\"}"
)"
expect_status "$response" "200" "old password before reset"

mailpit_delete_all
response="$(api_post "/api/auth/forgot-password" "{\"email\":\"${OWNER_EMAIL}\"}")"
expect_status "$response" "200" "forgot password"

reset_text="$(mailpit_latest_text)" || fail "reset email was not captured"
reset_code="$(printf '%s' "$reset_text" | extract_numeric_code)"
[ -n "$reset_code" ] || fail "reset email did not contain a numeric code"
ok "reset email captured"

response="$(
    api_post "/api/auth/reset-password" \
        "{\"email\":\"${OWNER_EMAIL}\",\"code\":\"${reset_code}\",\"newPassword\":\"${NEW_PASSWORD}\",\"confirmPassword\":\"${NEW_PASSWORD}\"}"
)"
expect_status "$response" "200" "reset password"

response="$(
    api_post "/api/auth/login" \
        "{\"tenantIdentifier\":\"${TENANT}\",\"email\":\"${OWNER_EMAIL}\",\"password\":\"${OLD_PASSWORD}\"}"
)"
expect_status "$response" "401" "old password rejected after reset"

response="$(
    api_post "/api/auth/login" \
        "{\"tenantIdentifier\":\"${TENANT}\",\"email\":\"${OWNER_EMAIL}\",\"password\":\"${NEW_PASSWORD}\"}"
)"
expect_status "$response" "200" "new password accepted"

response="$(
    api_post "/api/auth/reset-password" \
        "{\"email\":\"${OWNER_EMAIL}\",\"code\":\"${reset_code}\",\"newPassword\":\"${REUSE_PASSWORD}\",\"confirmPassword\":\"${REUSE_PASSWORD}\"}"
)"
expect_status "$response" "401" "used reset code rejected"

echo "PASSWORD RESET MAILPIT SMOKE PASSED"
