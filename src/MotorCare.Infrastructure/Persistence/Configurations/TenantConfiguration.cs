using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Tenants;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Identifier)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.LegalName)
            .HasMaxLength(200);

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.Phone)
            .HasMaxLength(30);

        builder.Property(t => t.Email)
            .HasMaxLength(150);

        builder.Property(t => t.Address)
            .HasMaxLength(500);

        builder.Property(t => t.TaxOffice)
            .HasMaxLength(120);

        builder.Property(t => t.TaxNumber)
            .HasMaxLength(30);

        builder.Property(t => t.Website)
            .HasMaxLength(250);

        builder.HasIndex(t => t.Identifier).IsUnique();

    }
}
