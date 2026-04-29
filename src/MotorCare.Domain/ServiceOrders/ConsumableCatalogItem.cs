using MotorCare.Domain.Common;

namespace MotorCare.Domain.ServiceOrders;

public sealed class ConsumableCatalogItem : AuditableEntity
{
    public string? TenantId { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string? SubCategory { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string? Specification { get; private set; }
    public string? Notes { get; private set; }
    
    public bool IsSystemDefault { get; private set; }
    public bool IsActive { get; private set; }
    public int UsageCount { get; private set; }

    private ConsumableCatalogItem()
    {
    }

    public ConsumableCatalogItem(
        string? tenantId,
        string category,
        string brand,
        string productName,
        string? subCategory = null,
        string? specification = null,
        string? notes = null,
        bool isSystemDefault = false)
    {
        if (!isSystemDefault && string.IsNullOrWhiteSpace(tenantId))
        {
            throw new DomainException("TenantId is required for custom catalog items.");
        }

        Id = Guid.NewGuid();
        TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
        Category = category.Trim();
        Brand = brand.Trim();
        ProductName = productName.Trim();
        SubCategory = string.IsNullOrWhiteSpace(subCategory) ? null : subCategory.Trim();
        Specification = string.IsNullOrWhiteSpace(specification) ? null : specification.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsSystemDefault = isSystemDefault;
        IsActive = true;
        UsageCount = 0;
    }

    public void IncrementUsage()
    {
        UsageCount++;
    }
}
