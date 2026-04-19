# MotorCare Smoke Test Runbook

This runbook verifies the core MVP backend flow before Sprint 2 work.

## Preconditions

1. Start PostgreSQL:

```powershell
docker compose up -d
```

2. Apply schema updates:

```powershell
dotnet ef database update --project src/MotorCare.Infrastructure/MotorCare.Infrastructure.csproj --startup-project src/MotorCare.Api/MotorCare.Api.csproj
```

3. Start the API:

```powershell
dotnet run --project src/MotorCare.Api/MotorCare.Api.csproj
```

The local API runs on `https://localhost:7278` and `http://localhost:5087`.

## Core Smoke Flow

### 1. Register tenant + owner

`POST /api/auth/register`

```json
{
  "tenantIdentifier": "stabil-demo",
  "tenantName": "Stabil Demo Servis",
  "ownerFullName": "Smoke Owner",
  "ownerEmail": "owner@stabil.local",
  "ownerPassword": "Password123!"
}
```

### 2. Login

`POST /api/auth/login`

```json
{
  "tenantIdentifier": "stabil-demo",
  "email": "owner@stabil.local",
  "password": "Password123!"
}
```

Use the returned `accessToken` as `Authorization: Bearer <token>`.

### 3. Get current user

`GET /api/auth/me`

Expected:
- correct tenant identifier
- `Owner` role

### 4. Create customer

`POST /api/customers`

```json
{
  "fullName": "Smoke Customer",
  "phone": "05551230000",
  "email": "customer@stabil.local",
  "whatsapp": "05551230000",
  "notes": "smoke customer"
}
```

### 5. List customers

`GET /api/customers`

Expected:
- created customer appears in the list

### 6. Create vehicle

`POST /api/vehicles`

```json
{
  "plate": "34SMOKE001",
  "brand": "Ford",
  "model": "Transit",
  "year": 2020,
  "chassisNumber": "CHASSIS123",
  "color": "White",
  "currentKm": 120000,
  "currentCustomerId": "<customerId>"
}
```

### 7. Lookup vehicle by plate

`GET /api/vehicles/34SMOKE001`

Expected:
- normalized plate is returned

### 8. Create appointment

`POST /api/appointments`

```json
{
  "customerId": "<customerId>",
  "vehicleId": "<vehicleId>",
  "customerName": "Smoke Customer",
  "phone": "05551230000",
  "plate": "34SMOKE001",
  "type": 0,
  "startAt": "2026-04-20T09:00:00Z",
  "endAt": "2026-04-20T10:00:00Z",
  "note": "smoke appointment",
  "complaint": "Periodic maintenance"
}
```

`type: 0` maps to `Maintenance`.

### 9. List appointments

`GET /api/appointments`

Expected:
- created appointment appears

### 10. Convert appointment to service order

`POST /api/appointments/{id}/convert-to-service-order`

```json
{
  "vehicleKm": 120500
}
```

Expected:
- response contains `serviceOrderId`

### 11. List service orders

`GET /api/service-orders`

Expected:
- converted order appears

### 12. Get service order detail

`GET /api/service-orders/{id}`

Expected:
- `status` is `Open`
- complaint is copied from appointment
- `vehicleId` and `customerId` match the source records

## Known Local Warnings

- `NU1608` warnings in `MotorCare.Api` are currently tolerated
- DataProtection keys are stored locally under `src/MotorCare.Api/App_Data/DataProtectionKeys` in Development
- split query is enabled for multi-collection service order and vehicle reads to avoid EF warning noise
