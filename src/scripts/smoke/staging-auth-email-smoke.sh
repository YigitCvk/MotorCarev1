#!/usr/bin/env bash
# staging-auth-email-smoke.sh
# Full uçtan uca email auth smoke testi.
# Gereksinim: curl, jq, Mailpit çalışıyor olmalı.
#
# Kullanım:
#   bash staging-auth-email-smoke.sh
#
# Env override:
#   API_BASE=http://localhost:5102 MAILPIT_BASE=http://localhost:8025 bash staging-auth-email-smoke.sh

set -euo pipefail

# ── Konfigürasyon ──────────────────────────────────────────────────────────
API_BASE="${API_BASE:-http://localhost:5102}"
MAILPIT_BASE="${MAILPIT_BASE:-http://localhost:8025}"
SUFFIX="$(date +%s)"
TENANT="smoke${SUFFIX}"
OWNER_EMAIL="owner@${TENANT}.test"
OWNER_PASS="SmokeOwner2026!"
INVITE_EMAIL="tech${SUFFIX}@${TENANT}.test"
INVITE_PASS="SmokeInvite2026!"
PASS="${PASS:-}"

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'

ok()   { echo -e "${GREEN}  ✓ $*${NC}"; }
fail() { echo -e "${RED}  ✗ $*${NC}"; exit 1; }
info() { echo -e "${YELLOW}► $*${NC}"; }

require_cmd() { command -v "$1" >/dev/null 2>&1 || { echo "Required: $1"; exit 1; }; }
require_cmd curl
require_cmd jq

# ── Yardımcı ──────────────────────────────────────────────────────────────
mailpit_latest_message_id() {
    local subject_hint="${1:-}"
    local max_wait=15
    local waited=0
    while [ $waited -lt $max_wait ]; do
        local id
        if [ -n "$subject_hint" ]; then
            id=$(curl -s "$MAILPIT_BASE/api/v1/messages" \
                | jq -r --arg h "$subject_hint" \
                    '.messages[] | select(.Subject | contains($h)) | .ID' \
                | head -1)
        else
            id=$(curl -s "$MAILPIT_BASE/api/v1/messages" | jq -r '.messages[0].ID // empty')
        fi
        [ -n "$id" ] && [ "$id" != "null" ] && { echo "$id"; return; }
        sleep 2; waited=$((waited+2))
    done
    fail "Mailpit'te beklenen email $max_wait saniye içinde gelmedi (hint: $subject_hint)"
}

mailpit_get_text() {
    local msg_id="$1"
    curl -s "$MAILPIT_BASE/api/v1/message/$msg_id" | jq -r '.Text'
}

mailpit_delete_all() {
    curl -s -X DELETE "$MAILPIT_BASE/api/v1/messages" >/dev/null
}

api_post() {
    local path="$1"; local data="$2"; local token="${3:-}"
    local auth_header=""
    [ -n "$token" ] && auth_header="-H \"Authorization: Bearer $token\""
    curl -s -w "\n__STATUS__%{http_code}" \
        -X POST "${API_BASE}${path}" \
        -H "Content-Type: application/json" \
        ${token:+-H "Authorization: Bearer $token"} \
        -d "$data"
}

check_status() {
    local response="$1"; local expected="$2"; local label="$3"
    local status
    status=$(echo "$response" | grep "__STATUS__" | grep -oP '\d+$')
    local body
    body=$(echo "$response" | grep -v "__STATUS__")
    if [ "$status" = "$expected" ]; then
        ok "$label → HTTP $status"
    else
        echo "  Body: $body"
        fail "$label → beklenen HTTP $expected, alınan $status"
    fi
    echo "$body"
}

# ── Mailpit erişim kontrolü ───────────────────────────────────────────────
info "Mailpit bağlantısı kontrol ediliyor..."
if ! curl -sf "$MAILPIT_BASE/api/v1/messages" >/dev/null; then
    fail "Mailpit'e erişilemiyor: $MAILPIT_BASE — SSH tunnel açık mı? (ssh -L 8025:localhost:8025 user@server)"
