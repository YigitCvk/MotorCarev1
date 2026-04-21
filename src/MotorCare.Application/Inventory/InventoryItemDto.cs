namespace MotorCare.Application.Inventory;

public sealed record InventoryItemDto(
    Guid Id,
    string Name,
    string? Sku,
    string? Barcode,
    string? Category,
    string? Brand,
    string Unit,
    decimal UnitPrice,
    decimal StockQuantity,
    decimal MinimumStockLevel,
    bool IsLowStock,
    bool IsActive);
