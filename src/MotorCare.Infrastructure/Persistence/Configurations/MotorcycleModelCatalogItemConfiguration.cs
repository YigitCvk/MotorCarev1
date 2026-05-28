using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Vehicles;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public sealed class MotorcycleModelCatalogItemConfiguration : IEntityTypeConfiguration<MotorcycleModelCatalogItem>
{
    public void Configure(EntityTypeBuilder<MotorcycleModelCatalogItem> builder)
    {
        builder.ToTable("MotorcycleModelCatalogItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Brand)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.BrandNormalized)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.ModelNormalized)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.ModelFamily).HasMaxLength(80);
        builder.Property(x => x.Segment).HasMaxLength(60);
        builder.Property(x => x.OriginCountry).HasMaxLength(60);
        builder.Property(x => x.OriginRegion).HasMaxLength(40);

        builder.HasIndex(x => x.Brand);
        builder.HasIndex(x => new { x.Brand, x.Model });
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Segment);
        builder.HasIndex(x => x.OriginCountry);
        builder.HasIndex(x => new { x.BrandNormalized, x.ModelNormalized }).IsUnique();
    }
}
