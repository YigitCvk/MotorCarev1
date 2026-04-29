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
    string? Brand,
    string? SubCategory,
    string? Specification,
    string? Notes);
