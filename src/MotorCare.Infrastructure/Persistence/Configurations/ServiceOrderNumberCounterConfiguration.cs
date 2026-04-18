using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Infrastructure.Persistence.Entities;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class ServiceOrderNumberCounterConfiguration : IEntityTypeConfiguration<ServiceOrderNumberCounter>
{
    public void Configure(EntityTypeBuilder<ServiceOrderNumberCounter> builder)
    {
        builder.ToTable("ServiceOrderNumberCounters");

        builder.HasKey(c => new { c.TenantId, c.CounterDate });

        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CounterDate)
            .HasColumnType("date");

        builder.Property(c => c.LastValue)
            .IsRequired();
    }
}
