namespace MotorCare.App.Models.Services;

public sealed class UpdateServiceCatalogItemRequest
{
    public string Name { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; } = ServiceCategory.PeriodicMaintenance;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; } = 60;
    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; } = true;
}
