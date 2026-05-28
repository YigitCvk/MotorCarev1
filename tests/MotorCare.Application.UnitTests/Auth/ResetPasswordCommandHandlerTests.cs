using MotorCare.Application.Auth.Commands.ResetPassword;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.UnitTests.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepo = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ISecurityTokenFactory _tokenFactory = Substitute.For<ISecurityTokenFactory>();
    private readonly ResetPasswordCommandHandler _handler;

    private const string TenantId = "tenant-a";
    private const string Email = "user@example.com";
    private const string ResetCode = "222222";
    private const string NewPassword = "NewPassword123!";

    public ResetPasswordCommandHandlerTests()
    {
        _handler = new ResetPasswordCommandHandler(_tenantRepo, _userRepo, _passwordHasher, _tokenFactory);

        _tenantRepo.GetByIdentifierAsync(TenantId, default).Returns(new Tenant(TenantId, "Tenant A"));
        _tokenFactory.Hash(ResetCode).Returns("HASH_222222");
        _tokenFactory.Hash("000000").Returns("HASH_000000");
        _passwordHasher.Hash(NewPassword).Returns("new-password-hash");
    }

    [Fact]
    public async Task Handle_ResetsOnlyUserWhoseActiveTokenMatches_WhenEmailExistsInMultipleTenants()
    {
        var tenantAUser = MakeUser("tenant-a", "Tenant A Owner", "tenant-a-old-hash");
        var tenantBUser = MakeUser("tenant-b", "Tenant B Owner", "tenant-b-old-hash");
        var tenantB = new Tenant("tenant-b", "Tenant B");
        var tenantBToken = tenantBUser.AddSecurityToken(
            UserSecurityTokenPurpose.PasswordReset,
            "HASH_222222",
            DateTimeOffset.UtcNow.AddMinutes(15),
            DateTimeOffset.UtcNow.AddMinutes(-5));

        _tenantRepo.GetByIdentifierAsync("tenant-b", default).Returns(tenantB);
        _userRepo.GetByEmailWithSecurityTokensAsync("tenant-b", Email, default).Returns(tenantBUser);
        _userRepo.GetLatestActiveSecurityTokenAsync(tenantBUser.Id, UserSecurityTokenPurpose.PasswordReset, default)
            .Returns(tenantBToken);
        _userRepo.GetByIdWithRefreshTokensAsync(tenantBUser.Id, default).Returns(tenantBUser);
        _passwordHasher.Verify(tenantBUser.PasswordHash, NewPassword).Returns(false);

        var result = await _handler.Handle(new ResetPasswordCommand("tenant-b", " User@Example.COM ", ResetCode, NewPassword, NewPassword), default);

        result.Message.Should().NotBeNullOrWhiteSpace();
        tenantAUser.PasswordHash.Should().Be("tenant-a-old-hash");
        tenantBUser.PasswordHash.Should().Be("new-password-hash");
        tenantBToken.ConsumedAt.Should().NotBeNull();
        _userRepo.Received(1).Update(Arg.Is<User>(u => u.Id == tenantBUser.Id));
        _userRepo.DidNotReceive().Update(Arg.Is<User>(u => u.Id == tenantAUser.Id));
        await _userRepo.Received(1).GetByEmailWithSecurityTokensAsync("tenant-b", Email, default);
        await _userRepo.DidNotReceive().GetByEmailWithSecurityTokensAsync("tenant-a", Email, Arg.Any<CancellationToken>());
        await _userRepo.DidNotReceive().GetByEmailAsync(Email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RejectsWrongCode_AndRegistersFailedAttempt()
    {
        var user = MakeUser("tenant-a", "Tenant A Owner", "old-hash");
        var token = user.AddSecurityToken(
            UserSecurityTokenPurpose.PasswordReset,
            "HASH_222222",
            DateTimeOffset.UtcNow.AddMinutes(15),
            DateTimeOffset.UtcNow.AddMinutes(-5));

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(user);
        _userRepo.GetLatestActiveSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.PasswordReset, default)
            .Returns(token);

        var act = async () => await _handler.Handle(new ResetPasswordCommand(TenantId, Email, "000000", NewPassword, NewPassword), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        token.FailedAttemptCount.Should().Be(1);
        user.PasswordHash.Should().Be("old-hash");
        await _userRepo.Received(1).SaveChangesAsync(default);
        _userRepo.DidNotReceive().Update(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_IgnoresInactiveUsers_EvenWhenTheyHaveMatchingToken()
    {
        var user = MakeUser("tenant-a", "Tenant A Owner", "old-hash");
        user.Deactivate();
        user.AddSecurityToken(
            UserSecurityTokenPurpose.PasswordReset,
            "HASH_222222",
            DateTimeOffset.UtcNow.AddMinutes(15),
            DateTimeOffset.UtcNow.AddMinutes(-5));

        _userRepo.GetByEmailWithSecurityTokensAsync(TenantId, Email, default).Returns(user);

        var act = async () => await _handler.Handle(new ResetPasswordCommand(TenantId, Email, ResetCode, NewPassword, NewPassword), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        user.PasswordHash.Should().Be("old-hash");
        await _userRepo.DidNotReceive().GetLatestActiveSecurityTokenAsync(
            Arg.Any<Guid>(),
            Arg.Any<UserSecurityTokenPurpose>(),
            Arg.Any<CancellationToken>());
    }

    private static User MakeUser(string tenantId, string fullName, string passwordHash)
    {
        var user = new User(tenantId, fullName, Email, passwordHash, UserRole.Owner);
        user.MarkEmailVerified();
        return user;
    }
}
