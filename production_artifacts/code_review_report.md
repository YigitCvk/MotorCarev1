# Code Review Report — MotorCare SaaS

**Tarih:** 2026-04-18  
**Reviewer:** Senior .NET Architect (AI Code Review Pipeline)  
**Branch:** `feature/bootstrap-solution`  
**Kapsam:** Tüm kaynak dosyalar — Domain, Application, Infrastructure, API, Tests

---

## Özet Tablo

| Kategori | Adet |
|---|---|
| Kritik Hatalar (blocker) | 6 |
| Uyarılar (warning) | 14 |
| Öneriler (recommendation) | 10 |

**Genel Değerlendirme:** Temel mimari kurulum sağlam; DDD aggregate tasarımı özellikle ServiceOrder için olgunlaşmış. Ancak DI eksiklikleri nedeniyle uygulama şu haliyle çalışmaz durumda. Carter geçişi ve race condition acil çözüm gerektirir.

---

## KRİTİK HATALAR

> Bunlar düzeltilmeden uygulama production'da çalışamaz veya güvenlik açığı oluşturur.

---

### [C1] DI Kayıtları Eksik — `Infrastructure/DependencyInjection.cs:13`

**Önem:** Production'da runtime `InvalidOperationException` → API tamamen kullanılamaz.

`ICustomerRepository`, `IServiceOrderRepository`, `ITenantRepository`, `IOrderNumberGenerator` servisleri DI container'a hiç kaydedilmemiş. Sadece `IVehicleRepository` kayıtlı.

```csharp
// Mevcut durum (DependencyInjection.cs — sadece Vehicle var)
services.AddScoped<IVehicleRepository, VehicleRepository>();
// ICustomerRepository → yok   → CreateCustomerCommandHandler inject edemez → crash
// IServiceOrderRepository → yok
// ITenantRepository → yok
// IOrderNumberGenerator → yok
```

**Düzeltme:**
```csharp
services.AddScoped<IVehicleRepository, VehicleRepository>();
services.AddScoped<ICustomerRepository, CustomerRepository>();
services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
services.AddScoped<ITenantRepository, TenantRepository>();
services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
```

---

### [C2] GlobalExceptionHandler Pipeline'a Bağlı Değil — `Api/Program.cs`

**Önem:** Tüm exception'lar handle edilmez, stack trace client'a sızar.

`GlobalExceptionHandler` sınıfı yazılmış ama `Program.cs`'de ne DI'a ekleniyor ne de middleware olarak kullanılıyor. Uygulama şu anda tüm exception'ları ASP.NET Core default handler'a iletir.

```csharp
// Mevcut Program.cs — GlobalExceptionHandler hiç yok
builder.Services.AddControllers();
// ...
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

**Düzeltme:**
```csharp
// Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ...

app.UseExceptionHandler();   // pipeline'a ekle
app.UseHttpsRedirection();
```

---

### [C3] CLAUDE.md İhlali — `ControllerBase` Kullanımı — `Api/Controllers/VehiclesController.cs`

**Önem:** Proje sözleşmesi (`CLAUDE.md`, `GEMINI.md`) açıkça yasak koyuyor: _"No ControllerBase"_.

`VehiclesController` `ControllerBase`'den türüyor. Carter kullanılması zorunlu.

**Düzeltme — Carter module örneği:**

```csharp
// src/MotorCare.Api/Modules/VehiclesModule.cs
using Carter;
using MediatR;
using MotorCare.Application.Vehicles.Commands.CreateVehicle;
using MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

namespace MotorCare.Api.Modules;

public sealed class VehiclesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vehicles")
            .WithTags("Vehicles")
            .WithOpenApi();

        group.MapPost("/", async (CreateVehicleCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.CreatedAtRoute("GetVehicleByPlate", new { plate = command.Plate }, id);
        })
        .WithName("CreateVehicle")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{plate}", async (string plate, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetVehicleByPlateQuery(plate), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetVehicleByPlate")
        .Produces<VehicleDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
