# Frontend API Contract

## Overview

- **API base URL**: configured via `NEXT_PUBLIC_API_BASE_URL` environment variable (trimmed of trailing slash)
- **Auth mechanism**: Bearer token in `Authorization` header, set automatically by `apiClient` interceptor
- **Client identifier**: `X-MotorCare-Client-Key` header sent on all requests (tenant/client scoping)
- **Token refresh**: On 401, the client automatically calls `POST /api/auth/refresh-token` once; on failure it clears tokens and redirects to `/login?from=<original-path>`
- **Rate limiting**: Public auth endpoints are protected by a `PublicAuth` rate limit policy on the backend
- **Timeout**: 15 seconds on all requests
- **Error shape**: Backend returns RFC 7807 `application/problem+json` with fields `code`, `message`, `errors`

All protected endpoints require a valid JWT. Routes marked **No auth** are either public or use the public rate-limited group without `[Authorize]`.

---

## Auth Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| POST | `/api/auth/login` | Login page, `auth.service.login()` | No | Body: `{ tenantIdentifier, email, password }`. Returns `LoginResponse` with `accessToken`, `refreshToken`, `userId`, `tenantId`, `tenantIdentifier`, `email`, `role`, `requiresTwoFactor`, optional `ticket` |
| POST | `/api/auth/register` | Register page, `auth.service.register()` | No | Body: `{ tenantIdentifier, tenantName, ownerFullName, ownerEmail, ownerPassword }`. Returns 201 with tenant info |
| POST | `/api/auth/refresh-token` | `apiClient` 401 interceptor | No | Body: `{ refreshToken }`. Returns new `accessToken`, `refreshToken` |
| POST | `/api/auth/forgot-password` | Forgot password page | No | Body: `{ email, tenantIdentifier }`. Returns `{ message }` |
| POST | `/api/auth/reset-password` | Reset password page | No | Body: `{ tenantIdentifier, email, code, newPassword, confirmPassword }`. Returns `{ message }` |
| POST | `/api/auth/verify-email-code` | Verify email page | No | Body: `{ tenantIdentifier, email, code }`. Also mapped at `/api/auth/verify-email` |
| POST | `/api/auth/resend-email-verification-code` | Verify email page | No | Body: `{ email, tenantIdentifier }`. Also mapped at `/api/auth/resend-email-verification` |
| POST | `/api/auth/two-factor/verify` | Two-factor page, `auth.service.verifyTwoFactor()` | No | Body: `{ ticket, code }`. Returns full `LoginResponse` |
| POST | `/api/auth/two-factor/resend` | Two-factor page, `auth.service.resendTwoFactorCode()` | No | Body: `{ ticket }`. Returns `{ message }` |
| POST | `/api/auth/accept-invite` | Accept invite page | No | Body: `{ token, fullName, password, confirmPassword }`. Returns `{ message }` |
| GET | `/api/auth/me` | `auth.service.loadCurrentUser()` — called on app boot | Yes | Returns `CurrentUser`: `{ userId, tenantId, tenantIdentifier, email, role }` |
| POST | `/api/auth/logout` | Logout action | Yes | Body: `{ refreshToken }`. Returns 204. Tokens cleared in `finally` block regardless of API response |
| GET | `/api/auth/security-status` | Security settings page (not yet wired in frontend) | Yes | Returns `SecurityStatusDto` with 2FA status |
| POST | `/api/auth/2fa/enable/send-code` | Security settings (not yet wired in frontend) | Yes | No body. Sends 2FA enable email |
| POST | `/api/auth/2fa/enable/confirm` | Security settings (not yet wired in frontend) | Yes | Body: `{ code }` |
| POST | `/api/auth/2fa/disable/send-code` | Security settings (not yet wired in frontend) | Yes | No body |
| POST | `/api/auth/2fa/disable/confirm` | Security settings (not yet wired in frontend) | Yes | Body: `{ code }` |

---

