namespace MotorCare.App.Models.Vehicles;

public sealed class VehicleLookupResponse
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string PlateOriginal { get; set; } = string.Empty;
    public string PlateNormalized { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string VehicleDisplay { get; set; } = string.Empty;
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public int? CurrentKm { get; set; }
}
