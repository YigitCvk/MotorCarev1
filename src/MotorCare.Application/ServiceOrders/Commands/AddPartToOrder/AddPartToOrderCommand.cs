using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.AddPartToOrder;

public sealed record AddPartToOrderCommand(
    Guid Id,
    string PartName,
    string? PartNumber,
    decimal UnitPrice,
    int Quantity,
    Guid? InventoryItemId = null,
    decimal Discount = 0m,
    string? Notes = null) : IRequest<Unit>;
