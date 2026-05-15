using MotorCare.Domain.Common;

namespace MotorCare.Domain.ServiceOrders.Entities;

public class ServicePartItem : AuditableEntity
{
    public string PartName { get; private set; } = string.Empty;
    public string? PartNumber { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public Guid? InventoryItemId { get; private set; }

    private ServicePartItem() { }

    internal ServicePartItem(string partName, string? partNumber, decimal unitPrice, int quantity, Guid? inventoryItemId = null)
    {
        if (string.IsNullOrWhiteSpace(partName)) throw new DomainException("Part name is required.");
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");

        Id = Guid.NewGuid();
        PartName = partName;
        PartNumber = partNumber;
        UnitPrice = unitPrice;
        Quantity = quantity;
        InventoryItemId = inventoryItemId;
    }

    internal void Update(decimal unitPrice, int quantity)
    {
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");
        
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
