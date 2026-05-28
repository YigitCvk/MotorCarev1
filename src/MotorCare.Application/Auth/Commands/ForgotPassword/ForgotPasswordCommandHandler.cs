using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, AuthActionMessageDto>
{
    private const string GenericMessage = "Eğer bu e-posta ile kayıtlı bir hesap varsa 6 haneli şifre sıfırlama kodu gönderildi.";

    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ISecurityTokenFactory _securityTokenFactory;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IEmailSender emailSender,
        ISecurityTokenFactory securityTokenFactory,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _securityTokenFactory = securityTokenFactory;
        _logger = logger;
    }

    public async Task<AuthActionMessageDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var tenantIdentifier = request.TenantIdentifier.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var tenant = await _tenantRepository.GetByIdentifierAsync(tenantIdentifier, cancellationToken);
        if (tenant is null || !tenant.IsActive)
        {
            return new AuthActionMessageDto(GenericMessage);
        }

        var user = await _userRepository.GetByEmailWithSecurityTokensAsync(tenant.Identifier, normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return new AuthActionMessageDto(GenericMessage);
        }

        var now = DateTimeOffset.UtcNow;

        var latest = await _userRepository.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.PasswordReset, cancellationToken);
        if (latest is not null && latest.CreatedAt >= now.AddMinutes(-1))
        {
            return new AuthActionMessageDto(GenericMessage);
        }

        user.RevokeSecurityTokens(UserSecurityTokenPurpose.PasswordReset, now);
        var plainCode = _securityTokenFactory.GenerateNumericCode();
        var expiresAt = now.AddMinutes(15);
        var token = user.AddSecurityToken(
            UserSecurityTokenPurpose.PasswordReset,
            _securityTokenFactory.Hash(plainCode),
            expiresAt,
            now);

        _userRepository.Update(user);
        _userRepository.AddSecurityToken(token);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Auth.PasswordResetEmailSendRequested,
            "Password reset code email requested. UserId={UserId} TenantIdentifier={TenantIdentifier} Provider={Provider} ExpiresAtUtc={ExpiresAtUtc}",
            user.Id,
            tenant.Identifier,
            "Email",
            token.ExpiresAt);

        try
        {
            await _emailSender.SendPasswordResetCodeAsync(user.Email, user.FullName, plainCode, expiresAt.UtcDateTime, cancellationToken);
            _logger.LogInformation(
                EventIdStore.Auth.PasswordResetEmailSent,
                "Password reset code email sent. UserId={UserId} TenantIdentifier={TenantIdentifier} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                tenant.Identifier,
                token.ExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                EventIdStore.Auth.PasswordResetEmailSendFailed,
                ex,
                "Password reset code email send failed. UserId={UserId} TenantIdentifier={TenantIdentifier} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                tenant.Identifier,
                token.ExpiresAt);
        }

        return new AuthActionMessageDto(GenericMessage);
    }
}
