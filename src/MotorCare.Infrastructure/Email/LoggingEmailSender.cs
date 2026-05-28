using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;

namespace MotorCare.Infrastructure.Email;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;
    private readonly EmailOptions _options;
    private readonly IHostEnvironment _environment;

    public LoggingEmailSender(
        ILogger<LoggingEmailSender> logger,
        IOptions<EmailOptions> options,
        IHostEnvironment environment)
    {
        _logger = logger;
        _options = options.Value;
        _environment = environment;
    }

    public Task SendEmailVerificationAsync(string toEmail, string displayName, string code, DateTime expiresAtUtc, CancellationToken cancellationToken)
        => SendAsync(EmailTemplateFactory.CreateVerificationEmail(toEmail, displayName, code, expiresAtUtc), cancellationToken);

    public Task SendPasswordResetCodeAsync(string toEmail, string displayName, string code, DateTime expiresAtUtc, CancellationToken cancellationToken)
        => SendAsync(EmailTemplateFactory.CreatePasswordResetCodeEmail(toEmail, displayName, code, expiresAtUtc), cancellationToken);

    public Task SendTwoFactorCodeAsync(string toEmail, string displayName, string code, DateTime expiresAtUtc, CancellationToken cancellationToken)
        => SendAsync(EmailTemplateFactory.CreateTwoFactorEmail(toEmail, displayName, code, expiresAtUtc), cancellationToken);

    public Task SendUserInvitationAsync(string toEmail, string tenantIdentifier, string inviteUrl, DateTime expiresAtUtc, CancellationToken cancellationToken)
        => SendAsync(EmailTemplateFactory.CreateInvitationEmail(toEmail, tenantIdentifier, inviteUrl, expiresAtUtc), cancellationToken);

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (_environment.IsDevelopment() && _options.LogEmailBodyInDevelopment)
        {
            _logger.LogInformation(
                "Email fallback sender. To={ToEmail} Subject={Subject} HtmlBody={HtmlBody}",
                message.ToEmail,
                message.Subject,
                message.HtmlBody);
        }
        else
        {
            _logger.LogInformation(
                "Email fallback sender. To={ToEmail} Subject={Subject}",
                message.ToEmail,
                message.Subject);
        }

        return Task.CompletedTask;
    }
}
