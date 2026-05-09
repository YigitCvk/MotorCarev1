namespace MotorCare.App.Models.PublicRecords;

public sealed class PublicInspectionReportPreview
{
    public string InspectionNo { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
    public string PackageType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int CriticalFindingCount { get; set; }
    public string ResultSummary { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string VerificationText { get; set; } = string.Empty;
}
