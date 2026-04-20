using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;

namespace MotorCare.Domain.Services;

public class ServiceCatalogItem : AggregateRoot, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ServiceCategory Category { get; private set; }
    public string? Description { get; private set; }
    public int DefaultDurationMinutes { get; private set; }
    public decimal DefaultPrice { get; private set; }
    public bool IsActive { get; private set; }

    private ServiceCatalogItem()
    {
    }

    public ServiceCatalogItem(
        string tenantId,
        string name,
        ServiceCategory category,
        string? description,
        int defaultDurationMinutes,
        decimal defaultPrice,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new DomainException("Tenant ID gereklidir.");
        }

        Validate(name, defaultDurationMinutes, defaultPrice);

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        Category = category;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DefaultDurationMinutes = defaultDurationMinutes;
        DefaultPrice = defaultPrice;
        IsActive = isActive;
    }

    public void Update(
        string name,
        ServiceCategory category,
        string? description,
        int defaultDurationMinutes,
        decimal defaultPrice,
        bool isActive)
    {
        Validate(name, defaultDurationMinutes, defaultPrice);

        Name = name.Trim();
        Category = category;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DefaultDurationMinutes = defaultDurationMinutes;
        DefaultPrice = defaultPrice;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void Validate(string name, int defaultDurationMinutes, decimal defaultPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Hizmet adı gereklidir.");
        }

        if (defaultDurationMinutes <= 0)
        {
            throw new DomainException("Varsayılan süre sıfırdan büyük olmalıdır.");
        }

        if (defaultPrice < 0)
        {
            throw new DomainException("Varsayılan fiyat negatif olamaz.");
        }
    }
}
