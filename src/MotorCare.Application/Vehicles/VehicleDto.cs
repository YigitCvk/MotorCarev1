namespace MotorCare.Application.Vehicles;

public sealed record VehicleDto(
    Guid Id,
    Guid? CustomerId,
    string? CustomerName,
    string PlateOriginal,
    string PlateNormalized,
    string Brand,
    string Model,
    int Year,
    string VehicleDisplay);
