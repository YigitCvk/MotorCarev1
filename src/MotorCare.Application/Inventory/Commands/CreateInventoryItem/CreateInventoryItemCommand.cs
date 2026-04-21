using MediatR;

namespace MotorCare.Application.Inventory.Commands.CreateInventoryItem;

public sealed record CreateInventoryItemCommand(
    string Name,
    string? Sku,
    string? Barcode,
    string? Category,
    string? Brand,
    string Unit,
    decimal UnitPrice,
    decimal StockQuantity,
    decimal MinimumStockLevel,
    bool IsActive = true) : IRequest<Guid>;
