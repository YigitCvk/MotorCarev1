using MotorCare.Domain.Common;

namespace MotorCare.Domain.ServiceOrders.Entities;

public class ServiceOperationItem : AuditableEntity
{
    public string Description { get; private set; } = string.Empty;
    public Guid? ServiceCatalogItemId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public string? Notes { get; private set; }
    public decimal Price { get; private set; }
    public decimal LineTotal => Price;

    private ServiceOperationItem() { }

    internal ServiceOperationItem(
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discount = 0m,
        string? notes = null,
        Guid? serviceCatalogItemId = null)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");
        ValidatePricing(quantity, unitPrice, discount);

        Id = Guid.NewGuid();
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discount;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        ServiceCatalogItemId = serviceCatalogItemId;
        Price = CalculateLineTotal(quantity, unitPrice, discount);
    }

    internal void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new DomainException("Price cannot be negative.");
        Quantity = 1m;
        UnitPrice = newPrice;
        Discount = 0m;
        Price = newPrice;
    }

    private static void ValidatePricing(decimal quantity, decimal unitPrice, decimal discount)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");
        if (discount < 0) throw new DomainException("Discount cannot be negative.");
        if (discount > quantity * unitPrice) throw new DomainException("Discount cannot exceed line total.");
    }

    private static decimal CalculateLineTotal(decimal quantity, decimal unitPrice, decimal discount)
        => (quantity * unitPrice) - discount;
}