fi
ok "Mailpit erişilebilir"

# ── API sağlık kontrolü ───────────────────────────────────────────────────
info "API sağlık kontrolü..."
if ! curl -sf "$API_BASE/health" >/dev/null; then
    fail "API'ye erişilemiyor: $API_BASE"
fi
ok "API sağlıklı"

echo ""
info "━━━ SENARYO 1: Email Verification ━━━"
info "Tenant: $TENANT | Owner: $OWNER_EMAIL"

# Mailpit'i temizle
mailpit_delete_all

# ── 1. Register ───────────────────────────────────────────────────────────
info "1/8 Register tenant..."
RESP=$(api_post "/api/auth/register" \
    "{\"tenantIdentifier\":\"$TENANT\",\"tenantName\":\"Smoke Garaj $SUFFIX\",\"ownerFullName\":\"Smoke Owner\",\"email\":\"$OWNER_EMAIL\",\"password\":\"$OWNER_PASS\"}")
check_status "$RESP" "204" "Register" >/dev/null

# ── 2. Verification code'u Mailpit'ten al ─────────────────────────────────
info "2/8 Verification email bekleniyor..."
MSG_ID=$(mailpit_latest_message_id "doğrulama kodu")
TEXT=$(mailpit_get_text "$MSG_ID")
CODE=$(echo "$TEXT" | grep -oP '\b\d{6}\b' | head -1)
[ -n "$CODE" ] || fail "Mailpit email body'sinde 6 haneli kod bulunamadı"
ok "Verification code alındı: $CODE"

# ── 3. Yanlış kod dene ────────────────────────────────────────────────────
info "3/8 Yanlış kod ile doğrulama (friendly error bekleniyor)..."
RESP=$(api_post "/api/auth/verify-email-code" \
    "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$OWNER_EMAIL\",\"code\":\"000000\"}")
STATUS=$(echo "$RESP" | grep "__STATUS__" | grep -oP '\d+$')
BODY=$(echo "$RESP" | grep -v "__STATUS__")
ERR_CODE=$(echo "$BODY" | jq -r '.code // empty')
if [ "$STATUS" != "200" ] && [ "$STATUS" != "204" ]; then
    ok "Yanlış kod reddedildi → HTTP $STATUS, code=$ERR_CODE"
else
    fail "Yanlış kod kabul edildi!"
fi

# ── 4. Doğru kod ile verify ───────────────────────────────────────────────
info "4/8 Doğru kod ile doğrulama..."
RESP=$(api_post "/api/auth/verify-email-code" \
    "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$OWNER_EMAIL\",\"code\":\"$CODE\"}")
check_status "$RESP" "200" "Email verification" >/dev/null

# ── 5. Login ──────────────────────────────────────────────────────────────
info "5/8 Owner login..."
RESP=$(api_post "/api/auth/login" \
    "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$OWNER_EMAIL\",\"password\":\"$OWNER_PASS\"}")
