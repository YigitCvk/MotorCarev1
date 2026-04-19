namespace MotorCare.App.Models.Vehicles;

public sealed class CreateVehicleRequest
{
    public string Plate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; } = DateTime.Now.Year;
    public string? ChassisNumber { get; set; }
    public string? Color { get; set; }
    public int? CurrentKm { get; set; }
    public Guid? CurrentCustomerId { get; set; }
}
