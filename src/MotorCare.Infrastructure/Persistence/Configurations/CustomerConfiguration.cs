using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Customers;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.TenantId);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Email)
            .HasMaxLength(150);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        // Value Objects mapped consistently with OwnsOne
        builder.OwnsOne(c => c.Phone, p =>
        {
            p.Property(pp => pp.Value).HasColumnName("Phone").HasMaxLength(20);
        });

        builder.OwnsOne(c => c.Whatsapp, p =>
        {
            p.Property(pp => pp.Value).HasColumnName("Whatsapp").HasMaxLength(20);
        });

        // Unique index: TenantId + Phone (normalized) — prevents duplicate phone per tenant  
        builder.HasIndex("TenantId", "Phone_Value")
            .IsUnique()
            .HasFilter("\"Phone_Value\" IS NOT NULL");

        // Concurrency token
        builder.Property(c => c.RowVersion)
            .IsRowVersion();
    }
}
