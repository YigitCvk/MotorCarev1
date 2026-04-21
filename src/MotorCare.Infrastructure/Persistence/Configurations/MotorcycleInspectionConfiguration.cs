using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Inspections;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public sealed class MotorcycleInspectionConfiguration : IEntityTypeConfiguration<MotorcycleInspection>
{
    public void Configure(EntityTypeBuilder<MotorcycleInspection> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.TenantId);

        builder.Property(x => x.InspectionNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.TenantId, x.InspectionNo })
            .IsUnique();

        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Plate)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Brand).HasMaxLength(100);
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.ChassisNumber).HasMaxLength(100);
        builder.Property(x => x.EngineNumber).HasMaxLength(100);
        builder.Property(x => x.Query5664).HasMaxLength(250);
        builder.Property(x => x.MileageQuery).HasMaxLength(250);
        builder.Property(x => x.GeneralNotes).HasMaxLength(4000);
        builder.Property(x => x.TestRideNotes).HasMaxLength(4000);
        builder.Property(x => x.CosmeticNotes).HasMaxLength(4000);

        builder.Property(x => x.PackageType)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.PackagePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.OwnsMany(x => x.Items, ib =>
        {
            ib.ToTable("MotorcycleInspectionItems");
            ib.HasKey(i => i.Id);
            ib.WithOwner().HasForeignKey("MotorcycleInspectionId");

            ib.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(200);

            ib.Property(i => i.Category)
                .HasConversion<string>()
                .HasMaxLength(40);

            ib.Property(i => i.Result)
                .HasConversion<string>()
                .HasMaxLength(30);

            ib.Property(i => i.Notes)
                .HasMaxLength(2000);
        });

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
