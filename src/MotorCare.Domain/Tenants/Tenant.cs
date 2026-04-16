using MotorCare.Domain.Common;

namespace MotorCare.Domain.Tenants;

public class Tenant : AggregateRoot
{
    public string Identifier { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    private Tenant() { } // For EF Core

    public Tenant(string identifier, string name)
    {
        if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException("Identifier is required");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");

        Id = Guid.NewGuid();
        Identifier = identifier;
        Name = name;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
