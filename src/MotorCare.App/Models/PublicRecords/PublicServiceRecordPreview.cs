namespace MotorCare.App.Models.PublicRecords;

public sealed class PublicServiceRecordPreview
{
    public string OrderNo { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string? VehiclePlate { get; set; }
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
    public string ServiceSummary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string VerificationText { get; set; } = string.Empty;
}