## Customer Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/customers` | Customers list page, service-orders new page, inspections new page, vehicles new page | Yes | Query: `q` (search), `pageNumber` (default 1), `pageSize` (default 20). Returns `PagedResult<Customer>` |
| GET | `/api/customers/{id}` | Customer detail page, customer edit page, customer vehicles new page | Yes | Returns `Customer`: `{ id, fullName, phone, email, whatsapp, notes, address, taxNumber, taxOffice, vehicleCount }` |
| GET | `/api/customers/{id}/summary` | (backend available, not yet used by frontend) | Yes | Returns `CustomerSummaryDto` with aggregated stats |
| POST | `/api/customers` | Customer create page (`/customers/create`) | Yes | Body: `{ fullName, phone, email?, whatsapp?, notes? }`. Returns created `Guid` with 201 |
| PUT | `/api/customers/{id}` | Customer edit page | Yes | Body: `{ fullName, phone, email?, whatsapp?, notes? }`. Returns 204 |
| GET | `/api/customers/{customerId}/vehicles` | Customer detail page (vehicle list), service-orders new, inspections new | Yes | Returns `Vehicle[]` for the customer |

**Frontend fallback**: On fetch error, customers list shows an error state with retry button.

---

## Vehicle Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| POST | `/api/vehicles` | Customer detail page (add vehicle inline), customer vehicles new page, vehicles new page | Yes | Body: `{ customerId, plate, brand?, model?, year?, color?, chassisNumber?, engineNumber?, currentKm? }`. Returns `Guid` with 201 |
| GET | `/api/vehicles/{plate}` | Vehicles search page (`/vehicles`) — lookup by plate string | Yes | Returns `VehicleDto`. Also used to navigate to vehicle detail |
| GET | `/api/vehicles/{id}/history` | Vehicle detail page (`/vehicles/[id]`), vehicle edit page | Yes | Returns `VehicleServiceHistoryDto`: `{ vehicleId, plate, brand, model, year, vehicleDisplay, currentKm, totalServiceOrderCount, lastServiceDate, totalSpent, history[] }` |

**Note**: `GET /api/vehicles/{plate}` accepts a raw plate string (not a GUID). The frontend normalizes the plate (strips spaces/hyphens, uppercases) before calling. Route `GET /api/vehicles/{id:guid}/history` uses a GUID.

---

## Appointment Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/appointments` | Appointments list page | Yes | Query: `q`, `status` (AppointmentStatus), `type` (AppointmentType), `startFrom` (ISO), `endTo` (ISO), `pageNumber` (default 1), `pageSize` (default 20). Returns `PagedResult<AppointmentDto>` |
| GET | `/api/appointments/{id}` | Appointment detail page | Yes | Returns `AppointmentDto` |
| POST | `/api/appointments` | New appointment page | Yes (ServiceOrderWrite) | Body: `AppointmentUpsertRequest` — `{ customerId?, vehicleId?, customerName, phone, plate?, type, startAt, endAt, note?, complaint? }`. Returns `AppointmentDto` with 201 |
| PUT | `/api/appointments/{id}` | Appointment edit page | Yes (ServiceOrderWrite) | Same body as POST. Returns updated `AppointmentDto` |
| PUT | `/api/appointments/{id}/status` | Appointment detail page (status change) | Yes (ServiceOrderWrite) | Body: `{ status: AppointmentStatus }`. Returns 204 |
| DELETE | `/api/appointments/{id}` | Appointment detail page (cancel) | Yes (ServiceOrderWrite) | Internally sets status to `Cancelled`. Returns 204 |
| POST | `/api/appointments/{id}/convert-to-service-order` | Appointment detail page (convert action) | Yes (ServiceOrderWrite) | Body: `{ vehicleKm: number }`. Returns `{ serviceOrderId: string }` |

**AppointmentStatus values**: `Scheduled`, `Confirmed`, `CheckedIn`, `ConvertedToOrder`, `Cancelled`, `NoShow`, `Completed`

**AppointmentType values**: `Maintenance`, `Repair`, `Cleaning`, `Washing`, `Inspection`, `TireChange`, `Other`

**Frontend fallback**: List shows empty state on error. Convert action navigates to the created service order on success.

---

