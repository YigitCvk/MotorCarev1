using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.CreateServiceOrder;

public sealed record CreateServiceOrderCommand(
    Guid VehicleId,
    Guid CustomerId,
    int VehicleKm,
    string? Complaint) : IRequest<Guid>;
