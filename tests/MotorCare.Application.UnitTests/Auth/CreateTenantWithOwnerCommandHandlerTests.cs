using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Tenants.Commands.CreateTenantWithOwner;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.UnitTests.Auth;

public class CreateTenantWithOwnerCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepo = Substitute.For<ITenantRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly ISecurityTokenFactory _tokenFactory = Substitute.For<ISecurityTokenFactory>();
    private readonly ILogger<CreateTenantWithOwnerCommandHandler> _logger =
        Substitute.For<ILogger<CreateTenantWithOwnerCommandHandler>>();

    private readonly CreateTenantWithOwnerCommandHandler _handler;

    public CreateTenantWithOwnerCommandHandlerTests()
    {
        _handler = new CreateTenantWithOwnerCommandHandler(
            _tenantRepo, _userRepo, _passwordHasher, _emailSender, _tokenFactory, _logger);

        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");
        _tokenFactory.GenerateNumericCode().Returns("654321");
        _tokenFactory.Hash("654321").Returns("HASH_654321");
        _tenantRepo.GetByIdentifierAsync(Arg.Any<string>(), default).Returns((Domain.Tenants.Tenant?)null);
        _userRepo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<string>(), default).Returns((Domain.Users.User?)null);
    }

    private CreateTenantWithOwnerCommand ValidCommand() => new(
        "acme-motors", "Acme Motors", "Owner Name", "owner@acme.com", "P@ssword1!");

    [Fact]
    public async Task Handle_SendsNumericCode_NotLink()
    {
        await _handler.Handle(ValidCommand(), default);

        await _emailSender.Received(1).SendEmailVerificationAsync(
            "owner@acme.com",
            Arg.Any<string>(),
            "654321",
            Arg.Any<DateTime>(),
            default);
    }

    [Fact]
    public async Task Handle_StoresHashNotPlainCode()
    {
        Domain.Users.User? capturedUser = null;
        await _userRepo.AddAsync(Arg.Do<Domain.Users.User>(u => capturedUser = u), default);

        await _handler.Handle(ValidCommand(), default);

        capturedUser.Should().NotBeNull();
        var token = capturedUser!.SecurityTokens.Single();
        token.TokenHash.Should().Be("HASH_654321");
        token.TokenHash.Should().NotBe("654321");
    }

    [Fact]
    public async Task Handle_VerificationTokenExpiresIn10Minutes()
    {
        Domain.Users.User? capturedUser = null;
        await _userRepo.AddAsync(Arg.Do<Domain.Users.User>(u => capturedUser = u), default);

        var before = DateTimeOffset.UtcNow;
        await _handler.Handle(ValidCommand(), default);

        var token = capturedUser!.SecurityTokens.Single();
        token.ExpiresAt.Should().BeCloseTo(before.AddMinutes(10), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ResponseContainsVerificationEmailSentTrue_WhenEmailSucceeds()
    {
        var result = await _handler.Handle(ValidCommand(), default);

        result.VerificationEmailSent.Should().BeTrue();
    }
}