## Service Order Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/service-orders` | Service orders list page | Yes (ServiceOrderRead) | Query: `customerId?`, `status?` (ServiceOrderStatus), `q?`, `openedFrom?`, `openedTo?`, `pageNumber` (default 1), `pageSize` (default 20). Returns `PagedResult<ServiceOrderSummaryDto>` |
| POST | `/api/service-orders` | New service order page | Yes (ServiceOrderWrite) | Body: `{ vehicleId, vehicleKm, complaint?, customerId? }`. Returns created `Guid` with 201. Frontend navigates to the created order |
| GET | `/api/service-orders/{id}` | Service order detail page, print page | Yes (ServiceOrderRead) | Returns `ServiceOrderDto` with full line items, payments, totals |
| GET | `/api/service-orders/{id}/status-history` | (backend available, not yet wired in frontend) | Yes (ServiceOrderRead) | Returns `ServiceOrderStatusHistoryDto[]` |
| GET | `/api/service-orders/{id}/activity-feed` | (backend available, not yet wired in frontend) | Yes (ServiceOrderRead) | Returns `ServiceOrderActivityFeedItem[]` |
| PUT | `/api/service-orders/{id}/status` | Service order detail page | Yes (ServiceOrderWrite) | Body: `{ status: ServiceOrderStatus, note?: string }`. Returns 204 |
| PATCH | `/api/service-orders/{id}/discount` | Service order detail page | Yes (ServiceOrderPayments) | Body: `{ discount: number }`. Returns 204 |
| POST | `/api/service-orders/{id}/operations` | Service order detail page (add operation) | Yes (ServiceOrderWrite) | Body: `{ description, quantity?, unitPrice?, discount, notes?, serviceCatalogItemId? }`. Returns 204 |
| DELETE | `/api/service-orders/{id}/operations/{operationId}` | Service order detail page | Yes (ServiceOrderWrite) | Returns 204 |
| POST | `/api/service-orders/{id}/parts` | Service order detail page (add part) | Yes (ServiceOrderWrite) | Body: `{ partName, partNumber?, unitPrice, quantity, inventoryItemId?, discount, notes? }`. Returns 204 |
| DELETE | `/api/service-orders/{id}/parts/{partId}` | Service order detail page | Yes (ServiceOrderWrite) | Returns 204 |
| POST | `/api/service-orders/{id}/consumables` | Service order detail page (add consumable) | Yes (ServiceOrderWrite) | Body: `{ category, productName, unitPrice, quantity, brand?, subCategory?, specification?, notes? }`. Returns 204 |
| DELETE | `/api/service-orders/{id}/consumables/{consumableId}` | Service order detail page | Yes (ServiceOrderWrite) | Returns 204 |
| POST | `/api/service-orders/{id}/payments` | Service order detail page (add payment) | Yes (ServiceOrderPayments) | Body: `{ amount, method: PaymentMethod, paymentDate?: ISO string }`. Returns 204 |
| GET | `/api/service-orders/{id}/attachments` | (backend available, not yet wired in frontend) | Yes (ServiceOrderRead) | Returns `ServiceOrderAttachmentDto[]` |
| POST | `/api/service-orders/{id}/attachments` | (backend available, not yet wired in frontend) | Yes (ServiceOrderWrite) | `multipart/form-data`: `File` (max 5 MB), `AttachmentType`, `Description?`. Returns 201 with attachment metadata |
| GET | `/api/service-orders/{id}/attachments/{attachmentId}/download` | (backend available, not yet wired in frontend) | Yes (ServiceOrderRead) | Streams file. Query `download=true` for Content-Disposition attachment |
| DELETE | `/api/service-orders/{id}/attachments/{attachmentId}` | (backend available, not yet wired in frontend) | Yes (ServiceOrderWrite) | Returns 204 |
| POST | `/api/service-orders/{id}/public-access` | (backend available, not yet wired in frontend) | Yes (ServiceOrderRead) | Get-or-create a public share link. Returns `PublicRecordAccessDto` with slug |
| GET | `/api/service-orders/{id}/public-access` | (backend available, not yet wired in frontend) | Yes (ServiceOrderRead) | Returns existing `PublicRecordAccessDto` or 404 |
| PUT | `/api/service-orders/{id}/public-access/enable` | (backend available, not yet wired in frontend) | Yes (ServiceOrderWrite) | Enables public link sharing. Returns `PublicRecordAccessDto` |
| PUT | `/api/service-orders/{id}/public-access/disable` | (backend available, not yet wired in frontend) | Yes (ServiceOrderWrite) | Disables public link sharing. Returns 204 |
| GET | `/api/service-orders/consumable-catalog/search` | (backend available, not yet wired in frontend) | Yes (ServiceOrderRead) | Query: `query?`, `category?`, `maxResults` (default 20). Returns `ConsumableCatalogItemDto[]` |
| POST | `/api/service-orders/consumable-catalog/track` | (backend available, not yet wired in frontend) | Yes (ServiceOrderWrite) | Body: `{ items: ConsumableCatalogItemInput[] }`. Returns 204. Used to persist frequently used consumables |

