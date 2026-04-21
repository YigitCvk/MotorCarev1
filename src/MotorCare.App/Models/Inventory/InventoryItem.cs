namespace MotorCare.App.Models.Inventory;

public sealed class InventoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string Unit { get; set; } = "Adet";
    public decimal UnitPrice { get; set; }
    public decimal StockQuantity { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsActive { get; set; }
}
