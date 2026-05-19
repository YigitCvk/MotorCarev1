using MotorCare.Domain.Common;

namespace MotorCare.Domain.ServiceOrders.Entities;

public class ServicePartItem : AuditableEntity
{
    public string PartName { get; private set; } = string.Empty;
    public string? PartNumber { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Discount { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalPrice => (UnitPrice * Quantity) - Discount;
    public decimal LineTotal => TotalPrice;
    public Guid? InventoryItemId { get; private set; }

    private ServicePartItem() { }

    internal ServicePartItem(
        string partName,
        string? partNumber,
        decimal unitPrice,
        int quantity,
        Guid? inventoryItemId = null,
        decimal discount = 0m,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(partName)) throw new DomainException("Part name is required.");
        ValidatePricing(unitPrice, quantity, discount);

        Id = Guid.NewGuid();
        PartName = partName.Trim();
        PartNumber = string.IsNullOrWhiteSpace(partNumber) ? null : partNumber.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        InventoryItemId = inventoryItemId;
        Discount = discount;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    internal void Update(decimal unitPrice, int quantity, decimal discount = 0m, string? notes = null)
    {
        ValidatePricing(unitPrice, quantity, discount);
        
        UnitPrice = unitPrice;
        Quantity = quantity;
        Discount = discount;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    private static void ValidatePricing(decimal unitPrice, int quantity, decimal discount)
    {
        if (unitPrice <= 0) throw new DomainException("Unit price must be greater than zero.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");
        if (discount < 0) throw new DomainException("Discount cannot be negative.");
        if (discount > unitPrice * quantity) throw new DomainException("Discount cannot exceed line total.");
    }
}
