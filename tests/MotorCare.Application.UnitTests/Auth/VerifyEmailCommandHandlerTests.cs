using MotorCare.Application.Auth.Commands.VerifyEmail;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.UnitTests.Auth;

public class VerifyEmailCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly ISecurityTokenFactory _tokenFactory = Substitute.For<ISecurityTokenFactory>();
    private readonly VerifyEmailCommandHandler _handler;

    private const string Tenant = "test-tenant";
    private const string Email = "test@example.com";
    private const string ValidCode = "123456";
    private const string ValidHash = "HASH_OF_123456";

    public VerifyEmailCommandHandlerTests()
    {
        _handler = new VerifyEmailCommandHandler(_userRepo, _tokenFactory);
        _tokenFactory.Hash(ValidCode).Returns(ValidHash);
    }

    private static User MakeUser(bool emailVerified = false)
    {
        var user = new User(Tenant, "Test User", Email, "pw-hash", UserRole.Owner);
        if (emailVerified) user.MarkEmailVerified();
        return user;
    }

    private static UserSecurityToken MakeToken(User user, string hash, DateTimeOffset? expiresAt = null)
    {
        var now = DateTimeOffset.UtcNow;
        return user.AddSecurityToken(
            UserSecurityTokenPurpose.EmailVerification,
            hash,
            expiresAt ?? now.AddMinutes(10),
            now);
    }

    private VerifyEmailCommand ValidCommand() => new(Tenant, Email, ValidCode);

    // ─── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_VerifiesEmail_WhenCodeIsValid()
    {
        var user = MakeUser();
        var token = MakeToken(user, ValidHash);
        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns(user);
        _userRepo.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns(token);

        var result = await _handler.Handle(ValidCommand(), default);

        user.IsEmailVerified.Should().BeTrue();
        token.ConsumedAt.Should().NotBeNull();
        result.Message.Should().Contain("doğrulandı");
    }

    [Fact]
    public async Task Handle_RevokesRemainingTokens_AfterSuccessfulVerification()
    {
        var user = MakeUser();
        var token = MakeToken(user, ValidHash);
        // A second stale token that should be revoked
        var stale = MakeToken(user, "STALE_HASH");

        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns(user);
        _userRepo.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns(token);

        await _handler.Handle(ValidCommand(), default);

        stale.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReturnAlreadyVerified_WhenUserAlreadyVerified()
    {
        var user = MakeUser(emailVerified: true);
        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns(user);

        var result = await _handler.Handle(ValidCommand(), default);

        result.Message.Should().Contain("zaten doğrulanmış");
    }

    // ─── Invalid code ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Throws_WhenUserNotFound()
    {
        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns((User?)null);

        var act = () => _handler.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_Throws_WhenNoActiveToken()
    {
        var user = MakeUser();
        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns(user);
        _userRepo.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns((UserSecurityToken?)null);

        var act = () => _handler.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_Throws_AndIncrementsAttempt_WhenCodeIsWrong()
    {
        var user = MakeUser();
        _tokenFactory.Hash("WRONG").Returns("WRONG_HASH");
        var token = MakeToken(user, ValidHash);

        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns(user);
        _userRepo.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns(token);

        var act = () => _handler.Handle(new VerifyEmailCommand(Tenant, Email, "WRONG"), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        token.FailedAttemptCount.Should().Be(1);
        await _userRepo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_RevokesToken_WhenMaxAttemptsReached()
    {
        var user = MakeUser();
        _tokenFactory.Hash("WRONG").Returns("WRONG_HASH");
        var token = MakeToken(user, ValidHash);

        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns(user);
        _userRepo.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns(token);

        var cmd = new VerifyEmailCommand(Tenant, Email, "WRONG");
        for (var i = 0; i < 5; i++)
        {
            try { await _handler.Handle(cmd, default); } catch { /* expected */ }
        }

        token.RevokedAt.Should().NotBeNull("token should be auto-revoked after 5 failed attempts");
    }

    [Fact]
    public async Task Handle_Throws_WhenCodeIsExpired()
    {
        var user = MakeUser();
        // Expired token — IsActive returns false, so GetLatestActiveSecurityTokenAsync returns null
        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns(user);
        _userRepo.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns((UserSecurityToken?)null);

        var act = () => _handler.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_DoesNotVerify_WhenCodeBelongsToAnotherUser()
    {
        // User B tries to use User A's code. User B has no active token of their own.
        var userB = MakeUser();
        _userRepo.GetByEmailWithSecurityTokensAsync(Tenant, Email, default).Returns(userB);
        _userRepo.GetLatestActiveSecurityTokenAsync(userB.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns((UserSecurityToken?)null); // userB has no active token

        var act = () => _handler.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        userB.IsEmailVerified.Should().BeFalse();
    }
}