```

**Program.cs güncelleme:**
```csharp
// Carter NuGet: dotnet add package Carter

builder.Services.AddCarter();
// AddControllers() kaldır

app.MapCarter();     // MapControllers() yerine
```

---

### [C4] OrderNumberGenerator'da Race Condition — `Infrastructure/Services/OrderNumberGenerator.cs:27`

**Önem:** Eşzamanlı isteklerde aynı `OrderNo` iki kez üretilir → unique constraint violation veya data corruption.

```csharp
// Tehlikeli: Count → +1 arasında başka bir request girebilir
var todayCount = await _context.ServiceOrders
    .Where(o => o.TenantId == tenantId && ...)
    .CountAsync(cancellationToken);

var sequence = todayCount + 1; // ← başka bir thread de aynı sonucu alıyor olabilir
return $"SRV-{todayDate:yyyyMMdd}-{sequence:D4}";
```

**Düzeltme — PostgreSQL advisory lock ile:**
```csharp
public async Task<string> GenerateAsync(string tenantId, CancellationToken cancellationToken = default)
{
    // PostgreSQL sequence veya advisory lock kullan
    // Basit yaklaşım: Transaction + SELECT FOR UPDATE on a sequence table
    // Veya: daha basit — Guid tabanlı OrderNo ile başla, sequence Sprint 2'ye bırak
    
    // Geçici güvenli çözüm: şans çakışması minimize etmek için timestamp+random suffix
    var todayDate = DateTimeOffset.UtcNow;
    var todayStart = new DateTimeOffset(todayDate.Date, TimeSpan.Zero);
    var todayEnd = todayStart.AddDays(1);

    await using var transaction = await _context.Database
        .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

    var todayCount = await _context.ServiceOrders
        .Where(o => o.TenantId == tenantId && o.OpenedAt >= todayStart && o.OpenedAt < todayEnd)
        .CountAsync(cancellationToken);

    var sequence = todayCount + 1;
    var orderNo = $"SRV-{todayDate:yyyyMMdd}-{sequence:D4}";

    await transaction.CommitAsync(cancellationToken);
    return orderNo;
}
```

---

### [C5] AuditableEntity — `public set` Güvenlik Açığı — `Domain/Common/AuditableEntity.cs:5`

**Önem:** Herhangi bir dış kod `CreatedAt`/`UpdatedAt`'ı istediği değere set edebilir. DDD encapsulation ihlali.

```csharp
// Mevcut (güvensiz)
public DateTimeOffset CreatedAt { get; set; }
public DateTimeOffset? UpdatedAt { get; set; }
```

Bu setter'lar `public` olduğu için serileştirme, test veya yanlış kod bu değerleri domain dışından değiştirebilir. Infrastructure `ApplyAuditInfo()` bunu override etse de property'nin kendisi açık kalmakta.

**Düzeltme:**
```csharp
// Domain içinden Infrastructure'ın da set edebildiği `internal set`
public DateTimeOffset CreatedAt { get; internal set; }
public DateTimeOffset? UpdatedAt { get; internal set; }
```

> Not: EF Core `internal set` ile çalışır; ayrı assembly'ler için `InternalsVisibleTo` gerekir.

---

### [C6] CreateVehicleCommandHandler'da Redundant `UpdateDetails` Çağrısı — `Application/Vehicles/Commands/CreateVehicle/CreateVehicleCommandHandler.cs:46`

**Önem:** Mantıksal hata — constructor sonrası aynı Brand/Model/Year değerleriyle `UpdateDetails` çağrılıyor. EF Core'da `Modified` state'i gereksiz yere tetiklenebilir.

```csharp
// Mevcut — hatalı pattern
var vehicle = new Vehicle(tenantId, plate, request.Brand, request.Model, request.Year);

