using Microsoft.Extensions.Logging;
using MotorCare.Application.Auth.Commands.Login;
using MotorCare.Application.Common.Errors;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;

namespace MotorCare.Application.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepo = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly ISecurityTokenFactory _tokenFactory = Substitute.For<ISecurityTokenFactory>();
    private readonly ILogger<LoginCommandHandler> _logger = Substitute.For<ILogger<LoginCommandHandler>>();

    private const string TenantIdentifier = "test-tenant";
    private const string Email = "user@example.com";
    private const string Password = "Password123!";

    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _tenantRepo, _userRepo, _passwordHasher, _jwtTokenGenerator,
            _refreshTokenGenerator, _emailSender, _tokenFactory, _logger);
    }

    private static LoginCommand Command() => new(TenantIdentifier, Email, Password);

    private static Tenant MakeTenant(bool active = true)
    {
        var t = new Tenant(TenantIdentifier, "Test İşletme");
        if (!active) t.Deactivate();
        return t;
    }

    private static User MakeUser(bool active = true, bool emailVerified = true)
    {
        var u = new User(TenantIdentifier, "Test User", Email, "hash", Domain.Enums.UserRole.Owner);
        if (emailVerified) u.MarkEmailVerified();
        if (!active) u.Deactivate();
        return u;
    }

    [Fact]
    public async Task Handle_ThrowsLoginFailed_WhenTenantNotFound()
    {
        _tenantRepo.GetByIdentifierAsync(TenantIdentifier, default).Returns((Tenant?)null);

        var act = async () => await _handler.Handle(Command(), default);

        var ex = await act.Should().ThrowAsync<LoginException>();
        ex.Which.Code.Should().Be(ErrorCodes.LoginFailed);
        ex.Which.UserMessage.Should().Be("İşletme kodu, e-posta veya şifre hatalı.");
    }

    [Fact]
    public async Task Handle_ThrowsTenantInactive_WhenTenantIsInactive()
    {
        _tenantRepo.GetByIdentifierAsync(TenantIdentifier, default).Returns(MakeTenant(active: false));

        var act = async () => await _handler.Handle(Command(), default);

        var ex = await act.Should().ThrowAsync<LoginException>();
        ex.Which.Code.Should().Be(ErrorCodes.TenantInactive);
    }

    [Fact]
    public async Task Handle_ThrowsLoginFailed_WhenUserNotFound()
    {
        _tenantRepo.GetByIdentifierAsync(TenantIdentifier, default).Returns(MakeTenant());
        _userRepo.GetByEmailAsync(TenantIdentifier, Email, default).Returns((User?)null);

        var act = async () => await _handler.Handle(Command(), default);

        var ex = await act.Should().ThrowAsync<LoginException>();
        ex.Which.Code.Should().Be(ErrorCodes.LoginFailed);
    }

    [Fact]
    public async Task Handle_ThrowsUserInactive_WhenUserIsInactive()
    {
        _tenantRepo.GetByIdentifierAsync(TenantIdentifier, default).Returns(MakeTenant());
        _userRepo.GetByEmailAsync(TenantIdentifier, Email, default).Returns(MakeUser(active: false));

        var act = async () => await _handler.Handle(Command(), default);

        var ex = await act.Should().ThrowAsync<LoginException>();
        ex.Which.Code.Should().Be(ErrorCodes.UserInactive);
        ex.Which.UserMessage.Should().Be("Hesabınız şu anda aktif değil.");
    }

    [Fact]
    public async Task Handle_ThrowsLoginFailed_WhenPasswordIsWrong()
    {
        var user = MakeUser();
        _tenantRepo.GetByIdentifierAsync(TenantIdentifier, default).Returns(MakeTenant());
        _userRepo.GetByEmailAsync(TenantIdentifier, Email, default).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, Password).Returns(false);

        var act = async () => await _handler.Handle(Command(), default);

        var ex = await act.Should().ThrowAsync<LoginException>();
        ex.Which.Code.Should().Be(ErrorCodes.LoginFailed);
        ex.Which.UserMessage.Should().Be("İşletme kodu, e-posta veya şifre hatalı.");
    }

    [Fact]
    public async Task Handle_ThrowsEmailNotVerified_WhenEmailIsUnverified()
    {
        var user = MakeUser(emailVerified: false);
        _tenantRepo.GetByIdentifierAsync(TenantIdentifier, default).Returns(MakeTenant());
        _userRepo.GetByEmailAsync(TenantIdentifier, Email, default).Returns(user);
        _passwordHasher.Verify(user.PasswordHash, Password).Returns(true);

        var act = async () => await _handler.Handle(Command(), default);

        var ex = await act.Should().ThrowAsync<LoginException>();
        ex.Which.Code.Should().Be(ErrorCodes.EmailNotVerified);
        ex.Which.UserMessage.Should().Be("E-posta adresinizi doğrulamanız gerekiyor.");
    }

    [Fact]
    public async Task Handle_DoesNotExposeSystemDetails_InLoginException()
    {
        _tenantRepo.GetByIdentifierAsync(TenantIdentifier, default).Returns((Tenant?)null);

        var act = async () => await _handler.Handle(Command(), default);

        var ex = await act.Should().ThrowAsync<LoginException>();
        // UserMessage must be safe for display — no technical details
        ex.Which.UserMessage.ToLowerInvariant().Should().NotContain("not found");
        ex.Which.UserMessage.ToLowerInvariant().Should().NotContain("null");
        ex.Which.UserMessage.ToLowerInvariant().Should().NotContain("exception");
    }
}
