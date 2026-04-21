using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;

namespace MotorCare.Domain.Inspections.Entities;

public sealed class MotorcycleInspectionItem : AuditableEntity
{
    public MotorcycleInspectionCategory Category { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public MotorcycleInspectionResult Result { get; private set; }
    public string? Notes { get; private set; }
    public int SortOrder { get; private set; }

    private MotorcycleInspectionItem()
    {
    }

    internal MotorcycleInspectionItem(
        MotorcycleInspectionCategory category,
        string name,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Ekspertiz kalemi adı gereklidir.");
        }

        if (sortOrder < 0)
        {
            throw new DomainException("Sıralama değeri negatif olamaz.");
        }

        Id = Guid.NewGuid();
        Category = category;
        Name = name.Trim();
        Result = MotorcycleInspectionResult.NotChecked;
        SortOrder = sortOrder;
    }

    internal void Update(MotorcycleInspectionResult result, string? notes)
    {
        Result = result;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