if (!string.IsNullOrWhiteSpace(request.ChassisNumber) || !string.IsNullOrWhiteSpace(request.Color))
{
    // Brand, Model, Year zaten set edildi — bunları tekrar geçmek gereksiz
    vehicle.UpdateDetails(request.Brand, request.Model, request.Year, request.ChassisNumber, request.Color);
}
```

`UpdateDetails` tüm parametreleri tekrar validate edip set ediyor. Constructor'dan hemen sonra çağrılmak için tasarlanmamış. Domain'e `SetOptionalDetails(chassisNumber, color)` gibi bir metot eklenmeli ya da constructor genişletilmeli.

**Düzeltme (Domain'e yeni metot):**
```csharp
// Vehicle.cs
public void SetChassisAndColor(string? chassisNumber, string? color)
{
    ChassisNumber = chassisNumber;
    Color = color;
}

// Handler'da
var vehicle = new Vehicle(tenantId, plate, request.Brand, request.Model, request.Year);
vehicle.SetChassisAndColor(request.ChassisNumber, request.Color);
if (request.CurrentKm.HasValue) vehicle.UpdateMileage(request.CurrentKm.Value);
if (request.CurrentCustomerId.HasValue) vehicle.AssignCustomer(request.CurrentCustomerId.Value);
```

---

## UYARILAR

> Hata üretebilir veya kod kalitesini önemli ölçüde düşürür. Sprint içinde çözülmeli.

---

### [W1] `sealed record` Eksikliği — Tüm Commands, Queries, DTOs

CLAUDE.md: _"sealed record for DTOs, Commands, Queries"_. Hiçbir record `sealed` değil.

```csharp
// Mevcut
public record CreateCustomerCommand(...) : IRequest<Guid>;
public record GetVehicleByPlateQuery(...) : IRequest<VehicleDto?>;
public record VehicleDto(...);

// Olması gereken
public sealed record CreateCustomerCommand(...) : IRequest<Guid>;
public sealed record GetVehicleByPlateQuery(...) : IRequest<VehicleDto?>;
public sealed record VehicleDto(...);
```

**Etkilenen dosyalar:** `CreateCustomerCommand.cs`, `UpdateCustomerCommand.cs`, `CreateVehicleCommand.cs`, `GetVehicleByPlateQuery.cs` (VehicleDto dahil).

---

### [W2] ValueObject.GetHashCode — XOR Hash Weak Collision Risk — `Domain/Common/ValueObject.cs:35`

```csharp
// Mevcut — XOR kombinasyon collision'a açık
return GetEqualityComponents()
    .Select(x => x != null ? x.GetHashCode() : 0)
    .Aggregate((x, y) => x ^ y);
```

XOR ile hash combine etmek simetrik olduğu için `(A, B)` ve `(B, A)` aynı hash'i üretir. `HashCode.Combine` kullanılmalı.

```csharp
public override int GetHashCode()
{
    var hash = new HashCode();
    foreach (var component in GetEqualityComponents())
        hash.Add(component);
    return hash.ToHashCode();
}
```

---

### [W3] `ArgumentException` vs `DomainException` Tutarsızlığı

- `Customer.cs:19` → `throw new ArgumentException("Tenant ID is required.")` — paramName eksik
- `Customer.cs:20` → `throw new ArgumentException("Full name is required.")` — paramName eksik
- `Vehicle.cs:30` → `throw new ArgumentNullException(nameof(plate))` — diğerleri DomainException
- `PlateNumber.cs:19` → `throw new ArgumentException(...)` — business kuralı DomainException olmalı

**Kural:** Constructor parametresi geçersizse `ArgumentException`/`ArgumentNullException` (paramName dahil); business invariant ihlalinde `DomainException`.

```csharp
// Customer.cs — düzeltme
if (string.IsNullOrWhiteSpace(tenantId))
    throw new ArgumentException("Tenant ID is required.", nameof(tenantId));
if (string.IsNullOrWhiteSpace(fullName))
    throw new ArgumentException("Full name is required.", nameof(fullName));

// PlateNumber.cs — business rule olduğu için DomainException
if (string.IsNullOrWhiteSpace(plate))
    throw new DomainException("Plate number cannot be empty.");
