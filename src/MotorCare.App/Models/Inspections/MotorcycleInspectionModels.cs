namespace MotorCare.App.Models.Inspections;

public enum MotorcycleInspectionPackageType
{
    MechanicalAndRunningGear = 1,
    BodyAndFairing = 2,
    ObdAndElectrical = 3,
    Full = 4
}

public enum MotorcycleInspectionStatus
{
    Draft = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum MotorcycleInspectionCategory
{
    MechanicalAndRunningGear = 1,
    BodyAndFairing = 2,
    ObdAndElectrical = 3,
    TestRide = 4,
    General = 5
}

public enum MotorcycleInspectionResult
{
    NotChecked = 0,
    Good = 1,
    Medium = 2,
    Bad = 3,
    NotAvailable = 4,
    Exists = 5,
    NotExists = 6,
    Damaged = 7,
    Painted = 8,
    Original = 9,
    Changed = 10,
    Scratched = 11,
    Missing = 12
}

public sealed class MotorcycleInspectionListItem
{
    public Guid Id { get; set; }
    public string InspectionNo { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public MotorcycleInspectionPackageType PackageType { get; set; }
    public string PackageTypeText { get; set; } = string.Empty;
    public MotorcycleInspectionStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal PackagePrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class MotorcycleInspectionItem
{
    public Guid Id { get; set; }
    public MotorcycleInspectionCategory Category { get; set; }
    public string CategoryText { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MotorcycleInspectionResult Result { get; set; }
    public string ResultText { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

public sealed class MotorcycleInspectionDetail
{
    public Guid Id { get; set; }
    public string InspectionNo { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public int? Mileage { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? Query5664 { get; set; }
    public string? MileageQuery { get; set; }
    public MotorcycleInspectionPackageType PackageType { get; set; }
    public string PackageTypeText { get; set; } = string.Empty;
    public MotorcycleInspectionStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public decimal PackagePrice { get; set; }
    public string? GeneralNotes { get; set; }
    public string? TestRideNotes { get; set; }
    public string? CosmeticNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public List<MotorcycleInspectionItem> Items { get; set; } = [];
}

public class CreateMotorcycleInspectionRequest
{
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public int? Mileage { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? Query5664 { get; set; }
    public string? MileageQuery { get; set; }
    public MotorcycleInspectionPackageType PackageType { get; set; } = MotorcycleInspectionPackageType.Full;
    public string? GeneralNotes { get; set; }
    public string? TestRideNotes { get; set; }
    public string? CosmeticNotes { get; set; }
}

public sealed class UpdateMotorcycleInspectionRequest : CreateMotorcycleInspectionRequest
{
}

public sealed class UpdateMotorcycleInspectionItemRequest
{
    public MotorcycleInspectionResult Result { get; set; }
    public string? Notes { get; set; }
}

public sealed class CreateMotorcycleInspectionResponse
{
    public Guid Id { get; set; }
    public string InspectionNo { get; set; } = string.Empty;
}