**Frontend fallback**: Service order detail page shows full page error state with retry on load failure. Add/remove mutations show toast errors inline. Print page renders a print-friendly layout using the same detail endpoint.

---

## Inspection Endpoints (`/api/inspections`)

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/inspections` | Inspections list page | Yes (InspectionRead) | Query: `q?`, `packageType?`, `status?`, `customerId?`, `vehicleId?`, `createdFrom?`, `createdTo?`, `pageNumber` (default 1), `pageSize` (default 20). Returns `PagedResult<MotorcycleInspectionListItemDto>` |
| GET | `/api/inspections/{id}` | Inspection detail page, print page | Yes (InspectionRead) | Returns `MotorcycleInspectionDto` with full items list |
| POST | `/api/inspections` | New inspection page | Yes (InspectionWrite) | Body: `{ customerId?, vehicleId?, customerName, phone, plate, brand?, model?, year?, mileage?, chassisNumber?, engineNumber?, query5664?, mileageQuery?, packageType, generalNotes?, testRideNotes?, cosmeticNotes? }`. Returns 201 with `{ id, inspectionNo }` |
| PUT | `/api/inspections/{id}` | (backend available, edit not yet built in frontend) | Yes (InspectionWrite) | Same body as POST. Returns 204 |
| PUT | `/api/inspections/{id}/items/{itemId}` | Inspection detail page (checklist item update) | Yes (InspectionWrite) | Body: `{ result: MotorcycleInspectionResult, notes? }`. Returns 204 |
| PUT | `/api/inspections/{id}/complete` | Inspection detail page (complete action) | Yes (InspectionWrite) | No body. Returns 204 |
| PUT | `/api/inspections/{id}/cancel` | Inspection detail page (cancel action) | Yes (InspectionWrite) | No body. Returns 204 |
| POST | `/api/inspections/{id}/public-access` | Inspection detail page (share link, get-or-create) | Yes (InspectionWrite) | Returns `PublicRecordAccessDto` with `publicIdentifier`/slug |
| GET | `/api/inspections/{id}/public-access` | (backend available, not yet explicitly called in frontend) | Yes (InspectionRead) | Returns existing `PublicRecordAccessDto` |
| PUT | `/api/inspections/{id}/public-access/enable` | (backend available, not yet wired in frontend) | Yes (InspectionWrite) | Enables sharing. Returns `PublicRecordAccessDto` |
| PUT | `/api/inspections/{id}/public-access/disable` | (backend available, not yet wired in frontend) | Yes (InspectionWrite) | Disables sharing. Returns 204 |

**MotorcycleInspectionPackageType values**: defined in `Domain.Enums` (e.g. `Basic`, `Standard`, `Full` — confirm with backend enum)

**Frontend note**: The inspection detail page computes the public share URL with `publicInspectionReportUrl(publicIdentifier)` and copies it to clipboard.

---

## Inventory Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/inventory` | Inventory list page, service order detail (parts picker) | Yes (InventoryRead) | Query: `q?`, `category?`, `isActive?`, `lowStockOnly?` (bool), `pageNumber` (default 1), `pageSize` (default 20). Returns `PagedResult<InventoryItemDto>` |
| GET | `/api/inventory/{id}` | Inventory item detail/edit page | Yes (InventoryRead) | Returns `InventoryItemDto` |
| POST | `/api/inventory` | Inventory create page (`/inventory/create`) | Yes (InventoryWrite) | Body: `{ name, sku?, barcode?, category?, brand?, unit, unitPrice, stockQuantity, minimumStockLevel, isActive }`. Returns created `Guid` with 201 |
| PUT | `/api/inventory/{id}` | Inventory item edit page | Yes (InventoryWrite) | Same body as POST. Returns 204 |
| PUT | `/api/inventory/{id}/activate` | Inventory item detail page (status toggle) | Yes (InventoryWrite) | No body. Returns 204 |
| PUT | `/api/inventory/{id}/deactivate` | Inventory item detail page (status toggle) | Yes (InventoryWrite) | No body. Returns 204 |
| POST | `/api/inventory/{id}/adjust-stock` | Inventory item detail page (stock adjustment) | Yes (InventoryWrite) | Body: `{ quantityDelta: number, reason: string }`. Returns 204 |

---

