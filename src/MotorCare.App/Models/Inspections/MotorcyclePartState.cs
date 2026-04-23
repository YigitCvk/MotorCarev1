namespace MotorCare.App.Models.Inspections;

public sealed class MotorcyclePartState
{
    public string PartId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public MotorcycleViewType ViewType { get; set; }
    public MotorcyclePartStatus Status { get; set; } = MotorcyclePartStatus.Unmarked;
    public string? Notes { get; set; }
}
