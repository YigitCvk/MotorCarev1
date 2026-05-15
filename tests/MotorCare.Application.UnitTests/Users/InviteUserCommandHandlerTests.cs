using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Users.Commands.InviteUser;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.UnitTests.Users;

public class InviteUserCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();
    private readonly ISecurityTokenFactory _tokenFactory = Substitute.For<ISecurityTokenFactory>();
    private readonly IAuthLinkBuilder _authLinkBuilder = Substitute.For<IAuthLinkBuilder>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly ILogger<InviteUserCommandHandler> _logger = Substitute.For<ILogger<InviteUserCommandHandler>>();

    private readonly InviteUserCommandHandler _handler;
    private const string TenantId = "test-tenant";
    private const string Email = "newuser@example.com";

    public InviteUserCommandHandlerTests()
    {
        _tenantProvider.GetTenantId().Returns(TenantId);
        _tokenFactory.GenerateOpaqueToken().Returns("raw-token-abc");
        _tokenFactory.Hash("raw-token-abc").Returns("HASH_ABC");
        _authLinkBuilder.BuildInviteUrl("raw-token-abc").Returns("https://app.test/accept-invite?token=raw-token-abc");

        _handler = new InviteUserCommandHandler(
            _userRepo, _tenantProvider, _tokenFactory, _authLinkBuilder, _emailSender, _logger);
    }

    private InviteUserCommand Command() => new(Email, UserRole.Technician);

    [Fact]
    public async Task Handle_CreatesNewPendingUser_WhenNoExistingUser()
    {
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns((User?)null);

        await _handler.Handle(Command(), default);

        await _userRepo.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == Email && !u.IsEmailVerified),
            default);
    }

    [Fact]
    public async Task Handle_SendsInviteEmail_WhenNewUser()
    {
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns((User?)null);

        await _handler.Handle(Command(), default);

        await _emailSender.Received(1).SendUserInvitationAsync(
            Email, TenantId, "https://app.test/accept-invite?token=raw-token-abc", Arg.Any<DateTime>(), default);
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenUserAlreadyVerified()
    {
        var verifiedUser = new User(TenantId, "Active User", Email, "pw-hash", UserRole.Technician);
        verifiedUser.MarkEmailVerified();
        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(verifiedUser);

        var act = async () => await _handler.Handle(Command(), default);

        await act.Should().ThrowAsync<ConflictException>();
        await _emailSender.DidNotReceive().SendUserInvitationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>(), default);
    }

    [Fact]
    public async Task Handle_RevokesOldToken_WhenResendingToPendingUser()
    {
        var pendingUser = new User(TenantId, "Davet Bekliyor", Email, $"INVITE_{Guid.NewGuid():N}", UserRole.Technician);
        var oldToken = pendingUser.AddSecurityToken(
            UserSecurityTokenPurpose.UserInvitation,
            "old-hash",
            DateTimeOffset.UtcNow.AddHours(24),
            DateTimeOffset.UtcNow.AddHours(-2));

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(pendingUser);

        await _handler.Handle(Command(), default);

        oldToken.RevokedAt.Should().NotBeNull("old invite token must be revoked before issuing a new one");
    }

    [Fact]
    public async Task Handle_IssuesNewToken_WhenResendingToPendingUser()
    {
        var pendingUser = new User(TenantId, "Davet Bekliyor", Email, $"INVITE_{Guid.NewGuid():N}", UserRole.Technician);
        pendingUser.AddSecurityToken(
            UserSecurityTokenPurpose.UserInvitation,
            "old-hash",
            DateTimeOffset.UtcNow.AddHours(24),
            DateTimeOffset.UtcNow.AddHours(-2));

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(pendingUser);

        await _handler.Handle(Command(), default);

        _userRepo.Received(1).AddSecurityToken(
            Arg.Is<UserSecurityToken>(t =>
                t.TokenHash == "HASH_ABC" &&
                t.Purpose == UserSecurityTokenPurpose.UserInvitation &&
                t.RevokedAt == null));
    }

    [Fact]
    public async Task Handle_SendsNewEmail_WhenResendingToPendingUser()
    {
        var pendingUser = new User(TenantId, "Davet Bekliyor", Email, $"INVITE_{Guid.NewGuid():N}", UserRole.Technician);
        pendingUser.AddSecurityToken(
            UserSecurityTokenPurpose.UserInvitation,
            "old-hash",
            DateTimeOffset.UtcNow.AddHours(24),
            DateTimeOffset.UtcNow.AddHours(-2));

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(pendingUser);

        await _handler.Handle(Command(), default);

        await _emailSender.Received(1).SendUserInvitationAsync(
            Email, TenantId, Arg.Any<string>(), Arg.Any<DateTime>(), default);
    }

    [Fact]
    public async Task Handle_DoesNotCreateNewUser_WhenResendingToPendingUser()
    {
        var pendingUser = new User(TenantId, "Davet Bekliyor", Email, $"INVITE_{Guid.NewGuid():N}", UserRole.Technician);
        pendingUser.AddSecurityToken(
            UserSecurityTokenPurpose.UserInvitation,
            "old-hash",
            DateTimeOffset.UtcNow.AddHours(24),
            DateTimeOffset.UtcNow.AddHours(-2));

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(pendingUser);

        await _handler.Handle(Command(), default);

        await _userRepo.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