BODY=$(check_status "$RESP" "200" "Owner login")
ACCESS_TOKEN=$(echo "$BODY" | jq -r '.accessToken')
[ -n "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "null" ] || fail "accessToken alınamadı"
ok "Access token alındı"

echo ""
info "━━━ SENARYO 2: Invite Accept ━━━"
info "Invite email: $INVITE_EMAIL"

# Mailpit'i temizle
mailpit_delete_all

# ── 6. Kullanıcı davet et ─────────────────────────────────────────────────
info "6/8 Kullanıcı davet ediliyor..."
RESP=$(api_post "/api/users/invite" \
    "{\"email\":\"$INVITE_EMAIL\",\"role\":\"Technician\",\"fullName\":\"Smoke Teknisyen\"}" \
    "$ACCESS_TOKEN")
check_status "$RESP" "204" "Invite user" >/dev/null

# ── 7. Invite token'ı Mailpit'ten al ─────────────────────────────────────
info "7/8 Invite email bekleniyor..."
MSG_ID=$(mailpit_latest_message_id "davet")
TEXT=$(mailpit_get_text "$MSG_ID")
INVITE_URL=$(echo "$TEXT" | grep -oP 'https?://[^\s]+accept-invite\?token=[^\s]+' | head -1)
INVITE_TOKEN=$(echo "$INVITE_URL" | grep -oP '(?<=token=)[^&\s]+' | head -1)
[ -n "$INVITE_TOKEN" ] || fail "Invite token URL'si Mailpit email'inde bulunamadı"
ok "Invite token alındı (${#INVITE_TOKEN} karakter)"

# ── 7b. Pending kullanıcı accept olmadan login edemez ────────────────────
info "7b/8 Pending kullanıcı login denemesi (reddedilmeli)..."
RESP=$(api_post "/api/auth/login" \
    "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$INVITE_EMAIL\",\"password\":\"herhangi\"}")
STATUS=$(echo "$RESP" | grep "__STATUS__" | grep -oP '\d+$')
[ "$STATUS" = "401" ] || fail "Pending kullanıcı login'i kabul edildi — beklenen 401, alınan $STATUS"
ok "Pending kullanıcı login reddedildi → 401"

# ── 8. Accept invite ──────────────────────────────────────────────────────
info "8/8 Invite accept..."
RESP=$(api_post "/api/auth/accept-invite" \
    "{\"token\":\"$INVITE_TOKEN\",\"fullName\":\"Smoke Teknisyen\",\"password\":\"$INVITE_PASS\",\"confirmPassword\":\"$INVITE_PASS\"}")
check_status "$RESP" "200" "Accept invite" >/dev/null

# ── 8b. Aynı token ikinci kez kullanılamaz ────────────────────────────────
info "8b. Tüketilen token ile ikinci accept (reddedilmeli)..."
RESP=$(api_post "/api/auth/accept-invite" \
    "{\"token\":\"$INVITE_TOKEN\",\"fullName\":\"x\",\"password\":\"$INVITE_PASS\",\"confirmPassword\":\"$INVITE_PASS\"}")
STATUS=$(echo "$RESP" | grep "__STATUS__" | grep -oP '\d+$')
[ "$STATUS" = "401" ] || fail "Tüketilmiş invite token ikinci kez kabul edildi — beklenen 401, alınan $STATUS"
ok "Tüketilmiş token reddedildi → 401"

# ── 8c. Davet edilen kullanıcı login ─────────────────────────────────────
info "8c. Davet edilen kullanıcı login..."
RESP=$(api_post "/api/auth/login" \
    "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$INVITE_EMAIL\",\"password\":\"$INVITE_PASS\"}")
BODY=$(check_status "$RESP" "200" "Invited user login")
INVITED_TOKEN=$(echo "$BODY" | jq -r '.accessToken')
[ -n "$INVITED_TOKEN" ] && [ "$INVITED_TOKEN" != "null" ] || fail "Davet edilen kullanıcı accessToken alınamadı"
ok "Davet edilen kullanıcı login başarılı"

# ── Özet ──────────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}  EMAIL AUTH SMOKE: TÜM KONTROLLER BAŞARILI  ✓${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""
echo "  Tenant          : $TENANT"
echo "  Owner email     : $OWNER_EMAIL"
echo "  Invited email   : $INVITE_EMAIL"
echo ""
echo "Checklist:"
echo "  ✓ Register tenant"
echo "  ✓ Verification code Mailpit'ten alındı"
echo "  ✓ Yanlış kod reddedildi (friendly error)"
echo "  ✓ Doğru kod ile email doğrulandı"
echo "  ✓ Owner login başarılı"
echo "  ✓ Kullanıcı davet edildi"
echo "  ✓ Invite token Mailpit'ten alındı"
echo "  ✓ Pending kullanıcı accept olmadan login edemedi"
echo "  ✓ Invite accept başarılı"
echo "  ✓ Tüketilmiş token reddedildi"
echo "  ✓ Davet edilen kullanıcı login başarılı"
