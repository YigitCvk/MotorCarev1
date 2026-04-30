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
    public bool IsEmailVerified { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public TwoFactorProvider? TwoFactorProvider { get; private set; }
    public string? TotpSecretEncrypted { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;
    private readonly List<UserSecurityToken> _securityTokens = new();
    public IReadOnlyCollection<UserSecurityToken> SecurityTokens => _securityTokens;

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
        IsEmailVerified = false;
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

    public void MarkEmailVerified()
    {
        IsEmailVerified = true;
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("Password hash is required.");
        PasswordHash = passwordHash;
    }

    public void SetTwoFactor(bool enabled, TwoFactorProvider? provider, string? totpSecretEncrypted = null)
    {
        if (enabled && provider is null)
        {
            throw new DomainException("Two-factor provider is required when 2FA is enabled.");
        }

        TwoFactorEnabled = enabled;
        TwoFactorProvider = enabled ? provider : null;
        TotpSecretEncrypted = enabled ? totpSecretEncrypted : null;
    }

    public UserSecurityToken AddSecurityToken(
        UserSecurityTokenPurpose purpose,
        string tokenHash,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt)
    {
        var token = new UserSecurityToken(Id, purpose, tokenHash, expiresAt, createdAt);
        _securityTokens.Add(token);
        return token;
    }

    public void RevokeSecurityToken(string tokenHash, DateTimeOffset revokedAt)
    {
        var token = _securityTokens.FirstOrDefault(t => t.TokenHash == tokenHash)
            ?? throw new DomainException("Security token was not found.");

        token.Revoke(revokedAt);
    }

    public void RevokeSecurityTokens(UserSecurityTokenPurpose purpose, DateTimeOffset revokedAt)
    {
        foreach (var token in _securityTokens.Where(t => t.Purpose == purpose && t.RevokedAt is null && t.ConsumedAt is null))
        {
            token.Revoke(revokedAt);
        }
    }

    public void ConsumeSecurityToken(string tokenHash, DateTimeOffset consumedAt)
    {
        var token = _securityTokens.FirstOrDefault(t => t.TokenHash == tokenHash)
            ?? throw new DomainException("Security token was not found.");

        token.Consume(consumedAt);
    }

    public void RegisterSecurityTokenFailedAttempt(string tokenHash, DateTimeOffset attemptedAt, int maxAttempts = 5)
    {
        var token = _securityTokens.FirstOrDefault(t => t.TokenHash == tokenHash)
            ?? throw new DomainException("Security token was not found.");

        token.RegisterFailedAttempt(attemptedAt, maxAttempts);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
