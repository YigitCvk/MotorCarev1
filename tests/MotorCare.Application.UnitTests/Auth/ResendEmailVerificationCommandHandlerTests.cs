using Microsoft.Extensions.Logging;
using MotorCare.Application.Auth.Commands.ResendEmailVerification;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.UnitTests.Auth;

public class ResendEmailVerificationCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly ITenantRepository _tenantRepo = Substitute.For<ITenantRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly ISecurityTokenFactory _tokenFactory = Substitute.For<ISecurityTokenFactory>();
    private readonly ILogger<ResendEmailVerificationCommandHandler> _logger =
        Substitute.For<ILogger<ResendEmailVerificationCommandHandler>>();

    private readonly ResendEmailVerificationCommandHandler _handler;
    private const string TenantId = "test-tenant";
    private const string Email = "test@example.com";

    public ResendEmailVerificationCommandHandlerTests()
    {
        _handler = new ResendEmailVerificationCommandHandler(
            _userRepo, _tenantRepo, _emailSender, _tokenFactory, _logger);

        _tokenFactory.GenerateNumericCode().Returns("123456");
        _tokenFactory.Hash("123456").Returns("HASH_123456");
    }

    private Tenant MakeTenant() => new Tenant(TenantId, "Test Tenant");

    private User MakeUser(bool emailVerified = false)
    {
        var user = new User(TenantId, "Test User", Email, "pw-hash", UserRole.Owner);
        if (emailVerified) user.MarkEmailVerified();
        return user;
    }

    private ResendEmailVerificationCommand Command() => new(TenantId, Email);

    [Fact]
    public async Task Handle_SendsCode_WhenUserUnverifiedAndNoRateLimit()
    {
        _tenantRepo.GetByIdentifierAsync(TenantId, default).Returns(MakeTenant());
        var user = MakeUser();
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(user);
        _userRepo.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns((UserSecurityToken?)null);

        var result = await _handler.Handle(Command(), default);

        result.Message.Should().Contain("gönderildi");
        await _emailSender.Received(1).SendEmailVerificationAsync(
            Email, Arg.Any<string>(), "123456", Arg.Any<DateTime>(), default);
    }

    [Fact]
    public async Task Handle_CodeIsHashedNotPlaintext_InStorage()
    {
        _tenantRepo.GetByIdentifierAsync(TenantId, default).Returns(MakeTenant());
        var user = MakeUser();
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(user);
        _userRepo.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns((UserSecurityToken?)null);

        await _handler.Handle(Command(), default);

        // Only the hash should be stored; plain code must never reach AddSecurityToken
        var storedToken = user.SecurityTokens.Single();
        storedToken.TokenHash.Should().Be("HASH_123456");
        storedToken.TokenHash.Should().NotBe("123456");
    }

    [Fact]
    public async Task Handle_ReturnsAlreadyVerified_WhenEmailVerified()
    {
        _tenantRepo.GetByIdentifierAsync(TenantId, default).Returns(MakeTenant());
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(MakeUser(emailVerified: true));

        var result = await _handler.Handle(Command(), default);

        result.Message.Should().Contain("zaten doğrulanmış");
        await _emailSender.DidNotReceive().SendEmailVerificationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>(), default);
    }

    [Fact]
    public async Task Handle_ReturnsRateLimit_WhenSentWithin60Seconds()
    {
        _tenantRepo.GetByIdentifierAsync(TenantId, default).Returns(MakeTenant());
        var user = MakeUser();
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(user);

        var recentToken = user.AddSecurityToken(
            UserSecurityTokenPurpose.EmailVerification,
            "recent-hash",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddSeconds(-30)); // 30 seconds ago — within 60s window

        _userRepo.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns(recentToken);

        var result = await _handler.Handle(Command(), default);

        result.Message.Should().Contain("60 saniye");
        await _emailSender.DidNotReceive().SendEmailVerificationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>(), default);
    }

    [Fact]
    public async Task Handle_InvalidatesPreviousCode_BeforeSendingNew()
    {
        _tenantRepo.GetByIdentifierAsync(TenantId, default).Returns(MakeTenant());
        var user = MakeUser();
        var oldToken = user.AddSecurityToken(
            UserSecurityTokenPurpose.EmailVerification,
            "old-hash",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow.AddMinutes(-5)); // 5 minutes ago — past 60s rate limit

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(user);
        _userRepo.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns(oldToken);

        await _handler.Handle(Command(), default);

        oldToken.RevokedAt.Should().NotBeNull("previous token must be revoked before issuing a new one");
    }

    [Fact]
    public async Task Handle_NewCodeExpiresIn10Minutes()
    {
        _tenantRepo.GetByIdentifierAsync(TenantId, default).Returns(MakeTenant());
        var user = MakeUser();
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(user);
        _userRepo.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, default)
            .Returns((UserSecurityToken?)null);

        var before = DateTimeOffset.UtcNow;
        await _handler.Handle(Command(), default);
        var after = DateTimeOffset.UtcNow;

        var newToken = user.SecurityTokens.Single();
        newToken.ExpiresAt.Should().BeCloseTo(before.AddMinutes(10), TimeSpan.FromSeconds(5));
        newToken.ExpiresAt.Should().BeBefore(after.AddMinutes(10).AddSeconds(2));
    }
}
