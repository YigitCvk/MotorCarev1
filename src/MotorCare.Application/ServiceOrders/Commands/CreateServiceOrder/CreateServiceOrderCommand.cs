using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.CreateServiceOrder;

public sealed record CreateServiceOrderCommand(
    Guid VehicleId,
    Guid CustomerId,
    int VehicleKm,
    string? Complaint,
    IReadOnlyList<CreateServiceOrderConsumableItem>? Consumables = null) : IRequest<Guid>;

public sealed record CreateServiceOrderConsumableItem(
    string Category,
    string ProductName,
    decimal UnitPrice = 0m,
    int Quantity = 1,
    string? Brand = null,
    string? SubCategory = null,
    string? Specification = null,
    string? Notes = null);
