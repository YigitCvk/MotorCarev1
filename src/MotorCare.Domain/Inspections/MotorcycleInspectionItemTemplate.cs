using MotorCare.Domain.Enums;

namespace MotorCare.Domain.Inspections;

public sealed record MotorcycleInspectionItemTemplate(
    MotorcycleInspectionCategory Category,
    string Name,
    int SortOrder);
