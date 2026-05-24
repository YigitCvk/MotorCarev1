# MotorCare / BakimSuite Backend

Bu repo şu an backend-only base olarak düzenlenmiştir. Eski Blazor frontend devreden çıkarılmıştır. Frontend repository/application yeniden oluşturulacaktır.

## Korunan Backend Yapısı

- `src/MotorCare.Api`: Carter tabanlı .NET 8 API, auth, tenant, public QR ve smoke endpointleri.
- `src/MotorCare.Application`: CQRS/MediatR use case katmanı ve DTO/validator yapıları.
- `src/MotorCare.Domain`: DDD aggregate, value object ve domain davranışları.
- `src/MotorCare.Infrastructure`: EF Core persistence, migrations, email, security, import ve repository altyapısı.
- `tests/`: Domain ve application unit testleri.

## Çalıştırma

```powershell
dotnet restore .\src\MotorCare.sln
dotnet build .\src\MotorCare.sln -c Release
dotnet test .\src\MotorCare.sln -c Release
```

Local backend stack:

```powershell
docker compose up -d postgres api
docker compose --profile tools run --rm migrator
```

## Deploy

- Local compose: `docker-compose.yml`
- Staging compose: `src/docker-compose.staging.yml`
- Production compose: `src/docker-compose.production.yml`
- Portainer env örnekleri: `src/deploy/portainer/*.env.example`
- Backup/restore scriptleri: `src/scripts/backup-*`, `src/scripts/restore-*`
- Staging auth/email smoke scriptleri: `src/scripts/smoke/`

Frontend container artık bu repo içindeki compose dosyalarında yer almaz. Yeni frontend ayrı uygulama olarak API base URL kullanacaktır.

## Frontend Entegrasyon Notları

- API base URL staging: `https://staging-api.bakimsuite.com`
- API base URL production: production API domaini için `src/deploy/nginx/garajpass.production.conf.template` güncellenir.
- CORS izinleri `Cors:AllowedOrigins` veya compose env `CORS_ALLOWED_ORIGINS` ile yönetilir.
- Auth token contract backend tarafında kalır: login access/refresh token döndürür, refresh token rotation endpointi korunur.
- Public QR endpointleri backend API'de kalır; yeni frontend bu endpointleri kendi route'larından çağıracaktır.
