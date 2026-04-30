using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;

namespace MotorCare.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string toEmail, string displayName, string verificationUrl, CancellationToken cancellationToken)
        => SendAsync(EmailTemplateFactory.CreateVerificationEmail(toEmail, displayName, verificationUrl), cancellationToken);

    public Task SendPasswordResetCodeAsync(string toEmail, string displayName, string code, DateTime expiresAtUtc, CancellationToken cancellationToken)
        => SendAsync(EmailTemplateFactory.CreatePasswordResetCodeEmail(toEmail, displayName, code, expiresAtUtc), cancellationToken);

    public Task SendTwoFactorCodeAsync(string toEmail, string displayName, string code, DateTime expiresAtUtc, CancellationToken cancellationToken)
        => SendAsync(EmailTemplateFactory.CreateTwoFactorEmail(toEmail, displayName, code, expiresAtUtc), cancellationToken);

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = _options.UseDefaultCredentials
        };

        if (!_options.UseDefaultCredentials)
        {
            client.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);
        }

        using var mail = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };

        mail.To.Add(new MailAddress(message.ToEmail, message.ToName));

        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.TextBody, null, "text/plain"));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(mail, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP email send failed. To={ToEmail} Subject={Subject}", message.ToEmail, message.Subject);
            throw;
        }
    }
}
