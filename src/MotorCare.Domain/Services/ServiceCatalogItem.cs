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
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "TRY";
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
        decimal price,
        string currency = "TRY",
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new DomainException("Tenant ID gereklidir.");
        }

        Validate(name, defaultDurationMinutes, price, currency);

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        Category = category;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DefaultDurationMinutes = defaultDurationMinutes;
        DefaultPrice = price;
        Price = price;
        Currency = NormalizeCurrency(currency);
        IsActive = isActive;
    }

    public void Update(
        string name,
        ServiceCategory category,
        string? description,
        int defaultDurationMinutes,
        decimal price,
        string currency,
        bool isActive)
    {
        Validate(name, defaultDurationMinutes, price, currency);

        Name = name.Trim();
        Category = category;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DefaultDurationMinutes = defaultDurationMinutes;
        DefaultPrice = price;
        Price = price;
        Currency = NormalizeCurrency(currency);
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void Validate(string name, int defaultDurationMinutes, decimal price, string currency)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Hizmet adi gereklidir.");
        }

        if (defaultDurationMinutes <= 0)
        {
            throw new DomainException("Varsayilan sure sifirdan buyuk olmalidir.");
        }

        if (price < 0)
        {
            throw new DomainException("Fiyat negatif olamaz.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Para birimi gereklidir.");
        }

        if (currency.Trim().Length > 5)
        {
            throw new DomainException("Para birimi en fazla 5 karakter olabilir.");
        }
    }

    private static string NormalizeCurrency(string currency) => currency.Trim().ToUpperInvariant();
}
