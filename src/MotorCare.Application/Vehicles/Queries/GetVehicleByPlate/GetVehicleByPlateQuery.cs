using MediatR;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

public record VehicleDto(Guid Id, string PlateOriginal, string PlateNormalized, string Brand, string Model, int Year);

public record GetVehicleByPlateQuery(string Plate) : IRequest<VehicleDto?>;
