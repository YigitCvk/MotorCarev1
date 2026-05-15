namespace MotorCare.App.Models.PublicRecords;

public sealed class PublicInspectionReportItem
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}

public sealed class PublicInspectionReport
{
    public string InspectionNo { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public int? VehicleMileage { get; set; }
    public string PackageType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int CriticalFindingCount { get; set; }
    public string ResultSummary { get; set; } = string.Empty;
    public string? GeneralNotes { get; set; }
    public string? TestRideNotes { get; set; }
    public string? CosmeticNotes { get; set; }
    public List<PublicInspectionReportItem> Items { get; set; } = [];
    public string? BusinessName { get; set; }
    public string VerificationText { get; set; } = string.Empty;
}
