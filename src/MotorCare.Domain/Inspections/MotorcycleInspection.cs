using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Inspections.Entities;

namespace MotorCare.Domain.Inspections;

public sealed class MotorcycleInspection : AggregateRoot, ITenantEntity
{
    private readonly List<MotorcycleInspectionItem> _items = [];

    public string TenantId { get; private set; } = string.Empty;
    public string InspectionNo { get; private set; } = string.Empty;
    public Guid? CustomerId { get; private set; }
    public Guid? VehicleId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Plate { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public int? Year { get; private set; }
    public int? Mileage { get; private set; }
    public string? ChassisNumber { get; private set; }
    public string? EngineNumber { get; private set; }
    public string? Query5664 { get; private set; }
    public string? MileageQuery { get; private set; }
    public MotorcycleInspectionPackageType PackageType { get; private set; }
    public MotorcycleInspectionStatus Status { get; private set; }
    public decimal PackagePrice { get; private set; }
    public string? GeneralNotes { get; private set; }
    public string? TestRideNotes { get; private set; }
    public string? CosmeticNotes { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyCollection<MotorcycleInspectionItem> Items => _items;

    private MotorcycleInspection()
    {
    }

    public MotorcycleInspection(
        string tenantId,
        string inspectionNo,
        Guid? customerId,
        Guid? vehicleId,
        string customerName,
        string phone,
        string plate,
        string? brand,
        string? model,
        int? year,
        int? mileage,
        string? chassisNumber,
        string? engineNumber,
        string? query5664,
        string? mileageQuery,
        MotorcycleInspectionPackageType packageType,
        string? generalNotes,
        string? testRideNotes,
        string? cosmeticNotes,
        IEnumerable<MotorcycleInspectionItemTemplate> itemTemplates)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new DomainException("Tenant ID gereklidir.");
        }

        if (string.IsNullOrWhiteSpace(inspectionNo))
        {
            throw new DomainException("Ekspertiz numarası gereklidir.");
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        InspectionNo = inspectionNo.Trim();
        Status = MotorcycleInspectionStatus.Draft;

        UpdateDetails(
            customerId,
            vehicleId,
            customerName,
            phone,
            plate,
            brand,
            model,
            year,
            mileage,
            chassisNumber,
            engineNumber,
            query5664,
            mileageQuery,
            packageType,
            generalNotes,
            testRideNotes,
            cosmeticNotes,
            itemTemplates);
    }

    public void UpdateDetails(
        Guid? customerId,
        Guid? vehicleId,
        string customerName,
        string phone,
        string plate,
        string? brand,
        string? model,
        int? year,
        int? mileage,
        string? chassisNumber,
        string? engineNumber,
        string? query5664,
        string? mileageQuery,
        MotorcycleInspectionPackageType packageType,
        string? generalNotes,
        string? testRideNotes,
        string? cosmeticNotes,
        IEnumerable<MotorcycleInspectionItemTemplate> itemTemplates)
    {
        EnsureEditable();
        Validate(customerId, vehicleId, customerName, phone, plate, packageType, year, mileage);

        CustomerId = customerId;
        VehicleId = vehicleId;
        CustomerName = customerId.HasValue ? NormalizeRequired(customerName, "Müşteri adı gereklidir.") : NormalizeRequired(customerName, "Müşteri adı gereklidir.");
        Phone = customerId.HasValue ? NormalizeRequired(phone, "Telefon gereklidir.") : NormalizeRequired(phone, "Telefon gereklidir.");
        Plate = NormalizeRequired(plate, "Plaka gereklidir.").ToUpperInvariant();
        Brand = NormalizeOptional(brand);
        Model = NormalizeOptional(model);
        Year = year;
        Mileage = mileage;
        ChassisNumber = NormalizeOptional(chassisNumber);
        EngineNumber = NormalizeOptional(engineNumber);
        Query5664 = NormalizeOptional(query5664);
        MileageQuery = NormalizeOptional(mileageQuery);
        var shouldReplaceTemplate = _items.Count == 0 || PackageType != packageType;
        PackageType = packageType;
        PackagePrice = GetPackagePrice(packageType);
        GeneralNotes = NormalizeOptional(generalNotes);
        TestRideNotes = NormalizeOptional(testRideNotes);
        CosmeticNotes = NormalizeOptional(cosmeticNotes);

        if (shouldReplaceTemplate)
        {
            ApplyChecklistTemplate(itemTemplates);
        }
    }

    public void UpdateItem(Guid itemId, MotorcycleInspectionResult result, string? notes)
    {
        EnsureEditable();

        var item = _items.FirstOrDefault(x => x.Id == itemId)
            ?? throw new DomainException("Ekspertiz kalemi bulunamadı.");

        item.Update(result, notes);

        if (Status == MotorcycleInspectionStatus.Draft)
        {
            Status = MotorcycleInspectionStatus.InProgress;
        }
    }

    public void Complete()
    {
        if (Status == MotorcycleInspectionStatus.Cancelled)
        {
            throw new DomainException("İptal edilen ekspertiz tamamlanamaz.");
        }

        Status = MotorcycleInspectionStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == MotorcycleInspectionStatus.Completed)
        {
            throw new DomainException("Tamamlanan ekspertiz iptal edilemez.");
        }

        Status = MotorcycleInspectionStatus.Cancelled;
        CompletedAt = null;
    }

