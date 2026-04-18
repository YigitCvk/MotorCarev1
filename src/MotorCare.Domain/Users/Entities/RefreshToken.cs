using MotorCare.Domain.Common;

namespace MotorCare.Domain.Users.Entities;

public class RefreshToken : AuditableEntity
{
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private RefreshToken()
    {
    }

    internal RefreshToken(string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainException("Refresh token hash is required.");

        Id = Guid.NewGuid();
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    internal void Revoke(DateTimeOffset revokedAt)
    {
        RevokedAt = revokedAt;
    }

    public bool IsActive(DateTimeOffset now)
    {
        return RevokedAt is null && ExpiresAt > now;
    }
}
