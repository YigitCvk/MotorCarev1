using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Vehicles;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.TenantId)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(v => v.TenantId);

        builder.Property(v => v.Brand).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Model).IsRequired().HasMaxLength(100);
        builder.Property(v => v.ChassisNumber).HasMaxLength(100);
        builder.Property(v => v.Color).HasMaxLength(50);

        builder.ComplexProperty(v => v.Plate, p =>
        {
            p.Property(pp => pp.OriginalValue).HasColumnName("PlateOriginal").HasMaxLength(20).IsRequired();
            p.Property(pp => pp.NormalizedValue).HasColumnName("PlateNormalized").HasMaxLength(20).IsRequired();
        });

        builder.HasIndex(v => new { v.TenantId, v.Plate.NormalizedValue }).IsUnique();

        builder.OwnsMany(v => v.Notes, nb =>
        {
            nb.ToTable("VehicleNotes");
            nb.HasKey(n => n.Id);
            nb.Property(n => n.Content).IsRequired().HasMaxLength(1000);
            nb.WithOwner().HasForeignKey("VehicleId");
        });

        builder.OwnsMany(v => v.Photos, pb =>
        {
            pb.ToTable("VehiclePhotos");
            pb.HasKey(p => p.Id);
            pb.Property(p => p.PhotoUrl).IsRequired().HasMaxLength(500);
            pb.Property(p => p.Description).HasMaxLength(500);
            pb.WithOwner().HasForeignKey("VehicleId");
        });

        builder.Navigation(v => v.Notes).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(v => v.Photos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
