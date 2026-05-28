namespace MotorCare.Application.Vehicles;

public sealed record MotorcycleCatalogSuggestionDto(
    Guid Id,
    string Brand,
    string Model,
    string DisplayName,
    string? Segment,
    int? EngineCc);
