using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.AddOperationToOrder;

public sealed record AddOperationToOrderCommand(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Discount = 0m,
    string? Notes = null,
    Guid? ServiceCatalogItemId = null) : IRequest<Unit>;
