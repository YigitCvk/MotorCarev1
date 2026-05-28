using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Imports;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class ImportBatchRowConfiguration : IEntityTypeConfiguration<ImportBatchRow>
{
    public void Configure(EntityTypeBuilder<ImportBatchRow> builder)
    {
        builder.ToTable("ImportBatchRows");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ImportBatchId)
            .IsRequired();

        builder.Property(r => r.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.RawJson)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(r => r.WarningMessage)
            .HasMaxLength(1000);

        builder.HasIndex(r => r.ImportBatchId);
        builder.HasIndex(r => new { r.ImportBatchId, r.RowNumber });
        builder.HasIndex(r => r.Status);
    }
}
