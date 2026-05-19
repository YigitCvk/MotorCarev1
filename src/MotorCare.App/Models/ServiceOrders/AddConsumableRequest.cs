namespace MotorCare.App.Models.ServiceOrders;

public sealed class AddConsumableRequest
{
    public string Category { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? SubCategory { get; set; }
    public string? Specification { get; set; }
    public string? Notes { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}
