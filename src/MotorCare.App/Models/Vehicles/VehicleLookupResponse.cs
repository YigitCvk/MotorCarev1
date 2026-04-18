namespace MotorCare.App.Models.Vehicles;

public sealed class VehicleLookupResponse
{
    public Guid Id { get; set; }
    public string PlateOriginal { get; set; } = string.Empty;
    public string PlateNormalized { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
}
