using MediatR;

namespace MotorCare.Application.Vehicles.Queries.GetVehiclesByCustomerId;

public sealed record GetVehiclesByCustomerIdQuery(Guid CustomerId) : IRequest<IReadOnlyList<VehicleDto>>;
