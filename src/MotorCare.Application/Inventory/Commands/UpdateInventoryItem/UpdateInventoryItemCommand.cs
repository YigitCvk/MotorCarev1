using MediatR;

namespace MotorCare.Application.Inventory.Commands.UpdateInventoryItem;

public sealed record UpdateInventoryItemCommand(
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
    bool IsActive) : IRequest;
