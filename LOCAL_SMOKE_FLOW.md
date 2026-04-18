# Local Smoke Flow

This runbook covers the minimum local flow for `MotorCare.Api` with PostgreSQL, onboarding, JWT login, and protected Swagger calls.

## Prerequisites

- Docker Desktop or a compatible Docker runtime
- .NET SDK 8+
- The repo root as the working directory

## 1. Start PostgreSQL

```powershell
docker compose up -d
```

This starts PostgreSQL on `localhost:5432` with:

- Database: `motorcare`
- Username: `motorcare`
- Password: `motorcare_dev_password`

Equivalent one-off Docker run:

```powershell
docker run --name motorcare-postgres `
  -e POSTGRES_DB=motorcare `
  -e POSTGRES_USER=motorcare `
  -e POSTGRES_PASSWORD=motorcare_dev_password `
  -p 5432:5432 `
  -d postgres:16-alpine
```

## 2. Local connection string

`MotorCare.Api` now reads the local development connection string from:

- `src/MotorCare.Api/appsettings.Development.json`

Current value:

```powershell
Host=localhost;Port=5432;Database=motorcare;Username=motorcare;Password=motorcare_dev_password
```

If you need to override it temporarily in PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=motorcare;Username=motorcare;Password=motorcare_dev_password"
```

## 3. Update the database schema

Apply the current EF Core migrations:

```powershell
dotnet ef database update --project src/MotorCare.Infrastructure/MotorCare.Infrastructure.csproj --startup-project src/MotorCare.Api/MotorCare.Api.csproj
```

If the command fails because the checked-in migrations are behind the current model, create a new migration first:

```powershell
dotnet ef migrations add LocalSmokeFlow --project src/MotorCare.Infrastructure/MotorCare.Infrastructure.csproj --startup-project src/MotorCare.Api/MotorCare.Api.csproj
dotnet ef database update --project src/MotorCare.Infrastructure/MotorCare.Infrastructure.csproj --startup-project src/MotorCare.Api/MotorCare.Api.csproj
```

Note:
- The service-order number generator now depends on the `ServiceOrderNumberCounters` table.
- That table must exist in the local database before `POST /api/service-orders` is exercised.

## 4. Run the API

```powershell
dotnet run --project src/MotorCare.Api/MotorCare.Api.csproj
```

Open Swagger:

- `https://localhost:7xxx/swagger`
- or `http://localhost:5xxx/swagger`

Use the actual URLs shown by the API startup output.

## 5. Onboard the first tenant owner

In Swagger, call:

- `POST /api/onboarding/tenant`

Sample request:

```json
{
  "tenantIdentifier": "asya-motor",
  "tenantName": "Asya Motor",
  "ownerFullName": "Yigit Cevik",
  "ownerEmail": "owner@asyamotor.com",
  "ownerPassword": "Password123!"
}
```

Expected result:

- `201 Created`
- response contains `tenantId`, `tenantIdentifier`, `ownerId`, and `ownerEmail`

## 6. Login and get tokens

In Swagger, call:

- `POST /api/auth/login`

Sample request:

```json
{
  "tenantIdentifier": "asya-motor",
  "email": "owner@asyamotor.com",
  "password": "Password123!"
}
```

Expected result:

- `200 OK`
- response contains:
  - `accessToken`
  - `refreshToken`
  - `tenantIdentifier`
  - `email`
  - `role`

## 7. Use Swagger Authorize

Swagger now exposes an `Authorize` button for JWT-protected endpoints.

1. Copy the `accessToken` value from the login response.
2. Click `Authorize`.
3. Enter:

```text
Bearer <accessToken>
```

4. Confirm authorization.

Protected endpoints should now be callable directly from Swagger.

## 8. Smoke-test protected endpoints

Recommended order:

1. `GET /api/auth/me`
2. `GET /api/dashboard/daily`
3. `POST /api/customers`
4. `POST /api/vehicles`
5. `POST /api/service-orders`
6. `GET /api/service-orders`

Example customer:

```json
{
  "fullName": "Ahmet Yilmaz",
  "phone": "05551234567",
  "email": "ahmet@example.com",
  "notes": "VIP customer"
}
```

Example vehicle:

```json
{
  "customerId": "<customerId>",
  "plate": "34ABC123",
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2020,
  "vin": "JTDBR32E720123456"
}
```

Example service order:

```json
{
  "vehicleId": "<vehicleId>",
  "customerId": "<customerId>",
  "vehicleKm": 125000,
  "complaint": "Engine noise at idle"
}
```

Expected service-order behavior:

- `POST /api/service-orders` returns `201 Created`
- the returned service order uses the `SRV-yyyyMMdd-0001` format
- concurrent requests should no longer duplicate `OrderNo`

## 9. Refresh and logout

To verify token lifecycle:

- `POST /api/auth/refresh-token`
- `POST /api/auth/logout`

Use the `refreshToken` returned by login.

## 10. Common failure points

- `Connection string 'DefaultConnection' is not configured`
  - verify `src/MotorCare.Api/appsettings.Development.json` is being loaded
  - or set `ConnectionStrings__DefaultConnection` in the shell before running the API
- `relation "ServiceOrderNumberCounters" does not exist`
  - the database schema is behind the current model; apply/update migrations
- `401 Unauthorized`
  - invalid JWT, expired JWT, inactive tenant, inactive user, or missing Bearer token
- `403 Forbidden`
  - authenticated user does not satisfy the endpoint role policy
