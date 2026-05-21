using Microsoft.Extensions.Logging;
using MotorCare.Application.Auth.Commands.ForgotPassword;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.UnitTests.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepo = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly ISecurityTokenFactory _tokenFactory = Substitute.For<ISecurityTokenFactory>();
    private readonly ILogger<ForgotPasswordCommandHandler> _logger =
        Substitute.For<ILogger<ForgotPasswordCommandHandler>>();

    private readonly ForgotPasswordCommandHandler _handler;

    private const string TenantId = "tenant-a";
    private const string Email = "user@example.com";

    public ForgotPasswordCommandHandlerTests()
    {
        _handler = new ForgotPasswordCommandHandler(_tenantRepo, _userRepo, _emailSender, _tokenFactory, _logger);

        _tenantRepo.GetByIdentifierAsync(TenantId, default).Returns(new Tenant(TenantId, "Tenant A"));
        _tokenFactory.GenerateNumericCode().Returns("111111", "222222", "333333");
        _tokenFactory.Hash(Arg.Any<string>()).Returns(call => $"HASH_{call.Arg<string>()}");
    }

    [Fact]
    public async Task Handle_IssuesResetTokenOnlyForRequestedTenant_WhenEmailExistsAcrossTenants()
    {
        var tenantAUser = MakeUser("tenant-a", "Tenant A Owner");
        var tenantBUser = MakeUser("tenant-b", "Tenant B Owner");
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(tenantAUser);
        _userRepo.GetLatestSecurityTokenAsync(tenantAUser.Id, UserSecurityTokenPurpose.PasswordReset, default)
            .Returns((UserSecurityToken?)null);

        await _handler.Handle(new ForgotPasswordCommand(TenantId, " User@Example.COM "), default);

        tenantAUser.SecurityTokens.Single().TokenHash.Should().Be("HASH_111111");
        tenantBUser.SecurityTokens.Should().BeEmpty();
        await _emailSender.Received(1).SendPasswordResetCodeAsync(
            tenantAUser.Email,
            tenantAUser.FullName,
            "111111",
            Arg.Any<DateTime>(),
            default);
        await _userRepo.Received(1).GetByEmailWithSecurityTokensAsync(TenantId, Email, default);
        await _userRepo.DidNotReceive().GetByEmailWithSecurityTokensAsync("tenant-b", Email, Arg.Any<CancellationToken>());
        await _userRepo.DidNotReceive().GetByEmailAsync(Email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotStorePlainResetCode()
    {
        var user = MakeUser("tenant-a", "Tenant A Owner");
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(user);
        _userRepo.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.PasswordReset, default)
            .Returns((UserSecurityToken?)null);

        await _handler.Handle(new ForgotPasswordCommand(TenantId, Email), default);

        var token = user.SecurityTokens.Single();
        token.TokenHash.Should().Be("HASH_111111");
        token.TokenHash.Should().NotBe("111111");
    }

    [Fact]
    public async Task Handle_ReturnsGenericAndDoesNotEmail_WhenRequestedTenantUserIsInactive()
    {
        var inactiveUser = MakeUser("tenant-a", "Tenant A Owner");
        inactiveUser.Deactivate();

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(inactiveUser);

        var result = await _handler.Handle(new ForgotPasswordCommand(TenantId, Email), default);

        result.Message.Should().NotBeNullOrWhiteSpace();
        inactiveUser.SecurityTokens.Should().BeEmpty();
        await _emailSender.DidNotReceive().SendPasswordResetCodeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
        await _userRepo.DidNotReceive().GetLatestSecurityTokenAsync(
            Arg.Any<Guid>(),
            Arg.Any<UserSecurityTokenPurpose>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RateLimitsRequestedTenantUser()
    {
        var rateLimitedUser = MakeUser("tenant-a", "Tenant A Owner");
        var recentToken = rateLimitedUser.AddSecurityToken(
            UserSecurityTokenPurpose.PasswordReset,
            "recent-hash",
            DateTimeOffset.UtcNow.AddMinutes(15),
            DateTimeOffset.UtcNow.AddSeconds(-30));

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(rateLimitedUser);
        _userRepo.GetLatestSecurityTokenAsync(rateLimitedUser.Id, UserSecurityTokenPurpose.PasswordReset, default)
            .Returns(recentToken);

        await _handler.Handle(new ForgotPasswordCommand(TenantId, Email), default);

        rateLimitedUser.SecurityTokens.Should().ContainSingle();
        await _emailSender.DidNotReceive().SendPasswordResetCodeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotQueryUsers_WhenTenantIsMissing()
    {
        const string missingTenant = "missing-tenant";
        _tenantRepo.GetByIdentifierAsync(missingTenant, default).Returns((Tenant?)null);

        var result = await _handler.Handle(new ForgotPasswordCommand(missingTenant, Email), default);

        result.Message.Should().NotBeNullOrWhiteSpace();
        await _userRepo.DidNotReceive().GetByEmailWithSecurityTokensAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendPasswordResetCodeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    private static User MakeUser(string tenantId, string fullName)
        => new(tenantId, fullName, Email, $"{tenantId}-hash", UserRole.Owner);
}
