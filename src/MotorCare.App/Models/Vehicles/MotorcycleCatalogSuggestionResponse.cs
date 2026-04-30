namespace MotorCare.App.Models.Vehicles;

public sealed class MotorcycleCatalogSuggestionResponse
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Segment { get; set; }
    public int? EngineCc { get; set; }
}
