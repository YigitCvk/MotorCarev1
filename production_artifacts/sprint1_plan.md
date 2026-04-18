# Sprint 1 Plan — MotorCare MVP (Yön A: Servis Ustasının Dijital Defteri)

**Sprint:** 1  
**Duration:** 2 hafta (2026-04-18 → 2026-05-02)  
**Sprint Hedefi:** Yön A MVP backend API'sini çalışır hale getirmek — servis kaydından ödeme kanalına kadar tüm iş akışları API üzerinden uçtan uca test edilebilir olacak.

---

## Güncel Durum (2026-04-18 itibarıyla tamamlananlar)

Bu sprint başlamadan önce tamamlanan teknik borç düzeltmeleri:

| Tamamlanan | Açıklama |
|---|---|
| ✅ Carter geçişi | `VehiclesController` → `VehiclesModule`, `AddCarter()` + `MapCarter()` |
| ✅ DI kayıt eksiklikleri | Tüm repository ve service'ler DI'a eklendi |
| ✅ GlobalExceptionHandler | `Program.cs` pipeline'ına bağlandı |
| ✅ AuditableEntity | `public set` → `internal set`, `InternalsVisibleTo` eklendi |
| ✅ Redundant UpdateDetails | `SetChassisAndColor()` domain metodu eklendi |
| ✅ sealed record | Tüm Command/Query/DTO'lara `sealed` eklendi |
| ✅ ValidationBehavior | `AppValidationException` throw edecek şekilde güncellendi |
| ✅ Hardcoded conn string | Exception fırlatacak şekilde değiştirildi |

### Güncel Mimari Durum

```
MotorCare.Domain        ████████████████████ 92%  — Aggregates, VO, Repository interfaces
MotorCare.Application   ████████░░░░░░░░░░░░ 40%  — Customer(2) + Vehicle(2) handler; eksik: ServiceOrder, Tenant, Dashboard
MotorCare.Infrastructure████████████████████ 95%  — DbContext, 4 repo impl, migration, services; tümü DI'a kayıtlı
MotorCare.Api           ████████████░░░░░░░░ 55%  — Carter kurulu, GlobalExceptionHandler aktif, VehiclesModule hazır
Tests                   ██░░░░░░░░░░░░░░░░░░  8%  — PlateNumber VO testleri
```

---

## User Story → Sprint Task Eşleşmesi

| Story | Sprint 1'de Yapılacak | Öncelik |
|---|---|---|
| US-01 Servis Kaydı | `CreateTenantCommand` + `TenantsModule` | Must |
| US-03 Müşteri Kaydı | `CustomersModule` Carter endpoint (handler hazır) | Must |
| US-04 Müşteri Arama | `SearchCustomersQuery` + handler + endpoint | Must |
| US-05 Müşteri Detayı | `GetCustomerByIdQuery` + handler + endpoint | Must |
| US-06 Araç Kaydı | ✅ Hazır — `POST /api/vehicles` çalışıyor | — |
| US-07 Plakaya Araç Arama | ✅ Hazır — `GET /api/vehicles/{plate}` çalışıyor | — |
| US-08 İş Emri Açma | `CreateServiceOrderCommand` + handler + endpoint | Must |
| US-09 Durum Geçişleri | `UpdateServiceOrderStatusCommand` + handler + endpoint | Must |
| US-10 İş Emri Detayı | `GetServiceOrderByIdQuery` + handler + endpoint | Must |
| US-11 İşçilik Ekleme | `AddOperationToOrderCommand` + handler + endpoint | Must |
| US-12 Parça Ekleme | `AddPartToOrderCommand` + handler + endpoint | Must |
| US-14 Ödeme Kaydı | `AddPaymentToOrderCommand` + handler + endpoint | Must |
| US-13 İndirim | `SetOrderDiscountCommand` + handler + endpoint | Should |
| US-15 Günlük Özet | `GetDailySummaryQuery` + handler + endpoint | Should |
| US-02 Pasifleştirme | `DeactivateTenantCommand` + endpoint | Should |

---

## MoSCoW Önceliklendirmesi

### Must Have

#### M1 — Tenant Onboarding API (US-01)
- `CreateTenantCommand` + Handler + Validator
- `TenantsModule`: `POST /api/tenants`, `GET /api/tenants/{identifier}`
- Uygulama başlarken seed tenant veya ilk kayıt akışı

