using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.TenantId);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique();

        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("UserRefreshTokens");
            rt.HasKey(t => t.Id);
            rt.Property(t => t.TokenHash).IsRequired().HasMaxLength(200);
            rt.Property(t => t.ExpiresAt).IsRequired();
            rt.Property(t => t.RevokedAt);
            rt.WithOwner().HasForeignKey("UserId");
            rt.HasIndex(t => t.TokenHash).IsUnique();
        });

        builder.Navigation(u => u.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(u => u.RowVersion)
            .IsRowVersion();
    }
}
