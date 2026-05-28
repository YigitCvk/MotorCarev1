using MotorCare.Domain.Common;

namespace MotorCare.Domain.ServiceOrders.Entities;

public class ServiceConsumableItem : AuditableEntity
{
    public string Category { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string? SubCategory { get; private set; }
    public string? Specification { get; private set; }
    public string? Notes { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    private ServiceConsumableItem() { }

    internal ServiceConsumableItem(
        string category,
        string productName,
        decimal unitPrice,
        int quantity,
        string? brand = null,
        string? subCategory = null,
        string? specification = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new DomainException("Consumable category is required.");
        if (string.IsNullOrWhiteSpace(productName)) throw new DomainException("Consumable product name is required.");
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");

        Id = Guid.NewGuid();
        Category = category.Trim();
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        Brand = string.IsNullOrWhiteSpace(brand) ? string.Empty : brand.Trim();
        SubCategory = string.IsNullOrWhiteSpace(subCategory) ? null : subCategory.Trim();
        Specification = string.IsNullOrWhiteSpace(specification) ? null : specification.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
