using MotorCare.Domain.Common;

namespace MotorCare.Domain.Users.Entities;

public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private RefreshToken()
    {
    }

    internal RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty) throw new DomainException("User ID is required.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainException("Refresh token hash is required.");

        Id = Guid.NewGuid();
        UserId = userId;
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