#### M2 — Customer Query Handlers (US-04, US-05)
Handler zaten var (Create+Update). Eksik olanlar:
- `GetCustomerByIdQuery` + Handler — `GET /api/customers/{id}`
- `SearchCustomersQuery` + Handler — `GET /api/customers/search?q=...`
- `CustomersModule` Carter modülü (Create + Update + GetById + Search endpoint'lerini barındırır)

#### M3 — Service Order Core Flow (US-08, US-09, US-10)
- `CreateServiceOrderCommand` + Handler + Validator
  - `IOrderNumberGenerator` kullanımı
  - Vehicle + Customer varlık doğrulaması
- `GetServiceOrderByIdQuery` + Handler (Operations, Parts, Payments dahil)
- `GetServiceOrdersQuery` + Handler (tenant bazlı liste, durum filtresi)
- `UpdateServiceOrderStatusCommand` + Handler (Open/InProgress/WaitingForParts/Completed/Delivered/Cancelled)
- `ServiceOrdersModule` Carter modülü

#### M4 — Line Items & Payment (US-11, US-12, US-14)
- `AddOperationToOrderCommand` + Handler + Validator
- `RemoveOperationFromOrderCommand` + Handler
- `AddPartToOrderCommand` + Handler + Validator
- `RemovePartFromOrderCommand` + Handler
- `AddPaymentToOrderCommand` + Handler + Validator (Cash/CreditCard/BankTransfer)
- Yukarıdakilerin endpoint'leri `ServiceOrdersModule`'e eklenir

#### M5 — Database + Smoke Test
- `appsettings.Development.json` PostgreSQL connection string
- `docker-compose.yml` (dev ortam)
- `dotnet ef database update` başarılı
- Swagger UI üzerinden her endpoint smoke test

---

### Should Have

#### S1 — Daily Dashboard (US-15)
- `GetDailySummaryQuery` + Handler
  - Bugün açılan iş emri sayısı
  - Bugün tamamlanan sayısı
  - Devam eden sayısı
  - Bugün tahsil edilen toplam tutar
  - Servisteki araç sayısı (Open + InProgress + WaitingForParts)
- `GET /api/dashboard/daily` endpoint'i

#### S2 — İndirim (US-13)
- `SetOrderDiscountCommand` + Handler + Validator
- `PATCH /api/service-orders/{id}/discount` endpoint'i

#### S3 — Tenant Pasifleştirme (US-02)
- `DeactivateTenantCommand` + Handler
- `PATCH /api/tenants/{identifier}/deactivate` endpoint'i
- Pasif tenant middleware kontrolü

#### S4 — Domain Aggregate Testleri
| Test Sınıfı | Kapsam |
|---|---|
| `ServiceOrderAggregateTests` | State machine geçişleri, geçersiz geçişler, ödeme validasyonu, toplam hesaplama |
| `CustomerAggregateTests` | Invariant enforcement, UpdateContactInfo |
| `VehicleAggregateTests` | Mileage validation, customer assignment |

#### S5 — Health Check
- `GET /health` → `{ status, timestamp, database }`
- PostgreSQL bağlantı doğrulaması

---

### Could Have

#### C1 — Vehicle List Query
- `GetVehiclesByTenantQuery` + Handler — tenant bazlı araç listesi
- `GET /api/vehicles` endpoint'i

#### C2 — Application Handler Unit Testleri
- `CreateServiceOrderHandlerTests`
- `CreateCustomerHandlerTests`
- `CreateVehicleHandlerTests`

#### C3 — Swagger Geliştirme
- `X-Tenant-Id` header tüm Swagger endpoint'lerinde görünsün
- XML doc comment'leri endpoint'lere eklenir

---

### Won't Have (Bu Sprint)

| Konu | Neden Ertelendi |
|---|---|
| JWT / Authentication | Sprint 2 |
| MAUI Frontend | Sprint 2 |
| WhatsApp Bildirimi | Sprint 3 |
| Fatura / Fiş Yazdırma | Sprint 3 |
| Teknisyen Yönetimi | Sprint 3 |
| Randevu Sistemi | Sprint 3+ |
| CI/CD Pipeline | Sprint 2 |

---

## Detaylı Task Listesi

### Backend Tasks

| # | Task | Story | Dosya(lar) | Süre | Bağımlılık |
|---|---|---|---|---|---|
| **BE-01** | CreateTenantCommand + Handler + Validator | US-01 | `Application/Tenants/Commands/CreateTenant/` | 2s | — |
| **BE-02** | TenantsModule Carter (POST, GET identifier) | US-01 | `Api/Modules/TenantsModule.cs` | 1s | BE-01 |
| **BE-03** | GetCustomerByIdQuery + Handler | US-05 | `Application/Customers/Queries/GetCustomerById/` | 1s | — |
| **BE-04** | SearchCustomersQuery + Handler + Validator | US-04 | `Application/Customers/Queries/SearchCustomers/` | 2s | — |
| **BE-05** | CustomersModule Carter (POST, PUT, GET, Search) | US-03..05 | `Api/Modules/CustomersModule.cs` | 2s | BE-03, BE-04 |
| **BE-06** | CreateServiceOrderCommand + Handler + Validator | US-08 | `Application/ServiceOrders/Commands/CreateServiceOrder/` | 3s | — |
| **BE-07** | GetServiceOrderByIdQuery + Handler | US-10 | `Application/ServiceOrders/Queries/GetServiceOrderById/` | 2s | — |
| **BE-08** | GetServiceOrdersQuery + Handler (liste + filtre) | US-10 | `Application/ServiceOrders/Queries/GetServiceOrders/` | 2s | — |
| **BE-09** | UpdateServiceOrderStatusCommand + Handler + Validator | US-09 | `Application/ServiceOrders/Commands/UpdateServiceOrderStatus/` | 2s | — |
| **BE-10** | AddOperationToOrderCommand + Handler + Validator | US-11 | `Application/ServiceOrders/Commands/AddOperationToOrder/` | 2s | — |
| **BE-11** | RemoveOperationFromOrderCommand + Handler | US-11 | `Application/ServiceOrders/Commands/RemoveOperationFromOrder/` | 1s | — |
| **BE-12** | AddPartToOrderCommand + Handler + Validator | US-12 | `Application/ServiceOrders/Commands/AddPartToOrder/` | 2s | — |
| **BE-13** | RemovePartFromOrderCommand + Handler | US-12 | `Application/ServiceOrders/Commands/RemovePartFromOrder/` | 1s | — |
| **BE-14** | AddPaymentToOrderCommand + Handler + Validator | US-14 | `Application/ServiceOrders/Commands/AddPaymentToOrder/` | 2s | — |
| **BE-15** | ServiceOrdersModule Carter (tüm endpoint'ler) | US-08..14 | `Api/Modules/ServiceOrdersModule.cs` | 3s | BE-06..BE-14 |
| **BE-16** | GetDailySummaryQuery + Handler | US-15 | `Application/Dashboard/Queries/GetDailySummary/` | 3s | — |
| **BE-17** | DashboardModule Carter | US-15 | `Api/Modules/DashboardModule.cs` | 1s | BE-16 |
| **BE-18** | SetOrderDiscountCommand + Handler | US-13 | `Application/ServiceOrders/Commands/SetOrderDiscount/` | 1s | — |
| **BE-19** | appsettings.Development.json + docker-compose.yml | M5 | Proje kökü | 1s | — |
| **BE-20** | Health check endpoint | S5 | `Program.cs` | 1s | BE-19 |

**Toplam Backend:** ~32 saat

### Test Tasks

| # | Task | Story | Dosya(lar) | Süre | Bağımlılık |
|---|---|---|---|---|---|
| **TE-01** | ServiceOrderAggregateTests — state machine | US-09 | `tests/.../ServiceOrders/ServiceOrderAggregateTests.cs` | 3s | — |
| **TE-02** | ServiceOrderAggregateTests — finansal hesaplar | US-11,12,14 | aynı dosya | 2s | — |
| **TE-03** | CustomerAggregateTests | US-03 | `tests/.../Customers/CustomerAggregateTests.cs` | 2s | — |
| **TE-04** | VehicleAggregateTests | US-06 | `tests/.../Vehicles/VehicleAggregateTests.cs` | 1s | — |

**Toplam Test:** ~8 saat

---

## Sprint Backlog Özeti

```
Sprint 1 Kapasitesi: ~50 saat

Must Have  (M1-M5):  23s  ██████████████████████░░░░  ~46%
Should Have (S1-S5): 14s  █████████████░░░░░░░░░░░░░  ~28%
Could Have  (C1-C3):  8s  ███████░░░░░░░░░░░░░░░░░░░  ~16%
Test tasks  (TE):     8s  ███████░░░░░░░░░░░░░░░░░░░  ~16%
```

---

## API Endpoint Haritası (Sprint 1 Sonu Hedefi)

```
POST   /api/tenants                              → US-01 Servis kaydı
GET    /api/tenants/{identifier}                 → Servis detayı
PATCH  /api/tenants/{identifier}/deactivate      → US-02

POST   /api/customers                            → US-03 Müşteri kaydı
PUT    /api/customers/{id}                       → Müşteri güncelleme
GET    /api/customers/{id}                       → US-05 Müşteri detayı
GET    /api/customers/search?q=...               → US-04 Müşteri arama

POST   /api/vehicles                             → US-06 Araç kaydı ✅
GET    /api/vehicles/{plate}                     → US-07 Plaka arama ✅
GET    /api/vehicles                             → C1 (Could have)

POST   /api/service-orders                       → US-08 İş emri açma
GET    /api/service-orders/{id}                  → US-10 İş emri detayı
GET    /api/service-orders?status=...            → İş emri listesi
PATCH  /api/service-orders/{id}/status           → US-09 Durum güncelleme
POST   /api/service-orders/{id}/operations       → US-11 İşçilik ekleme
DELETE /api/service-orders/{id}/operations/{oid} → İşçilik silme
POST   /api/service-orders/{id}/parts            → US-12 Parça ekleme
DELETE /api/service-orders/{id}/parts/{pid}      → Parça silme
POST   /api/service-orders/{id}/payments         → US-14 Ödeme kaydı
PATCH  /api/service-orders/{id}/discount         → US-13

GET    /api/dashboard/daily                      → US-15 Günlük özet
GET    /health                                   → S5
```

---

## Definition of Done (Sprint 1)

### Zorunlu (Must)
- [ ] `dotnet build src/MotorCare.sln` → 0 hata
- [ ] `dotnet test` → tüm testler yeşil
- [ ] PostgreSQL bağlantısı çalışıyor (`docker-compose up` ile)
- [ ] `dotnet ef database update` hatasız tamamlanıyor
- [ ] Swagger UI'da tüm Must endpoint'leri görünüyor
- [ ] `X-Tenant-Id: test-tenant` ile aşağıdaki uçtan uca akış çalışıyor:
  1. `POST /api/tenants` → tenant oluştur
  2. `POST /api/customers` → müşteri oluştur
  3. `POST /api/vehicles` → araç oluştur
  4. `POST /api/service-orders` → iş emri aç
  5. `POST /api/service-orders/{id}/operations` → işçilik ekle
  6. `POST /api/service-orders/{id}/parts` → parça ekle
  7. `PATCH /api/service-orders/{id}/status` → InProgress yap
  8. `PATCH /api/service-orders/{id}/status` → Completed yap
  9. `POST /api/service-orders/{id}/payments` → ödeme kaydet
  10. `GET /api/service-orders/{id}` → tüm detayları doğrula
- [ ] Controller içeren dosya sıfır (hepsi Carter module)

### Önerilen (Should)
- [ ] `GET /api/dashboard/daily` çalışıyor ve anlamlı veri döndürüyor
- [ ] Domain aggregate testleri: ServiceOrder state machine %100 kapsama
- [ ] Health check endpoint aktif

---

## Riskler (Güncellenmiş)

| Risk | Olasılık | Etki | Azaltma |
|---|---|---|---|
| `CreateServiceOrderCommand`'da Vehicle+Customer cross-tenant doğrulama | Orta | Yüksek | Handler'da her ikisi için GetByIdAsync + tenant check |
| `GetDailySummaryQuery` N+1 sorgu | Orta | Orta | Tek aggregated sorgu yaz, ayrı ayrı çağırma |
| ServiceOrder OwnsMany include performansı | Düşük | Orta | GetById'de `AsSplitQuery()` düşün |
| PostgreSQL local ortam yok | Yüksek | Yüksek | docker-compose.yml ilk iş olarak tamamlanacak (BE-19) |

---

## Sprint 2 Önizleme

Sprint 1 çıktıları hazırsa Sprint 2'de:
- JWT Authentication (tenant bazlı token)
- .NET MAUI proje iskeleti + Refit client
- ServiceOrder tam UI (liste → detay → form akışı)
- WhatsApp bildirim entegrasyonu (ilk versiyon)
- CI/CD GitHub Actions pipeline

---

*Son güncelleme: 2026-04-18 (Yön A seçimi sonrası)*  
*Sonraki review: 2026-04-25 (Sprint Mid-Review)*
