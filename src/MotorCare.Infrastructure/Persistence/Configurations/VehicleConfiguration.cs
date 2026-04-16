using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Entities;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Brand).HasMaxLength(100);
        builder.Property(v => v.Model).HasMaxLength(100);

        builder.OwnsOne(v => v.Plate, p =>
        {
            p.Property(pp => pp.OriginalValue).HasColumnName("PlateOriginal").HasMaxLength(20).IsRequired();
            p.Property(pp => pp.NormalizedValue).HasColumnName("PlateNormalized").HasMaxLength(20).IsRequired();
        });
        
        // Enforce unique normalized plate per tenant
        builder.HasIndex(v => new { v.TenantId, v.Plate.NormalizedValue }).IsUnique();
    }
}
