using MotorCare.Domain.Common;

namespace MotorCare.Domain.Vehicles.Entities;

public class VehicleNote : AuditableEntity
{
    public string Content { get; private set; } = string.Empty;

    private VehicleNote() { }

    internal VehicleNote(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new DomainException("Note content cannot be empty.");
        
        Id = Guid.NewGuid();
        Content = content;
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new DomainException("Note content cannot be empty.");
        Content = content;
    }
}
