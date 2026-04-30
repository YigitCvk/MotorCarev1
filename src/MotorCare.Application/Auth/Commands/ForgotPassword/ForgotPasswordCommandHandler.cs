using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, AuthActionMessageDto>
{
    private const string GenericMessage = "Eğer bu e-posta ile kayıtlı bir hesap varsa şifre sıfırlama bağlantısı gönderildi.";

    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IAuthLinkBuilder _authLinkBuilder;
    private readonly ISecurityTokenFactory _securityTokenFactory;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IEmailSender emailSender,
        IAuthLinkBuilder authLinkBuilder,
        ISecurityTokenFactory securityTokenFactory,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
        _authLinkBuilder = authLinkBuilder;
        _securityTokenFactory = securityTokenFactory;
        _logger = logger;
    }

    public async Task<AuthActionMessageDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var users = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var user in users.Where(x => x.IsActive))
        {
            var latest = await _userRepository.GetLatestSecurityTokenAsync(user.Id, UserSecurityTokenPurpose.PasswordReset, cancellationToken);
            if (latest is not null && latest.CreatedAt >= now.AddMinutes(-1))
            {
                continue;
            }

            user.RevokeSecurityTokens(UserSecurityTokenPurpose.PasswordReset, now);
            var plainToken = _securityTokenFactory.GenerateOpaqueToken();
            var token = user.AddSecurityToken(
                UserSecurityTokenPurpose.PasswordReset,
                _securityTokenFactory.Hash(plainToken),
                now.AddMinutes(30),
                now);

            _userRepository.Update(user);
            _userRepository.AddSecurityToken(token);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                EventIdStore.Auth.PasswordResetEmailSendRequested,
                "Password reset email requested. UserId={UserId} Provider={Provider} ExpiresAtUtc={ExpiresAtUtc}",
                user.Id,
                "Email",
                token.ExpiresAt);

            try
            {
                var resetUrl = _authLinkBuilder.BuildPasswordResetUrl(user.Email, plainToken);
                await _emailSender.SendPasswordResetAsync(user.Email, user.FullName, resetUrl, cancellationToken);
                _logger.LogInformation(
                    EventIdStore.Auth.PasswordResetEmailSent,
                    "Password reset email sent. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
                    user.Id,
                    token.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    EventIdStore.Auth.PasswordResetEmailSendFailed,
                    ex,
                    "Password reset email send failed. UserId={UserId} ExpiresAtUtc={ExpiresAtUtc}",
                    user.Id,
                    token.ExpiresAt);
            }
        }

        return new AuthActionMessageDto(GenericMessage);
    }
}
