using MotorCare.Domain.Common;

namespace MotorCare.Domain.Inventory;

public sealed class InventoryItem : AggregateRoot, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }
    public string? Category { get; private set; }
    public string? Brand { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public decimal StockQuantity { get; private set; }
    public decimal MinimumStockLevel { get; private set; }
    public bool IsActive { get; private set; }

    private InventoryItem()
    {
    }

    public InventoryItem(
        string tenantId,
        string name,
        string? sku,
        string? barcode,
        string? category,
        string? brand,
        string unit,
        decimal unitPrice,
        decimal stockQuantity,
        decimal minimumStockLevel,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new DomainException("Tenant ID gereklidir.");
        }

        Validate(name, unit, unitPrice, stockQuantity, minimumStockLevel);

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        Sku = NormalizeOptional(sku);
        Barcode = NormalizeOptional(barcode);
        Category = NormalizeOptional(category);
        Brand = NormalizeOptional(brand);
        Unit = unit.Trim();
        UnitPrice = unitPrice;
        StockQuantity = stockQuantity;
        MinimumStockLevel = minimumStockLevel;
        IsActive = isActive;
    }

    public void Update(
        string name,
        string? sku,
        string? barcode,
        string? category,
        string? brand,
        string unit,
        decimal unitPrice,
        decimal stockQuantity,
        decimal minimumStockLevel,
        bool isActive)
    {
        Validate(name, unit, unitPrice, stockQuantity, minimumStockLevel);

        Name = name.Trim();
        Sku = NormalizeOptional(sku);
        Barcode = NormalizeOptional(barcode);
        Category = NormalizeOptional(category);
        Brand = NormalizeOptional(brand);
        Unit = unit.Trim();
        UnitPrice = unitPrice;
        StockQuantity = stockQuantity;
        MinimumStockLevel = minimumStockLevel;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void AdjustStock(decimal quantityDelta, string? reason)
    {
        if (quantityDelta == 0)
        {
            throw new DomainException("Stok değişim miktarı sıfır olamaz.");
        }

        var projected = StockQuantity + quantityDelta;
        if (projected < 0)
        {
            throw new DomainException("Stok miktarı negatif olamaz.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Stok güncelleme nedeni gereklidir.");
        }

        StockQuantity = projected;
    }

    public bool IsLowStock => StockQuantity <= MinimumStockLevel;

    private static void Validate(
        string name,
        string unit,
        decimal unitPrice,
        decimal stockQuantity,
        decimal minimumStockLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Parça adı gereklidir.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new DomainException("Birim bilgisi gereklidir.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Birim fiyat negatif olamaz.");
        }

        if (stockQuantity < 0)
        {
            throw new DomainException("Stok miktarı negatif olamaz.");
        }

        if (minimumStockLevel < 0)
        {
            throw new DomainException("Minimum stok seviyesi negatif olamaz.");
        }
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