```

---

### [W4] PhoneNumber — OriginalValue Korunmuyor — `Domain/ValueObjects/PhoneNumber.cs`

`PlateNumber`'da hem `OriginalValue` hem `NormalizedValue` var (kullanıcıya orijinal gösterilebilir). `PhoneNumber`'da ise yalnızca `Value` (normalize edilmiş). Tutarsızlık; UI'de kullanıcının girdiği `+90 532 123 45 67` formatı kaybolur.

```csharp
// PhoneNumber.cs — önerilen ekleme
public string OriginalValue { get; private set; }

private PhoneNumber(string originalValue, string normalizedValue)
{
    OriginalValue = originalValue;
    Value = normalizedValue;
}

public static PhoneNumber Create(string phoneNumber)
{
    // ...
    return new PhoneNumber(phoneNumber, normalized);
}
```

---

### [W5] ValidationBehavior `FluentValidation.ValidationException` Fırlatıyor — `AppValidationException` Dead Code

`ValidationBehavior.cs:32` → `throw new ValidationException(failures)` (`FluentValidation.ValidationException`)  
`AppValidationException` ise hiç throw edilmiyor → `GlobalExceptionHandler`'daki `AppValidationException` branch ölü kod.

**Seçenekler:**
1. `ValidationBehavior`'ı `AppValidationException` throw edecek şekilde güncelle (sonra GlobalExceptionHandler sadece bunu handle eder)
2. `AppValidationException`'ı sil, yalnızca `FluentValidation.ValidationException` kullan

```csharp
// Tercih edilen: AppValidationException'ı throw et
if (failures.Count != 0)
    throw new AppValidationException(failures);
```

---

### [W6] Repository'lerde `SaveChangesAsync` — Unit of Work Anti-Pattern

Her repository kendi `SaveChangesAsync` metodunu expose ediyor ve handler'lar bunu tek tek çağırıyor. Birden fazla aggregate'in aynı transaction'da değişmesi gerektiğinde atomiklik kırılır.

```csharp
// UpdateCustomerCommandHandler.cs
_repository.Update(customer);
await _repository.SaveChangesAsync(cancellationToken); // ← Customer repository'nin SaveChanges'i

// Eğer başka bir aggregate de değişseydi (örn. Vehicle) ayrı SaveChanges → atomik değil
```

Uzun vadede `IUnitOfWork` pattern veya handler'dan `DbContext.SaveChangesAsync` çağrısı. Sprint 1'de en azından notlanmalı.

---

### [W7] CustomerRepository.SearchAsync — PostgreSQL'de Sorgusuz `ToLower()` — `Infrastructure/Repositories/CustomerRepository.cs:35`

```csharp
// EF Core bu ifadeyi LOWER(column) LIKE '%term%' olarak çevirir
// PostgreSQL için daha verimli yaklaşım EF.Functions.ILike
var term = searchTerm.ToLower();
query = query.Where(c => c.FullName.ToLower().Contains(term));
```

```csharp
// Düzeltme
query = query.Where(c =>
    EF.Functions.ILike(c.FullName, $"%{searchTerm}%") ||
    (c.Email != null && EF.Functions.ILike(c.Email, $"%{searchTerm}%")));