    private void ApplyChecklistTemplate(IEnumerable<MotorcycleInspectionItemTemplate> itemTemplates)
    {
        var templates = itemTemplates
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .OrderBy(x => x.SortOrder)
            .ToList();

        if (templates.Count == 0)
        {
            throw new DomainException("Ekspertiz için en az bir kontrol kalemi gereklidir.");
        }

        _items.Clear();
        foreach (var template in templates)
        {
            _items.Add(new MotorcycleInspectionItem(template.Category, template.Name, template.SortOrder));
        }
    }

    private void EnsureEditable()
    {
        if (Status == MotorcycleInspectionStatus.Completed)
        {
            throw new DomainException("Tamamlanan ekspertiz düzenlenemez.");
        }

        if (Status == MotorcycleInspectionStatus.Cancelled)
        {
            throw new DomainException("İptal edilen ekspertiz düzenlenemez.");
        }
    }

    private static void Validate(
        Guid? customerId,
        Guid? vehicleId,
        string customerName,
        string phone,
        string plate,
        MotorcycleInspectionPackageType packageType,
        int? year,
        int? mileage)
    {
        if (!Enum.IsDefined(packageType))
        {
            throw new DomainException("Ekspertiz paketi gereklidir.");
        }

        if (!vehicleId.HasValue && string.IsNullOrWhiteSpace(plate))
        {
            throw new DomainException("Araç seçilmediyse plaka zorunludur.");
        }

        if (!customerId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                throw new DomainException("Müşteri seçilmediyse müşteri adı zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new DomainException("Müşteri seçilmediyse telefon zorunludur.");
            }
        }

        if (year.HasValue && year.Value < 1900)
        {
            throw new DomainException("Model yılı geçersiz.");
        }

        if (mileage.HasValue && mileage.Value < 0)
        {
            throw new DomainException("Kilometre negatif olamaz.");
        }
    }

    public static decimal GetPackagePrice(MotorcycleInspectionPackageType packageType) => packageType switch
    {
        MotorcycleInspectionPackageType.MechanicalAndRunningGear => 1000m,
        MotorcycleInspectionPackageType.BodyAndFairing => 1000m,
        MotorcycleInspectionPackageType.ObdAndElectrical => 1000m,
        MotorcycleInspectionPackageType.Full => 2500m,
        _ => throw new DomainException("Geçersiz ekspertiz paketi.")
    };

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
