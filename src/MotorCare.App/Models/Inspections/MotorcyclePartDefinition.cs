namespace MotorCare.App.Models.Inspections;

public sealed record MotorcyclePartDefinition(
    string PartId,
    int Number,
    string Label,
    MotorcycleViewType ViewType,
    double HotspotLeftPercent,
    double HotspotTopPercent,
    IReadOnlyList<string> MatchKeys);
