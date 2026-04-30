using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Tenants.Commands.CreateTenantWithOwner;

public class CreateTenantWithOwnerCommandHandler : IRequestHandler<CreateTenantWithOwnerCommand, CreateTenantWithOwnerResultDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IAuthLinkBuilder _authLinkBuilder;
    private readonly ISecurityTokenFactory _securityTokenFactory;
    private readonly ILogger<CreateTenantWithOwnerCommandHandler> _logger;

    public CreateTenantWithOwnerCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IAuthLinkBuilder authLinkBuilder,
        ISecurityTokenFactory securityTokenFactory,
        ILogger<CreateTenantWithOwnerCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _authLinkBuilder = authLinkBuilder;
        _securityTokenFactory = securityTokenFactory;
        _logger = logger;
    }

    public async Task<CreateTenantWithOwnerResultDto> Handle(CreateTenantWithOwnerCommand request, CancellationToken cancellationToken)
    {
        var existingTenant = await _tenantRepository.GetByIdentifierAsync(request.TenantIdentifier, cancellationToken);
        if (existingTenant is not null)
        {
            throw new ConflictException("A tenant with this identifier already exists.");
        }

        var normalizedEmail = request.OwnerEmail.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(request.TenantIdentifier, normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException("A user with this email already exists for the tenant.");
        }

        var tenant = new Tenant(request.TenantIdentifier, request.TenantName);
        var passwordHash = _passwordHasher.Hash(request.OwnerPassword);
        var owner = new User(tenant.Identifier, request.OwnerFullName, request.OwnerEmail, passwordHash, UserRole.Owner);

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _userRepository.AddAsync(owner, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var verificationEmailSent = await TrySendVerificationEmailAsync(owner, cancellationToken);

        return new CreateTenantWithOwnerResultDto(tenant.Id, tenant.Identifier, owner.Id, owner.Email, verificationEmailSent);
    }

    private async Task<bool> TrySendVerificationEmailAsync(User owner, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var plainToken = _securityTokenFactory.GenerateOpaqueToken();
        var tokenHash = _securityTokenFactory.Hash(plainToken);
        var token = owner.AddSecurityToken(
            UserSecurityTokenPurpose.EmailVerification,
            tokenHash,
            now.AddHours(24),
            now);

        _userRepository.Update(owner);
        _userRepository.AddSecurityToken(token);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Auth.EmailVerificationSendRequested,
            "Email verification requested. UserId={UserId} Provider={Provider}",
            owner.Id,
            "Email");

        try
        {
            var verificationUrl = _authLinkBuilder.BuildEmailVerificationUrl(owner.Email, plainToken);
            await _emailSender.SendEmailVerificationAsync(owner.Email, owner.FullName, verificationUrl, cancellationToken);

            _logger.LogInformation(
                EventIdStore.Auth.EmailVerificationSent,
                "Email verification sent. UserId={UserId}",
                owner.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                EventIdStore.Auth.EmailVerificationSendFailed,
                ex,
                "Email verification send failed. UserId={UserId}",
                owner.Id);

            return false;
        }
    }
}
