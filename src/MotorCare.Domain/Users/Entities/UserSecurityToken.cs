using MotorCare.Domain.Common;

namespace MotorCare.Domain.Users.Entities;

public class UserSecurityToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public UserSecurityTokenPurpose Purpose { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public int FailedAttemptCount { get; private set; }

    private UserSecurityToken()
    {
    }

    internal UserSecurityToken(Guid userId, UserSecurityTokenPurpose purpose, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        if (userId == Guid.Empty) throw new DomainException("User ID is required.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainException("Security token hash is required.");

        Id = Guid.NewGuid();
        UserId = userId;
        Purpose = purpose;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    internal void Revoke(DateTimeOffset revokedAt)
    {
        RevokedAt = revokedAt;
    }

    internal void Consume(DateTimeOffset consumedAt)
    {
        ConsumedAt = consumedAt;
    }

    internal void RegisterFailedAttempt(DateTimeOffset attemptedAt, int maxAttempts = 5)
    {
        FailedAttemptCount++;

        if (FailedAttemptCount >= maxAttempts)
        {
            RevokedAt = attemptedAt;
        }
    }

    public bool IsActive(DateTimeOffset now)
    {
        return RevokedAt is null && ConsumedAt is null && ExpiresAt > now;
    }
}
