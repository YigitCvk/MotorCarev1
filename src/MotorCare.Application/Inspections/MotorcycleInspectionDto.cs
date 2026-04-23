using MotorCare.Domain.Enums;

namespace MotorCare.Application.Inspections;

public sealed record MotorcycleInspectionItemDto(
    Guid Id,
    MotorcycleInspectionCategory Category,
    string CategoryText,
    string Name,
    MotorcycleInspectionResult Result,
    string ResultText,
    string? Notes,
    int SortOrder);

public sealed record MotorcycleInspectionListItemDto(
    Guid Id,
    string InspectionNo,
    Guid? CustomerId,
    Guid? VehicleId,
    string CustomerName,
    string Phone,
    string Plate,
    MotorcycleInspectionPackageType PackageType,
    string PackageTypeText,
    MotorcycleInspectionStatus Status,
    string StatusText,
    decimal PackagePrice,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record MotorcycleInspectionDto(
    Guid Id,
    string InspectionNo,
    Guid? CustomerId,
    Guid? VehicleId,
    string CustomerName,
    string Phone,
    string Plate,
    string? Brand,
    string? Model,
    int? Year,
    int? Mileage,
    string? ChassisNumber,
    string? EngineNumber,
    string? Query5664,
    string? MileageQuery,
    MotorcycleInspectionPackageType PackageType,
    string PackageTypeText,
    MotorcycleInspectionStatus Status,
    string StatusText,
    decimal PackagePrice,
    string? GeneralNotes,
    string? TestRideNotes,
    string? CosmeticNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<MotorcycleInspectionItemDto> Items);

public static class MotorcycleInspectionTextMapper
{
    public static string ToText(MotorcycleInspectionPackageType packageType) => packageType switch
    {
        MotorcycleInspectionPackageType.MechanicalAndRunningGear => "Mekanik ve Yürüyen Aksam",
        MotorcycleInspectionPackageType.BodyAndFairing => "Karenaj ve Kaporta",
        MotorcycleInspectionPackageType.ObdAndElectrical => "OBD Test ve Elektrik/Elektronik",
        MotorcycleInspectionPackageType.Full => "Full Ekspertiz",
        _ => packageType.ToString()
    };

    public static string ToText(MotorcycleInspectionStatus status) => status switch
    {
        MotorcycleInspectionStatus.Draft => "Taslak",
        MotorcycleInspectionStatus.InProgress => "Devam Ediyor",
        MotorcycleInspectionStatus.Completed => "Tamamlandı",
        MotorcycleInspectionStatus.Cancelled => "İptal",
        _ => status.ToString()
    };

    public static string ToText(MotorcycleInspectionCategory category) => category switch
    {
        MotorcycleInspectionCategory.MechanicalAndRunningGear => "Mekanik",
        MotorcycleInspectionCategory.BodyAndFairing => "Karenaj / Kaporta",
        MotorcycleInspectionCategory.ObdAndElectrical => "OBD / Elektronik",
        MotorcycleInspectionCategory.TestRide => "Test Sürüşü",
        MotorcycleInspectionCategory.General => "Genel",
        _ => category.ToString()
    };

    public static string ToText(MotorcycleInspectionResult result) => result switch
    {
        MotorcycleInspectionResult.NotChecked => "Kontrol Edilmedi",
        MotorcycleInspectionResult.Good => "İyi",
        MotorcycleInspectionResult.Medium => "Orta",
        MotorcycleInspectionResult.Bad => "Kötü",
        MotorcycleInspectionResult.NotAvailable => "Yok",
        MotorcycleInspectionResult.Exists => "Var",
        MotorcycleInspectionResult.NotExists => "Yok",
        MotorcycleInspectionResult.Damaged => "Hasarlı",
        MotorcycleInspectionResult.Painted => "Boyalı",
        MotorcycleInspectionResult.Original => "Orijinal",
        MotorcycleInspectionResult.Changed => "Değişen",
        MotorcycleInspectionResult.Scratched => "Çizik",
        MotorcycleInspectionResult.Missing => "Yok",
        _ => result.ToString()
    };
}
