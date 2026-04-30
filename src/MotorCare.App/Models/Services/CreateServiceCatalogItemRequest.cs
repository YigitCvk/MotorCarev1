namespace MotorCare.App.Models.Services;

public sealed class CreateServiceCatalogItemRequest
{
    public string Name { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; } = ServiceCategory.PeriodicMaintenance;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; } = 60;
    public decimal DefaultPrice { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; } = true;
}
