using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Customers;
using MotorCare.Domain.ValueObjects;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        var phoneComparer = new ValueComparer<PhoneNumber?>(
            (left, right) =>
                left == null && right == null ||
                left != null && right != null && left.Value == right.Value,
            phone => phone == null ? 0 : phone.Value.GetHashCode(),
            phone => phone == null ? null : PhoneNumber.Create(phone.Value));

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

        builder.Property(c => c.Phone)
            .HasConversion(
                phone => phone == null ? null : phone.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : PhoneNumber.Create(value))
            .HasColumnName("Phone")
            .HasMaxLength(20)
            .Metadata.SetValueComparer(phoneComparer);

        builder.Property(c => c.Whatsapp)
            .HasConversion(
                phone => phone == null ? null : phone.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : PhoneNumber.Create(value))
            .HasColumnName("Whatsapp")
            .HasMaxLength(20)
            .Metadata.SetValueComparer(phoneComparer);

        // Store the normalized phone value directly in the Customers table
        // so EF can create a normal unique index without owned-type shadow properties.
        builder.HasIndex("TenantId", "Phone")
            .IsUnique()
            .HasFilter("\"Phone\" IS NOT NULL");

    }
}
