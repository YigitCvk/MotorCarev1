using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class ServiceOrderAttachmentConfiguration : IEntityTypeConfiguration<ServiceOrderAttachment>
{
    public void Configure(EntityTypeBuilder<ServiceOrderAttachment> builder)
    {
        builder.ToTable("ServiceOrderAttachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AttachmentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.UploadedByUserName)
            .HasMaxLength(256);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.ServiceOrderId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasOne<ServiceOrder>()
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.ServiceOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
