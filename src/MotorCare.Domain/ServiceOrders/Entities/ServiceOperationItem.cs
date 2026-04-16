using MotorCare.Domain.Common;

namespace MotorCare.Domain.ServiceOrders.Entities;

public class ServiceOperationItem : AuditableEntity
{
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }

    private ServiceOperationItem() { }

    internal ServiceOperationItem(string description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");
        if (price < 0) throw new DomainException("Price cannot be negative.");

        Id = Guid.NewGuid();
        Description = description;
        Price = price;
    }

    internal void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new DomainException("Price cannot be negative.");
        Price = newPrice;
    }
}