## Service Catalog Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/services` | Service catalog list page, service order detail (operations picker) | Yes (CustomerRead) | Query: `q?`, `category?` (ServiceCategory), `isActive?`, `pageNumber` (default 1), `pageSize` (default 20). Returns `PagedResult<ServiceCatalogItemDto>` |
| GET | `/api/services/{id}` | Service catalog item detail/edit page | Yes (CustomerRead) | Returns `ServiceCatalogItemDto`: `{ id, name, category, description?, defaultDurationMinutes, price, currency, isActive }` |
| POST | `/api/services` | Service catalog create page (`/service-catalog/create`) | Yes (CustomerOperations) | Body: `{ name, category, description?, defaultDurationMinutes, price/defaultPrice, currency, isActive }`. Returns `Guid` with 201 |
| PUT | `/api/services/{id}` | Service catalog item edit page | Yes (CustomerOperations) | Same body as POST. Returns 204 |
| PUT | `/api/services/{id}/activate` | Service catalog item detail page (status toggle) | Yes (CustomerOperations) | No body. Returns 204 |
| PUT | `/api/services/{id}/deactivate` | Service catalog item detail page (status toggle) | Yes (CustomerOperations) | No body. Returns 204 |

**Note**: The create/update request accepts both `price` and `defaultPrice` fields; `EffectivePrice` uses `price` if non-zero, else `defaultPrice`.

---

## Dashboard Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/dashboard/daily` | Dashboard page | Yes (DashboardRead) | No query params. Returns `DailySummaryDto`: `{ totalRevenue?, activeServiceOrders?, completedToday?, totalCustomers?, todayAppointments?, pendingPayment?, recentServiceOrders[], todayAppointmentsList[], criticalInspections[] }`. `staleTime` = 30 s |
| GET | `/api/dashboard/monthly` | Dashboard page (monthly revenue chart) | Yes (DashboardRead) | Returns `List<MonthlyRevenueStat>`: `[{ month, revenue, orderCount }]` |
| GET | `/api/dashboard/payment-summary` | (backend available, not yet wired in frontend) | Yes (DashboardRead) | Query: `from` (ISO), `to` (ISO). Returns `PaymentSummaryDto` |
| GET | `/api/dashboard/open-balances` | (backend available, not yet wired in frontend) | Yes (DashboardRead) | Query: `take` (default 50). Returns `OpenBalanceDto[]` |

**Frontend note**: The monthly revenue chart calls `GET /api/dashboard/monthly` through React Query and renders returned `{ month, revenue, orderCount }` rows. Empty/error responses are scoped to the chart and do not block the rest of the dashboard. Error copy: `Aylık grafik bilgileri şu anda yüklenemedi.`

**Frontend fallback**: On daily summary error, the page renders an `<ErrorState>` component with a retry button.

---

## Settings / Users Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/users` | Settings users page | Yes (UserManagement) | Returns `UserDto[]`: `{ id, fullName, email, role, isActive }` |
| POST | `/api/users/invite` | Settings users page (invite modal) | Yes (UserManagement) | Body: `{ email, role: UserRole, fullName? }`. Returns 204. Sends invitation email |
| PUT | `/api/users/{id}/role` | Settings users page (role change) | Yes (UserManagement) | Body: `{ role: UserRole }`. Returns 204 |
| PATCH | `/api/users/{id}/deactivate` | Settings users page (deactivate) | Yes (UserManagement) | No body. Returns 204 |
| GET | `/api/users/invitations/{token}/validate` | Accept invite page, `auth.service.validateInvite()` | No | Returns `{ email, fullName?, role, isValid }` |
| GET | `/api/tenants/current/profile` | Settings business page | Yes | Returns `TenantProfileDto`: `{ tenantName, ownerFullName, ownerEmail, phone?, address?, taxNumber?, taxOffice?, logoUrl?, currency }` |
| PUT | `/api/tenants/current/profile` | Settings business page | Yes (TenantManagement) | Body: same fields as profile response. Returns updated `TenantProfileDto` |

**UserRole values** (from backend): `Owner`, `Technician`, `Inspector`, `Accountant`, `ReadOnly`

---

