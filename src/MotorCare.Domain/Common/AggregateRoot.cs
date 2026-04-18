namespace MotorCare.Domain.Common;

public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Concurrency token for optimistic concurrency control.
    /// EF Core maps this as a row version / xmin column.
    /// </summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
