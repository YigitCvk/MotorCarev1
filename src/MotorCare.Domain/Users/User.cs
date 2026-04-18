using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Domain.Users;

public class User : AggregateRoot, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    private User()
    {
    }

    public User(string tenantId, string fullName, string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("Tenant ID is required.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        FullName = fullName.Trim();
        Email = NormalizeEmail(email);
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void MarkLogin(DateTimeOffset loggedInAt)
    {
        LastLoginAt = loggedInAt;
    }

    public RefreshToken AddRefreshToken(string tokenHash, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        var token = new RefreshToken(Id, tokenHash, expiresAt, createdAt);
        _refreshTokens.Add(token);
        return token;
    }

    public void RevokeRefreshToken(string tokenHash, DateTimeOffset revokedAt)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash)
            ?? throw new DomainException("Refresh token was not found.");

        token.Revoke(revokedAt);
    }

    public bool HasActiveRefreshToken(string tokenHash, DateTimeOffset now)
    {
        return _refreshTokens.Any(t => t.TokenHash == tokenHash && t.IsActive(now));
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
