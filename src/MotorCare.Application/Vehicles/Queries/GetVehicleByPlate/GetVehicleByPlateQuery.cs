using MediatR;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

public sealed record VehicleDto(Guid Id, string PlateOriginal, string PlateNormalized, string Brand, string Model, int Year);

public sealed record GetVehicleByPlateQuery(string Plate) : IRequest<VehicleDto?>;
