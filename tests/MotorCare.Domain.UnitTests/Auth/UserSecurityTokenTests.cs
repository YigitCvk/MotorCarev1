using MotorCare.Domain.Enums;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Domain.UnitTests.Auth;

public class UserSecurityTokenTests
{
    private static (User user, UserSecurityToken token) CreateUserWithToken(DateTimeOffset? expiresAt = null)
    {
        var user = new User("test-tenant", "Test User", "test@example.com", "hash", UserRole.Owner);
        var now = DateTimeOffset.UtcNow;
        var expires = expiresAt ?? now.AddMinutes(10);
        var token = user.AddSecurityToken(UserSecurityTokenPurpose.EmailVerification, "hash123", expires, now);
        return (user, token);
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenExpired()
    {
        var (_, token) = CreateUserWithToken(expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        Assert.False(token.IsActive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenRevoked()
    {
        var (_, token) = CreateUserWithToken();
        token.Revoke(DateTimeOffset.UtcNow);
        Assert.False(token.IsActive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenConsumed()
    {
        var (_, token) = CreateUserWithToken();
        token.Consume(DateTimeOffset.UtcNow);
        Assert.False(token.IsActive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsActive_ReturnsTrue_WhenFreshAndUnused()
    {
        var (_, token) = CreateUserWithToken();
        Assert.True(token.IsActive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void RegisterFailedAttempt_IncrementsCount()
    {
        var (_, token) = CreateUserWithToken();
        token.RegisterFailedAttempt(DateTimeOffset.UtcNow, maxAttempts: 5);
        Assert.Equal(1, token.FailedAttemptCount);
        Assert.Null(token.RevokedAt);
    }

    [Fact]
    public void RegisterFailedAttempt_RevokesToken_WhenMaxAttemptsReached()
    {
        var (_, token) = CreateUserWithToken();
        var now = DateTimeOffset.UtcNow;

        for (var i = 0; i < 5; i++)
            token.RegisterFailedAttempt(now, maxAttempts: 5);

        Assert.Equal(5, token.FailedAttemptCount);
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public void RegisterFailedAttempt_DoesNotRevoke_BeforeMaxAttempts()
    {
        var (_, token) = CreateUserWithToken();
        for (var i = 0; i < 4; i++)
            token.RegisterFailedAttempt(DateTimeOffset.UtcNow, maxAttempts: 5);

        Assert.Null(token.RevokedAt);
    }

    [Fact]
    public void User_MarkEmailVerified_SetsFlag()
    {
        var (user, _) = CreateUserWithToken();
        Assert.False(user.IsEmailVerified);
        user.MarkEmailVerified();
        Assert.True(user.IsEmailVerified);
    }

    [Fact]
    public void User_RevokeSecurityTokens_RevokesActiveTokens()
    {
        var user = new User("test-tenant", "Test User", "test@example.com", "hash", UserRole.Owner);
        var now = DateTimeOffset.UtcNow;
        var t1 = user.AddSecurityToken(UserSecurityTokenPurpose.EmailVerification, "h1", now.AddMinutes(10), now);
        var t2 = user.AddSecurityToken(UserSecurityTokenPurpose.EmailVerification, "h2", now.AddMinutes(10), now);

        user.RevokeSecurityTokens(UserSecurityTokenPurpose.EmailVerification, now);

        Assert.NotNull(t1.RevokedAt);
        Assert.NotNull(t2.RevokedAt);
    }
}