## Public Records Endpoints (Unauthenticated)

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/public/service-record/{slug}` | `/public/service-record/[slug]` page | No (anonymous) | Returns `PublicServiceRecordDto` with sanitized service order info for customer-facing sharing |
| GET | `/api/public/inspection-report/{slug}` | `/public/inspection-report/[slug]` page | No (anonymous) | Returns `PublicInspectionReportDto` with sanitized inspection data for customer-facing sharing |

**Note**: These are entirely public-facing pages. The slug is either the `publicIdentifier` from `PublicRecordAccessDto` or falls back to the record's own GUID.

---

## Vehicle Catalog Endpoints (Reference Data)

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/vehicle-catalog/motorcycles/brands` | New vehicle / new inspection forms (brand autocomplete) | Yes (CustomerOperations) | Query: `search?`, `maxResults` (default 20). Returns `string[]` |
| GET | `/api/vehicle-catalog/motorcycles/models` | New vehicle / new inspection forms (model autocomplete) | Yes (CustomerOperations) | Query: `brand` (required), `search?`, `maxResults` (default 20). Returns `MotorcycleCatalogSuggestionDto[]` |
| GET | `/api/vehicle-catalog/motorcycles/search` | (backend available, not yet wired in frontend) | Yes (CustomerOperations) | Query: `query?`, `maxResults` (default 20). Returns `MotorcycleCatalogSuggestionDto[]` |

---

## Import Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/imports` | (backend available, import UI not yet built) | Yes (ImportOperations) | Returns `ImportBatchDto[]` — list of all past imports for tenant |
| GET | `/api/imports/{batchId}` | (backend available, import UI not yet built) | Yes (ImportOperations) | Query: `previewRows` (default 50). Returns `ImportBatchDto` with row preview |
| GET | `/api/imports/{batchId}/rows` | (backend available, import UI not yet built) | Yes (ImportOperations) | Query: `status?`, `maxRows` (default 200). Returns `ImportBatchRowDto[]` |
| POST | `/api/imports/upload` | (backend available, import UI not yet built) | Yes (ImportOperations) | `multipart/form-data`: `file` + query param `importType` (`Customers`, `Vehicles`, `ServiceHistory`). Returns `ImportBatchDto` |
| POST | `/api/imports/{batchId}/commit` | (backend available, import UI not yet built) | Yes (ImportOperations) | No body. Commits parsed batch to database. Returns updated `ImportBatchDto` |
| GET | `/api/imports/templates/{type}` | (backend available, import UI not yet built) | Yes (ImportOperations) | Returns CSV template file download. `type` = `Customers`, `Vehicles`, or `ServiceHistory` |

---

## Onboarding Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| POST | `/api/onboarding/tenant` | Alternative registration flow (mirrors `/api/auth/register`) | No | Body: same as register. Returns 201 with tenant info |

---

## Infrastructure Endpoints

| Method | Route | Frontend Usage | Auth | Notes |
|--------|-------|----------------|------|-------|
| GET | `/api/health` | (not called by frontend) | No | Health check endpoint |
| GET | `/api/version` | (not called by frontend) | No | Returns API version info |

---

## Missing / Pending Endpoints

| Feature | Needed Endpoint | Current Backend Status | Frontend Fallback | Priority |
|---------|----------------|------------------------|-------------------|----------|
| Dashboard | `GET /api/dashboard/payment-summary` | Implemented | Not wired | Medium |
| Dashboard | `GET /api/dashboard/open-balances` | Implemented | Not wired | Medium |
| Service Orders | `GET /api/service-orders/{id}/status-history` | Implemented | Timeline not yet rendered | Low |
| Service Orders | `GET /api/service-orders/{id}/activity-feed` | Implemented | Activity feed not rendered | Low |
| Service Orders | Attachment CRUD (`/api/service-orders/{id}/attachments`) | Implemented | No attachment UI | Medium |
| Service Orders | Public access management (`/api/service-orders/{id}/public-access`) | Implemented | Share button renders only when `publicSlug` is available; enable/disable toggle not built | Medium |
| Inspections | Public access management (`/api/inspections/{id}/public-access`) | Implemented | Share button calls `POST /api/inspections/{id}/public-access` correctly; enable/disable toggle not built | Medium |
| Auth / Security | `GET /api/auth/security-status`, 2FA enable/disable flow | Implemented | Settings security tab shows placeholder | Medium |
| Imports | Full import flow (`/api/imports/*`) | Implemented | No import UI exists | Low |
| Vehicle Catalog | `GET /api/vehicle-catalog/motorcycles/search` | Implemented | Not wired (brand/model lookup uses separate brand+model endpoints) | Low |
| Customers | `GET /api/customers/{id}/summary` | Implemented | Customer detail page fetches raw customer + vehicles separately | Low |
