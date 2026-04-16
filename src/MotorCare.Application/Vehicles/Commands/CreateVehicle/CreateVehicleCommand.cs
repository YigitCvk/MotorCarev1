using MediatR;

namespace MotorCare.Application.Vehicles.Commands.CreateVehicle;

public record CreateVehicleCommand(string Plate, string Brand, string Model, int Year) : IRequest<Guid>;
