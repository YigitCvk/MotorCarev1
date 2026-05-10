using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Imports;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.ImportType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(b => b.FileName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(b => b.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(b => b.TenantId);
        builder.HasIndex(b => new { b.TenantId, b.CreatedAtUtc });
        builder.HasIndex(b => b.Status);

        builder.HasMany(b => b.Rows)
            .WithOne()
            .HasForeignKey(r => r.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
