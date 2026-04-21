namespace MotorCare.App.Models.Inventory;

public sealed class AdjustInventoryStockRequest
{
    public decimal QuantityDelta { get; set; }
    public string Reason { get; set; } = string.Empty;
}
