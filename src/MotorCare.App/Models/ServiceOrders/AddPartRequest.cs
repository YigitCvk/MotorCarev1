namespace MotorCare.App.Models.ServiceOrders;

public sealed class AddPartRequest
{
    public string PartName { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
