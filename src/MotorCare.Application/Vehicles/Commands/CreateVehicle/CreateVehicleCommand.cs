using MediatR;

namespace MotorCare.Application.Vehicles.Commands.CreateVehicle;

public sealed record CreateVehicleCommand(
    string Plate,
    string Brand,
    string Model,
    int Year,
    string? ChassisNumber = null,
    string? Color = null,
    int? CurrentKm = null,
    Guid? CurrentCustomerId = null) : IRequest<Guid>;
