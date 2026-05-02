using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class ServiceOrderStatusHistoryConfiguration : IEntityTypeConfiguration<ServiceOrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<ServiceOrderStatusHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.ToStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.ChangedByUserName)
            .HasMaxLength(256);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.ServiceOrderId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne<ServiceOrder>()
            .WithMany()
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