```

---

### [W8] DependencyInjection.cs — Hardcoded Fallback Connection String — `Infrastructure/DependencyInjection.cs:21`

```csharp
// Tehlikeli: null coalescing ile hardcoded default
configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=MotorCareDb;Username=postgres;Password=postgres"
```

Configuration eksikse sessizce localhost'a bağlanır; production'da fark edilmez. Exception fırlatmalı.

```csharp
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
```

---

### [W9] `GetVehicleByPlateQueryHandler` — Kullanılmayan `using` — `Application/Queries/GetVehicleByPlate/GetVehicleByPlateQueryHandler.cs:6`

```csharp
using MotorCare.Domain.Common; // ← kullanılmıyor, derleyici uyarısı
```

---

### [W10] VehicleRepository.GetByIdAsync — Global Query Filter + Explicit TenantId Çift Filtresi

```csharp
// Global query filter zaten TenantId filtreler
// Explicit v.TenantId == tenantId redundant
.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId, ...)
```

Bu kendisi bir hata değil ama `IgnoreQueryFilters()` ile bypass edildiğinde explicit filtre güvence sağlar. Kasıtlıysa yorum eklenmeli. Kasıtsızsa sadece `v.Id == id` yeterli.

---

### [W11] VehicleNote — `UpdateContent` Metodu Public, Child Entity için Uygun Değil

```csharp
// VehicleNote.cs:19 — public erişim
public void UpdateContent(string content) { ... }
```

Child entity. `Vehicle` aggregate'i üzerinden değiştirilmeli (domain method). Doğrudan erişim aggregate invariant'larını bypass eder. `internal` olmalı.

---

### [W12] Test Projesi .NET 9 — Diğer Projeler .NET 8

`MotorCare.Domain.UnitTests.csproj` `.NET 9.0` hedefliyor; `MotorCare.Domain` `.NET 8.0`. CI ortamında SDK farklılığı build hatası yaratabilir. Test projesi de `.NET 8.0`'a indirilmeli.

---

### [W13] `Result<T>` ve `ISoftDelete` — Dead Code

`Result<T>` (`Application/Common/Models/Result.cs`) ve `ISoftDelete` (`Domain/Common/ISoftDelete.cs`) tanımlı ama hiçbir yerde kullanılmıyor. Proje gereksiz API yüzeyine sahip.

- `Result<T>` → ya handler'lara standardize edilmeli ya silinmeli
- `ISoftDelete` → implement eden aggregate yoksa silinmeli

---

### [W14] ServiceOrder.RecalculateTotals — `AddPayment` Sonrası PaidTotal Çift Hesaplanıyor — `Domain/ServiceOrders/ServiceOrder.cs:111`

```csharp
public void AddPayment(decimal amount, PaymentMethod method, DateTimeOffset paymentDate)
{
    // ...
    _payments.Add(new ServicePayment(amount, method, paymentDate));
    PaidTotal = _payments.Sum(p => p.Amount); // ← doğrudan set
}

private void RecalculateTotals()
{
    // ...
    PaidTotal = _payments.Sum(p => p.Amount); // ← aynı hesaplama burada da var
}
```

`AddPayment` `RecalculateTotals()` çağırmadığı için özellikle `PaidTotal`'ı kendi set ediyor. Tutarlılık için `AddPayment` da `RecalculateTotals()` çağırmalı; ama bu durumda `AddPayment` validation'ında `RemainingTotal` kullanılıyor ve `RecalculateTotals` `PaidTotal`'ı `_payments.Sum()` ile hesaplıyor — çelişki yok ama maintainability açısından kötü.

```csharp
public void AddPayment(decimal amount, PaymentMethod method, DateTimeOffset paymentDate)
{
    EnsureModifiable();
    if (amount <= 0) throw new DomainException("Payment amount must be greater than zero.");
    if (amount > RemainingTotal) throw new DomainException($"Payment amount ({amount}) exceeds the remaining total ({RemainingTotal}).");

    _payments.Add(new ServicePayment(amount, method, paymentDate));
    RecalculateTotals(); // PaidTotal dahil tüm totaller RecalculateTotals'ta
}
```

---

## ÖNERİLER

> Kod kalitesi, DDD uyumu ve uzun vadeli sürdürülebilirlik için.

---

### [R1] Carter Geçişi — Tam Adım Adım Talimat

**1. NuGet:**
```bash
dotnet add src/MotorCare.Api/MotorCare.Api.csproj package Carter --version 8.*
```

**2. Program.cs:**
```csharp
// Kaldır
builder.Services.AddControllers();

// Ekle
builder.Services.AddCarter();

// Kaldır
app.MapControllers();

