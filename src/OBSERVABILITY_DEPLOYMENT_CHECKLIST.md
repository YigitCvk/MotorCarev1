# Elastic / Kibana Deployment Readiness Checklist

This checklist verifies that MotorCare / BakimSuite logging is ready for staging deployment and Kibana inspection.

## 1. Configuration

Confirm the following settings are present in staging secrets or environment variables:

- `Elastic__Uri`
- `Elastic__Username`
- `Elastic__Password`
- `Elastic__IndexFormat`
- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`

Recommended index format:

- `motorcare-api-{0:yyyy.MM}`

Expected application-level fields:

- `ApplicationName = MotorCare.Api`
- `Environment`
- `SourceContext`
- `EventId`
- `EventName`
- `CorrelationId`
- `RequestId`
- `TenantId`
- `UserId`

## 2. Middleware Verification

The request pipeline should preserve structured request context in this order:

1. `UseExceptionHandler()`
2. `CorrelationIdMiddleware`
3. `UseAuthentication()`
4. `UserContextLoggingMiddleware`
5. `UseSerilogRequestLogging()`
6. `ActiveTenantUserGuardMiddleware`
7. `UseAuthorization()`

Why this order matters:

- correlation properties must exist for the whole request
- user and tenant properties must be pushed before request logging writes the completion event
- exception handling must wrap the pipeline

## 3. Staging Verification Steps

After deployment, confirm:

1. The API starts without Serilog bootstrap failure.
2. Elasticsearch is reachable from the API host.
3. A new index matching the configured pattern is created.
4. Request logs arrive for authenticated and anonymous endpoints.
5. `EventName` is present beside `EventId`.
6. `CorrelationId` is present on request and handler logs.
7. `TenantId` and `UserId` appear for authenticated flows.
8. Errors are indexed with exception details.
9. Warnings are visible for blocked business rules.
10. Kibana filters can isolate a single request chain.

## 4. Sample Actions To Test

Run these 10 actions after staging deployment:

1. `POST /api/auth/login`
2. `POST /api/customers`
3. `PUT /api/customers/{id}`
4. `POST /api/vehicles`
5. `GET /api/vehicles/{plate}`
6. `POST /api/appointments`
7. `PUT /api/appointments/{id}/status`
8. `POST /api/appointments/{id}/convert-to-service-order`
9. `POST /api/service-orders`
10. `POST /api/inspections`

Optional extended checks:

- add service order operation
- add service order part
- add payment
- update inspection item
- activate/deactivate inventory item
- create/update service catalog item
- load dashboard daily summary

## 5. Kibana Starter Guide

### Filter by tenant

Use:

- `TenantId : "<tenant-guid-or-identifier>"`

If tenant id is a keyword field in your index template, prefer exact match.

### Filter by correlation id

Use:

- `CorrelationId : "<x-correlation-id>"`

This is the fastest way to trace one request end-to-end across middleware, handlers and exception logs.

### Filter by event id or event name

Use:

- `EventId.Id : 1500`
- `EventName : "ServiceOrderCreated"`

Recommended searches:

- `EventName : "AppointmentCreated"`
- `EventName : "AppointmentConvertedToServiceOrder"`
- `EventName : "InspectionCreated"`
- `EventName : "InspectionItemUpdated"`
- `EventName : "BusinessRuleBlocked"`

### Find errors and warnings

Use:

- `Level : "Error"`
- `Level : "Warning"`

Pair with:

- `TenantId`
- `CorrelationId`
- `SourceContext`

### Verify core flows

Appointment flow:

- `AppointmentCreated`
- `AppointmentStatusUpdated`
- `AppointmentConvertedToServiceOrder`

Service order flow:

- `ServiceOrderCreated`
- `OperationAdded`
- `PartAdded`
- `PaymentAdded`
- `ServiceOrderStatusUpdated`
- `BusinessRuleBlocked`

Inspection flow:

- `InspectionCreated`
- `InspectionFetched`
- `InspectionUpdated`
- `InspectionItemUpdated`
- `InspectionCompleted`
- `InspectionCancelled`

Catalog and inventory flow:

- `ServiceCatalogItemCreated`
- `ServiceCatalogItemUpdated`
- `ServiceCatalogItemActivated`
- `ServiceCatalogItemDeactivated`
- `InventoryItemCreated`
- `InventoryItemUpdated`
- `InventoryItemActivated`
- `InventoryItemDeactivated`
- `InventoryStockAdjusted`

## 6. Expected Structured Fields

Important properties you should see in Kibana:

- `ApplicationName`
- `Environment`
- `SourceContext`
- `EventId.Id`
- `EventId.Name`
- `EventName`
- `CorrelationId`
- `RequestId`
- `TenantId`
- `TenantIdentifier`
- `UserId`
- `UserRole`
- domain-specific ids such as:
  - `AppointmentId`
  - `ServiceOrderId`
  - `InspectionId`
  - `CustomerId`
  - `VehicleId`

## 7. Deployment Sign-off

Staging is observability-ready when all of the following are true:

- logs reach Elasticsearch consistently
- request logs include correlation and user context
- critical create/update/status/business-rule events appear in Kibana
- at least one end-to-end request can be traced by `CorrelationId`
- errors and warnings are discoverable without digging through console output
