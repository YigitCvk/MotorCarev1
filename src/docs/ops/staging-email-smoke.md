# Staging Email Smoke Guide

## Genel Bakış

Staging ortamında email tabanlı auth akışlarını (verification code, invite, password reset) doğrulayabilmek için **Mailpit** email capture servisi kullanılır.

Mailpit; production'a giden gerçek bir SMTP sunucusu gibi davranır ancak emaili dışarı göndermek yerine kendi web UI'ında saklar. Böylece ops ekibi gerçek email altyapısına ihtiyaç duymadan tüm auth akışlarını smoke edebilir.

> **Production'da Mailpit yoktur.** `docker-compose.production.yml` bu servisi içermez. Staging-only bir araçtır.

---

## Mailpit Web UI Erişimi

### Sunucuda SSH Tunnel ile (Önerilen)

```bash
ssh -L 8025:localhost:8025 user@staging.bakimsuite.com
# Sonra tarayıcıda: http://localhost:8025
```

### Doğrudan Sunucu Erişimi

```bash
# Sunucuda çalışıyorsa:
curl http://localhost:8025/api/v1/messages
```

Mailpit sadece `127.0.0.1:8025` üzerinde dinler (public IP'ye açık değil).

---

## Mailpit REST API

| İşlem | Endpoint |
|---|---|
| Gelen tüm emailler | `GET /api/v1/messages` |
| Email detayı (body dahil) | `GET /api/v1/message/{ID}` |
| Tüm emaili sil (test cleanup) | `DELETE /api/v1/messages` |

### Örnek — Son emaili oku:

```bash
MAILPIT_BASE="http://localhost:8025"

# Mesaj ID'lerini al
MSG_ID=$(curl -s "$MAILPIT_BASE/api/v1/messages" | jq -r '.messages[0].ID')

# Tam içeriği al (Text body 6 haneli kodu içerir)
curl -s "$MAILPIT_BASE/api/v1/message/$MSG_ID" | jq -r '.Text'
```

---

## Email Verification Code Smoke

### Akış

```
Register → Mailpit email → Code oku → verify-email-code → Login
```

### Adımlar

```bash
API="http://localhost:5102"
MAILPIT="http://localhost:8025"
TENANT="smoke-$(date +%s)"
EMAIL="owner@${TENANT}.test"

# 1. Tenant kayıt
curl -s -X POST "$API/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"tenantName\":\"Smoke Garaj\",\"ownerFullName\":\"Smoke Owner\",\"email\":\"$EMAIL\",\"password\":\"Smoke2026!\"}" | jq .

# 2. Mailpit'ten kodu oku (birkaç saniye bekle)
sleep 3
MSG_ID=$(curl -s "$MAILPIT/api/v1/messages" | jq -r '.messages[0].ID')
CODE=$(curl -s "$MAILPIT/api/v1/message/$MSG_ID" | jq -r '.Text' | grep -oP '\b\d{6}\b' | head -1)
echo "Verification code: $CODE"

# 3. Email doğrula
curl -s -X POST "$API/api/auth/verify-email-code" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\",\"code\":\"$CODE\"}" | jq .

# 4. Login
curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\",\"password\":\"Smoke2026!\"}" | jq .
```

### Doğrulanacak Noktalar

- [ ] Email konusu: `GarajPass e-posta doğrulama kodu`
- [ ] Text body'de 6 haneli kod var
- [ ] Yanlış kod → `{ "code": "EMAIL_VERIFICATION_CODE_INVALID", ... }` 
- [ ] Doğru kod → `{ "message": "..." }` ile 200
- [ ] Login sonrası `accessToken` dönüyor

---

## Invite Accept Smoke

### Akış

```
Owner login → Invite user → Mailpit email → Token oku → /accept-invite → Invited user login
```

### Adımlar

```bash
API="http://localhost:5102"
MAILPIT="http://localhost:8025"

# Owner token'ı al (önceki smoke'tan veya yeni tenant ile)
TOKEN=$(curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\",\"password\":\"Smoke2026!\"}" \
  | jq -r '.accessToken')

INVITE_EMAIL="invited-$(date +%s)@smoke.test"

# Mailpit'i temizle
curl -s -X DELETE "$MAILPIT/api/v1/messages"

# 1. Kullanıcı davet et
curl -s -X POST "$API/api/users/invite" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$INVITE_EMAIL\",\"role\":\"Technician\",\"fullName\":\"Smoke Teknisyen\"}" \
  -w "\nHTTP %{http_code}\n"

# 2. Mailpit'ten invite token'ı oku
sleep 3
MSG_ID=$(curl -s "$MAILPIT/api/v1/messages" | jq -r '.messages[0].ID')
INVITE_URL=$(curl -s "$MAILPIT/api/v1/message/$MSG_ID" | jq -r '.Text' | grep -oP 'https?://[^\s]+accept-invite\?token=[^\s]+' | head -1)
INVITE_TOKEN=$(echo "$INVITE_URL" | grep -oP '(?<=token=)[^&\s]+')
echo "Invite token: $INVITE_TOKEN"

# 3. Accept invite
curl -s -X POST "$API/api/auth/accept-invite" \
  -H "Content-Type: application/json" \
  -d "{\"token\":\"$INVITE_TOKEN\",\"fullName\":\"Smoke Teknisyen\",\"password\":\"SmokeInvite2026!\",\"confirmPassword\":\"SmokeInvite2026!\"}" | jq .

# 4. Davet edilen kullanıcı login
curl -s -X POST "$API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$INVITE_EMAIL\",\"password\":\"SmokeInvite2026!\"}" | jq .
```

### Doğrulanacak Noktalar

- [ ] Email konusu: `GarajPass'a davet edildiniz`
- [ ] Text body'de `/accept-invite?token=` içeren URL var
- [ ] Token DB'de plaintext değil (hash olarak saklanıyor)
- [ ] Accept sonrası kullanıcı login olabiliyor
- [ ] Accept sonrası aynı token ile ikinci accept → `401`
- [ ] Pending kullanıcı accept olmadan login → `{ "code": "EMAIL_NOT_VERIFIED", ... }`
- [ ] Resend invite → eski token revoke oluyor, yeni token geliyor

---

## Password Reset Smoke

```bash
# 1. Şifre sıfırlama isteği
curl -s -X POST "$API/api/auth/forgot-password" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\"}"

# 2. Mailpit'ten kodu oku
sleep 3
MSG_ID=$(curl -s "$MAILPIT/api/v1/messages" | jq -r '.messages[0].ID')
CODE=$(curl -s "$MAILPIT/api/v1/message/$MSG_ID" | jq -r '.Text' | grep -oP '\b\d{6}\b' | head -1)

# 3. Yeni şifre belirle
curl -s -X POST "$API/api/auth/reset-password" \
  -H "Content-Type: application/json" \
  -d "{\"tenantIdentifier\":\"$TENANT\",\"email\":\"$EMAIL\",\"code\":\"$CODE\",\"newPassword\":\"NewSmoke2026!\",\"confirmPassword\":\"NewSmoke2026!\"}" | jq .
```

---

## Production Güvenliği

| Özellik | Staging | Production |
|---|---|---|
| Mailpit container | ✅ Var | ❌ Yok |
| SMTP hedefi | `mailpit:1025` | Gerçek SMTP |
| Email body logda | ❌ Hayır | ❌ Hayır |
| Verification code logda | ❌ Hayır | ❌ Hayır |
| Invite token logda | ❌ Hayır | ❌ Hayır |
| Token DB'de | Hash (SHA-256) | Hash (SHA-256) |

Email içeriği hiçbir ortamda loglara yazılmaz (sadece Development + `LogEmailBodyInDevelopment=true` açıksa). Staging'de bu flag her zaman `false`'dur.

---

## Otomatik Smoke Script

Tüm akışı tek komutla çalıştırmak için:

```bash
# Sunucuya SSH tunnel açıkken veya doğrudan sunucuda:
bash src/scripts/smoke/staging-auth-email-smoke.sh
```

Detaylar için: [staging-auth-email-smoke.sh](../../scripts/smoke/staging-auth-email-smoke.sh)

---

## Sorun Giderme

### Email Mailpit'e düşmüyor

```bash
# API email config kontrolü
docker exec motorcare-staging-api env | grep Email__

# Mailpit container ayakta mı?
docker ps | grep mailpit

# SmtpHost mailpit olarak ayarlanmış mı?
# Email__SmtpHost=mailpit, Email__SmtpPort=1025, Email__SendEmails=true olmalı
```

### Mailpit'e erişilemiyor

```bash
# Localhost port açık mı?
curl -s http://localhost:8025/api/v1/messages

# SSH tunnel:
ssh -L 8025:localhost:8025 user@staging.bakimsuite.com
```

### Verification code bulunamıyor

```bash
# Mailpit'teki tüm mesajları listele
curl -s http://localhost:8025/api/v1/messages | jq '.messages[] | {id: .ID, subject: .Subject, to: .To}'
```
