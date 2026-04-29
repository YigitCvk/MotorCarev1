using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.TenantId);

        builder.Property(o => o.OrderNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => new { o.TenantId, o.OrderNo }).IsUnique();

        builder.Property(o => o.Complaint).HasMaxLength(1000);
        builder.Property(o => o.WorkDescription).HasMaxLength(2000);
        builder.Property(o => o.InternalNote).HasMaxLength(1000);

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);

        // Map Decimals with precision
        builder.Property(o => o.LaborTotal).HasPrecision(18, 2);
        builder.Property(o => o.PartsTotal).HasPrecision(18, 2);
        builder.Property(o => o.DiscountTotal).HasPrecision(18, 2);
        builder.Property(o => o.GrandTotal).HasPrecision(18, 2);
        builder.Property(o => o.PaidTotal).HasPrecision(18, 2);

        // Computed property — do not map to database
        builder.Ignore(o => o.RemainingTotal);

        // Child Collections
        builder.OwnsMany(o => o.Operations, ob =>
        {
            ob.ToTable("ServiceOperationItems");
            ob.HasKey(oi => oi.Id);
            ob.Property(oi => oi.Description).IsRequired().HasMaxLength(500);
            ob.Property(oi => oi.Price).HasPrecision(18, 2);
            ob.WithOwner().HasForeignKey("ServiceOrderId");
        });

        builder.OwnsMany(o => o.Parts, pb =>
        {
            pb.ToTable("ServicePartItems");
            pb.HasKey(pi => pi.Id);
            pb.Property(pi => pi.PartName).IsRequired().HasMaxLength(200);
            pb.Property(pi => pi.PartNumber).HasMaxLength(100);
            pb.Property(pi => pi.UnitPrice).HasPrecision(18, 2);
            // TotalPrice is a computed property — do not map to database
            pb.Ignore(pi => pi.TotalPrice);
            pb.WithOwner().HasForeignKey("ServiceOrderId");
        });

        builder.OwnsMany(o => o.Consumables, cb =>
        {
            cb.ToTable("ServiceConsumableItems");
            cb.HasKey(ci => ci.Id);
            cb.Property(ci => ci.Category).IsRequired().HasMaxLength(64);
            cb.Property(ci => ci.Brand).HasMaxLength(80);
            cb.Property(ci => ci.ProductName).IsRequired().HasMaxLength(160);
            cb.Property(ci => ci.SubCategory).HasMaxLength(100);
            cb.Property(ci => ci.Specification).HasMaxLength(160);
            cb.Property(ci => ci.Notes).HasMaxLength(250);
            cb.WithOwner().HasForeignKey("ServiceOrderId");
        });

        builder.OwnsMany(o => o.Payments, payb =>
        {
            payb.ToTable("ServicePayments");
            payb.HasKey(p => p.Id);
            payb.Property(p => p.Amount).HasPrecision(18, 2);
            payb.Property(p => p.Method).HasConversion<string>().HasMaxLength(30);
            payb.WithOwner().HasForeignKey("ServiceOrderId");
        });

        builder.Navigation(o => o.Operations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.Parts).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.Consumables).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}