// Ekle
app.MapCarter();
```

**3. Swagger (Carter 8.x ile uyumlu):**
Carter, `AddEndpointsApiExplorer()` + `AddSwaggerGen()` ile doğrudan çalışır. `WithOpenApi()` her route group'a eklenmeli.

**4. Module dosya yapısı:**
```
src/MotorCare.Api/
├── Modules/
│   ├── VehiclesModule.cs
│   ├── CustomersModule.cs
│   ├── ServiceOrdersModule.cs
│   └── TenantsModule.cs
├── Middleware/
│   └── GlobalExceptionHandler.cs   (değişmez)
└── Program.cs
```

**5. `Controllers/` klasörünü sil** — içindeki tüm `ControllerBase` dosyaları.

---

### [R2] Domain'de `sealed` Class Politikası

Exception hierarchy'de tutarlılık için:

```csharp
// Mevcut — açık inheritance
public class DomainException : Exception { }
public class ConflictException : Exception { }
public class NotFoundException : Exception { }

// Öneri — exception'lar sealed
public sealed class DomainException : Exception { }
public sealed class ConflictException : Exception { }
public sealed class NotFoundException : Exception { }
public sealed class AppValidationException : Exception { }
```

---

### [R3] `VehicleDto` Ayrı Dosyaya Taşınmalı

`GetVehicleByPlateQuery.cs` hem query'yi hem DTO'yu barındırıyor. Birden fazla query aynı DTO'ya ihtiyaç duyduğunda coupling oluşur.

```
Vehicles/
├── Commands/
└── Queries/
    ├── GetVehicleByPlate/
    │   ├── GetVehicleByPlateQuery.cs       (sadece query)
    │   └── GetVehicleByPlateQueryHandler.cs
    └── Dtos/
        └── VehicleDto.cs                   (paylaşılan DTO)
```

---

### [R4] Domain Events Implement Edilmeli — Altyapı Hazır

`IDomainEvent`, `AddDomainEvent`, `ClearDomainEvents` hazır ama sıfır event tanımlı. Minimum olarak:

```csharp
// Domain/ServiceOrders/Events/ServiceOrderCreatedEvent.cs
public sealed record ServiceOrderCreatedEvent(Guid OrderId, string TenantId, string OrderNo) : IDomainEvent;

// ServiceOrder.cs constructor
public ServiceOrder(...)
{
    // ... kurulum
    AddDomainEvent(new ServiceOrderCreatedEvent(Id, TenantId, OrderNo));
}
```

MediatR notification + publisher pattern ile publish edilmeli (Infrastructure layer'da `IDomainEventPublisher`).

---

### [R5] `PagedResult<T>` — `List<T>` Yerine `IReadOnlyList<T>`

```csharp
// Mevcut — mutable collection expose ediyor
public List<T> Items { get; }

// Öneri
public IReadOnlyList<T> Items { get; }
```

---

### [R6] GetTodayOrderCountAsync — UTC vs Local Time Bug Riski — `Infrastructure/Repositories/ServiceOrderRepository.cs:61`

```csharp
// ServiceOrderRepository.cs:61
var todayStart = DateTimeOffset.UtcNow.Date; // DateTimeOffset.Date → DateTime (UTC)
// Bu aslında DateTimeOffset değil, implicit DateTime dönüşümü

