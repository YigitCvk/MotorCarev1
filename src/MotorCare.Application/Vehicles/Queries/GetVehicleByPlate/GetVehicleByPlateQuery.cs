using MediatR;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

public sealed record GetVehicleByPlateQuery(string Plate) : IRequest<VehicleDto?>;
