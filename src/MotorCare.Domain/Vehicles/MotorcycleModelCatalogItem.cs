using MotorCare.Domain.Common;

namespace MotorCare.Domain.Vehicles;

public sealed class MotorcycleModelCatalogItem : AuditableEntity
{
    public string Brand { get; private set; } = string.Empty;
    public string BrandNormalized { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string ModelNormalized { get; private set; } = string.Empty;
    public string? ModelFamily { get; private set; }
    public string? Segment { get; private set; }
    public int? EngineCc { get; private set; }
    public string? OriginCountry { get; private set; }
    public string? OriginRegion { get; private set; }
    public bool IsSystemDefault { get; private set; }
    public bool IsActive { get; private set; }
    public int UsageCount { get; private set; }

    private MotorcycleModelCatalogItem()
    {
    }

    public MotorcycleModelCatalogItem(
        string brand,
        string model,
        string? modelFamily,
        string? segment,
        int? engineCc,
        string? originCountry,
        string? originRegion,
        bool isSystemDefault = true,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(brand)) throw new DomainException("Brand is required.");
        if (string.IsNullOrWhiteSpace(model)) throw new DomainException("Model is required.");
        if (engineCc.HasValue && engineCc.Value <= 0) throw new DomainException("EngineCc must be positive.");

        Id = Guid.NewGuid();
        Brand = brand.Trim();
        BrandNormalized = Normalize(brand);
        Model = model.Trim();
        ModelNormalized = Normalize(model);
        ModelFamily = NormalizeOptional(modelFamily);
        Segment = NormalizeOptional(segment);
        EngineCc = engineCc;
        OriginCountry = NormalizeOptional(originCountry);
        OriginRegion = NormalizeOptional(originRegion);
        IsSystemDefault = isSystemDefault;
        IsActive = isActive;
        UsageCount = 0;
    }

    public void RegisterUsage()
    {
        UsageCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string Normalize(string value)
        => value.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
