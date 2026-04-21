using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Inventory;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(x => x.Sku)
            .HasMaxLength(80);

        builder.Property(x => x.Barcode)
            .HasMaxLength(120);

        builder.Property(x => x.Category)
            .HasMaxLength(100);

        builder.Property(x => x.Brand)
            .HasMaxLength(100);

        builder.Property(x => x.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.StockQuantity)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.MinimumStockLevel)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique().HasFilter("\"Sku\" IS NOT NULL");
        builder.HasIndex(x => new { x.TenantId, x.Barcode }).IsUnique().HasFilter("\"Barcode\" IS NOT NULL");
    }
}
