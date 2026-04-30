using MotorCare.Application.Common.Models;

namespace MotorCare.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        string toEmail,
        string displayName,
        string verificationUrl,
        CancellationToken cancellationToken);

    Task SendPasswordResetCodeAsync(
        string toEmail,
        string displayName,
        string code,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken);

    Task SendTwoFactorCodeAsync(
        string toEmail,
        string displayName,
        string code,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken);

    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
