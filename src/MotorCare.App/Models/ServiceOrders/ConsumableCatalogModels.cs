namespace MotorCare.App.Models.ServiceOrders;

public sealed class ConsumableCatalogItemDto
{
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public string? Notes { get; set; }
}

public sealed class TrackConsumableCatalogItemRequest
{
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Specification { get; set; }
    public string? Notes { get; set; }
}

public sealed class TrackConsumableCatalogUsageRequest
{
    public IReadOnlyList<TrackConsumableCatalogItemRequest> Items { get; set; } = [];
}
