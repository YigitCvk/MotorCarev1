using MotorCare.Domain.Common;

namespace MotorCare.Domain.Vehicles.Entities;

public class VehicleNote : AuditableEntity
{
    public string Content { get; private set; }

    private VehicleNote() { }

    internal VehicleNote(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Note content cannot be empty.");
        
        Id = Guid.NewGuid();
        Content = content;
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Note content cannot be empty.");
        Content = content;
    }
}
