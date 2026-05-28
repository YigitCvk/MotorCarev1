using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.AddConsumableToOrder;

public sealed record AddConsumableToOrderCommand(
    Guid Id,
    string Category,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string? Brand = null,
    string? SubCategory = null,
    string? Specification = null,
    string? Notes = null) : IRequest<Unit>;
