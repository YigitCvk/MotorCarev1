using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.ResendEmailVerification;

public sealed class ResendEmailVerificationCommandHandler : IRequestHandler<ResendEmailVerificationCommand, AuthActionMessageDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IEmailSender _emailSender;
    private readonly IAuthLinkBuilder _authLinkBuilder;
    private readonly ISecurityTokenFactory _securityTokenFactory;
    private readonly ILogger<ResendEmailVerificationCommandHandler> _logger;

    public ResendEmailVerificationCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IEmailSender emailSender,
        IAuthLinkBuilder authLinkBuilder,
        ISecurityTokenFactory securityTokenFactory,
        ILogger<ResendEmailVerificationCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _emailSender = emailSender;
        _authLinkBuilder = authLinkBuilder;
        _securityTokenFactory = securityTokenFactory;
        _logger = logger;
    }

    public async Task<AuthActionMessageDto> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdentifierAsync(request.TenantIdentifier, cancellationToken)
            ?? throw new UnauthorizedAccessException("İşletme bulunamadı.");

        var user = await _userRepository.GetByEmailAsync(tenant.Identifier, request.Email.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        if (user.IsEmailVerified)
        {
            return new AuthActionMessageDto("E-posta adresi zaten doğrulanmış.");
        }

        var now = DateTimeOffset.UtcNow;
        var latest = await _userRepository.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.EmailVerification, cancellationToken);
        if (latest is not null && latest.CreatedAt >= now.AddMinutes(-1))
        {
            return new AuthActionMessageDto("Lütfen yeni doğrulama e-postası istemeden önce kısa süre bekleyin.");
        }

        user.RevokeSecurityTokens(UserSecurityTokenPurpose.EmailVerification, now);
        var plainToken = _securityTokenFactory.GenerateOpaqueToken();
        var token = user.AddSecurityToken(
            UserSecurityTokenPurpose.EmailVerification,
            _securityTokenFactory.Hash(plainToken),
            now.AddHours(24),
            now);

        _userRepository.Update(user);
        _userRepository.AddSecurityToken(token);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Auth.EmailVerificationSendRequested,
            "Email verification resend requested. UserId={UserId}",
            user.Id);

        try
        {
            var verificationUrl = _authLinkBuilder.BuildEmailVerificationUrl(user.Email, plainToken);
            await _emailSender.SendEmailVerificationAsync(user.Email, user.FullName, verificationUrl, cancellationToken);
            _logger.LogInformation(EventIdStore.Auth.EmailVerificationSent, "Email verification resent. UserId={UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(EventIdStore.Auth.EmailVerificationSendFailed, ex, "Email verification resend failed. UserId={UserId}", user.Id);
        }

        return new AuthActionMessageDto("Doğrulama e-postası gönderildi.");
    }
}