// Güvenli yaklaşım
var now = DateTimeOffset.UtcNow;
var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
var todayEnd = todayStart.AddDays(1);
```

---

### [R7] `HeaderTenantProvider` — Gereksiz `using`

```csharp
using MotorCare.Domain.Common; // ← kullanılmıyor
```

---

### [R8] Vehicle.AssignCustomer — Cross-Aggregate Referans Doğrulaması Application Katmanında Yapılmalı

```csharp
// Vehicle.cs — sadece ID alıyor, doğrulama yok
public void AssignCustomer(Guid customerId)
{
    CurrentCustomerId = customerId;
}
```

Handler'da `ICustomerRepository.GetByIdAsync` ile customer'ın varlığı ve aynı tenant'a ait olduğu doğrulanmalı. Bu domain değil, application concern.

---

### [R9] AggregateRoot.RowVersion — `Array.Empty<byte>()` EF Core Davranışıyla Uyumsuz Olabilir

```csharp
// AggregateRoot.cs:13
public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
```

PostgreSQL'de `xmin` column, EF Core tarafından `uint` olarak okunur ve `byte[]`'e map edilir. `Array.Empty<byte>()` başlangıç değeri, `Added` state'inde EF Core'un `xmin` değeri set etmesini beklediği için sorun çıkarmaz. Ancak explicit `= null!` veya `= default!` kullanımı intent'i daha net gösterir.

---

### [R10] CustomerRepository.SearchAsync — Sayfalama Eksik

```csharp
// Mevcut — tüm müşterileri döndürür, sayfalama yok
return await query.OrderBy(c => c.FullName).ToListAsync(cancellationToken);
```

`ICustomerRepository.SearchAsync` signature'ı `List<Customer>` döndürüyor. Büyük tenant'larda performans sorunu çıkar. `SearchAsync(tenantId, searchTerm, page, pageSize, ct)` olarak güncellenmeli ve `PagedResult<Customer>` döndürmeli.

---

## CONTROLLER → CARTER GEÇİŞ ÖNCESİ / SONRASI

### Mevcut Sorunlar (Controller)

| Sorun | Detay |
|---|---|
| CLAUDE.md ihlali | `ControllerBase` yasak |
| Swagger routing | `[Route("api/[controller]")]` Carter'da yok |
| Boilerplate | `ActionResult<T>`, `CreatedAtAction`, `Ok()`, `NotFound()` manuel |
| Response type docs | `[ProducesResponseType]` attribute gerektiriyor |

### Carter Sonrası Avantajlar

| Özellik | Carter |
|---|---|
| Minimal API tabanlı | Daha az boilerplate |
| `Results.Created`, `Results.Ok`, `Results.NotFound` | Fluent, açık |
| `.Produces<T>()`, `.ProducesProblem()` | Swagger otomatik |
| `WithTags()`, `WithOpenApi()` | API grouping |
| Route naming | `.WithName()` ile tipli |

---

## KRİTİK DÜZELTME SIRASI (Önerilen)

```
1. [C2] GlobalExceptionHandler → Program.cs'e ekle           (10 dk)
2. [C1] DI eksik registrations → DependencyInjection.cs      (5 dk)
3. [C3] Controller → Carter geçişi                           (45 dk)
4. [C4] OrderNumberGenerator race condition                   (30 dk)
5. [C5] AuditableEntity public setter → internal set         (5 dk)
6. [C6] CreateVehicleCommandHandler redundant UpdateDetails   (15 dk)
7. [W1] sealed record tüm Commands/Queries/DTOs              (10 dk)
8. [W5] ValidationBehavior → AppValidationException          (10 dk)
9. [W8] Hardcoded connection string → exception              (5 dk)
```

---

## İyi Yapılanlar (Korunmalı)

- **ServiceOrder aggregate state machine:** Mükemmel finite state machine implementasyonu; `EnsureModifiable()` pattern doğru.
- **Multi-tenant query filters:** `DbContext` seviyesinde global filter + `EnforceTenantSecurity()` kombinasyonu güçlü güvenlik katmanı.
- **OwnsMany/OwnsOne EF konfigürasyonu:** Value object ve child entity mapping'i doğru, field-level access mode ayarlı.
- **ValidationBehavior pipeline:** MediatR pipeline integration temiz, validator'lar paralel çalıştırılıyor.
- **RFC 9110 ProblemDetails:** Exception→HTTP mapping kapsamlı ve standarda uygun.
- **internal constructor** child entity'lerde: `ServiceOperationItem`, `VehicleNote` vb. `internal` constructor ile aggregate dışından doğrudan oluşturma engellenmiş.
- **Optimistic concurrency:** `RowVersion` + `xmin` doğru setup.
- **PlateNumber normalization:** Türkçe plaka formatlarını handle ediyor, test coverage var.

---

*Rapor oluşturuldu: 2026-04-18*  
*Sonraki review: Sprint 1 tamamlanınca*
