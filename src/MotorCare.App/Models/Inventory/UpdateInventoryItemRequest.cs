namespace MotorCare.App.Models.Inventory;

public sealed class UpdateInventoryItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string Unit { get; set; } = "Adet";
    public decimal UnitPrice { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public bool IsActive { get; set; } = true;
}
