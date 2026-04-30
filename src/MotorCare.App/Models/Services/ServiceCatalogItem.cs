namespace MotorCare.App.Models.Services;

public sealed class ServiceCatalogItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public string CategoryText { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public decimal DefaultPrice { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; }
}
