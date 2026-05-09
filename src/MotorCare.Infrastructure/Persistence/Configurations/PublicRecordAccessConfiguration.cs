using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.PublicRecords;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public sealed class PublicRecordAccessConfiguration : IEntityTypeConfiguration<PublicRecordAccess>
{
    public void Configure(EntityTypeBuilder<PublicRecordAccess> builder)
    {
        builder.ToTable("PublicRecordAccesses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RecordType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.AccessCount)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.RecordType, x.RecordId })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.RecordType });
    }
}
