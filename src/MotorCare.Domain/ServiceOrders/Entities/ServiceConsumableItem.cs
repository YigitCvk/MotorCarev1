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

    private ServiceConsumableItem() { }

    internal ServiceConsumableItem(
        string category,
        string productName,
        string? brand = null,
        string? subCategory = null,
        string? specification = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new DomainException("Consumable category is required.");
        if (string.IsNullOrWhiteSpace(productName)) throw new DomainException("Consumable product name is required.");

        Id = Guid.NewGuid();
        Category = category.Trim();
        ProductName = productName.Trim();
        Brand = string.IsNullOrWhiteSpace(brand) ? string.Empty : brand.Trim();
        SubCategory = string.IsNullOrWhiteSpace(subCategory) ? null : subCategory.Trim();
        Specification = string.IsNullOrWhiteSpace(specification) ? null : specification.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
