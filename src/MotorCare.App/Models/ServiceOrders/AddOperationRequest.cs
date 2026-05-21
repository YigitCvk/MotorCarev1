namespace MotorCare.App.Models.ServiceOrders;

public sealed class AddOperationRequest
{
    public Guid? ServiceCatalogItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public string? Notes { get; set; }
}
