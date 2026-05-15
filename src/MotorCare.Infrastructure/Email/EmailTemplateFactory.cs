using System.Globalization;
using MotorCare.Application.Common.Models;

namespace MotorCare.Infrastructure.Email;

internal static class EmailTemplateFactory
{
    public static EmailMessage CreateVerificationEmail(string toEmail, string displayName, string code, DateTime expiresAtUtc)
    {
        var remaining = Math.Max(1, (int)Math.Ceiling((expiresAtUtc - DateTime.UtcNow).TotalMinutes));
        var subject = "GarajPass e-posta doğrulama kodu";
        var html = $"""
            <div style="font-family:Segoe UI,Arial,sans-serif;background:#f8fafc;padding:24px;color:#0f172a">
              <div style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:20px;padding:32px;border:1px solid #e2e8f0">
                <div style="font-size:13px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#f97316;margin-bottom:12px">GarajPass</div>
                <h1 style="font-size:24px;margin:0 0 12px">E-posta doğrulama kodunuz</h1>
                <p style="font-size:15px;line-height:1.7;margin:0 0 20px">Merhaba {displayName},</p>
                <p style="font-size:15px;line-height:1.7;margin:0 0 16px">GarajPass hesabınızı etkinleştirmek için doğrulama kodunuz:</p>
                <div style="font-size:36px;font-weight:800;letter-spacing:.22em;background:#0f172a;color:#f8fafc;border-radius:16px;padding:18px 24px;text-align:center;margin:0 0 20px">{code}</div>
                <p style="font-size:14px;line-height:1.7;margin:0 0 8px">Kod {remaining} dakika geçerlidir.</p>
                <p style="font-size:14px;line-height:1.7;color:#475569;margin:0">Bu işlemi siz başlatmadıysanız bu e-postayı yok sayabilirsiniz.</p>
              </div>
            </div>
            """;
        var text = $"Merhaba {displayName}, GarajPass hesabınızı etkinleştirmek için doğrulama kodunuz: {code}. Kod {remaining} dakika geçerlidir. Bu işlemi siz başlatmadıysanız bu e-postayı yok sayabilirsiniz.";
        return new EmailMessage { ToEmail = toEmail, ToName = displayName, Subject = subject, HtmlBody = html, TextBody = text };
    }

    public static EmailMessage CreatePasswordResetCodeEmail(string toEmail, string displayName, string code, DateTime expiresAtUtc)
    {
        var remaining = Math.Max(1, (int)Math.Ceiling((expiresAtUtc - DateTime.UtcNow).TotalMinutes));
        var subject = "GarajPass şifre sıfırlama kodu";
        var html = $"""
            <div style="font-family:Segoe UI,Arial,sans-serif;background:#f8fafc;padding:24px;color:#0f172a">
              <div style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:20px;padding:32px;border:1px solid #e2e8f0">
                <div style="font-size:13px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#f97316;margin-bottom:12px">GarajPass</div>
                <h1 style="font-size:24px;margin:0 0 12px">Şifre sıfırlama kodunuz</h1>
                <p style="font-size:15px;line-height:1.7;margin:0 0 20px">Merhaba {displayName},</p>
                <p style="font-size:15px;line-height:1.7;margin:0 0 16px">Şifreni sıfırlamak için doğrulama kodun:</p>
                <div style="font-size:36px;font-weight:800;letter-spacing:.22em;background:#0f172a;color:#f8fafc;border-radius:16px;padding:18px 24px;text-align:center;margin:0 0 20px">{code}</div>
                <p style="font-size:14px;line-height:1.7;margin:0 0 8px">Kod {remaining} dakika geçerlidir.</p>
                <p style="font-size:14px;line-height:1.7;color:#475569;margin:0">Bu işlemi sen başlatmadıysan bu e-postayı yok sayabilirsin.</p>
              </div>
            </div>
            """;

        var text = $"Merhaba {displayName}, şifreni sıfırlamak için doğrulama kodun: {code}. Kod {remaining} dakika geçerlidir. Bu işlemi sen başlatmadıysan bu e-postayı yok sayabilirsin.";
        return new EmailMessage { ToEmail = toEmail, ToName = displayName, Subject = subject, HtmlBody = html, TextBody = text };
    }

    public static EmailMessage CreateTwoFactorEmail(string toEmail, string displayName, string code, DateTime expiresAtUtc)
    {
        var remaining = Math.Max(1, (int)Math.Ceiling((expiresAtUtc - DateTime.UtcNow).TotalMinutes));
        var subject = "GarajPass doğrulama kodu";
        var html = $"""
            <div style="font-family:Segoe UI,Arial,sans-serif;background:#f8fafc;padding:24px;color:#0f172a">
              <div style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:20px;padding:32px;border:1px solid #e2e8f0">
                <div style="font-size:13px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#f97316;margin-bottom:12px">GarajPass</div>
                <h1 style="font-size:24px;margin:0 0 12px">Doğrulama kodunuz</h1>
                <p style="font-size:15px;line-height:1.7;margin:0 0 20px">Merhaba {displayName},</p>
                <p style="font-size:15px;line-height:1.7;margin:0 0 16px">Doğrulama kodunuz:</p>
                <div style="font-size:32px;font-weight:800;letter-spacing:.18em;background:#0f172a;color:#f8fafc;border-radius:16px;padding:18px 24px;text-align:center;margin:0 0 20px">{code}</div>
                <p style="font-size:14px;line-height:1.7;margin:0 0 8px">Kod {remaining} dakika geçerlidir.</p>
                <p style="font-size:14px;line-height:1.7;color:#475569;margin:0">Bu işlemi siz başlatmadıysanız hesabınızı kontrol edin.</p>
              </div>
            </div>
            """;

        var text = $"Merhaba {displayName}, doğrulama kodunuz: {code}. Kod {remaining} dakika geçerlidir.";
        return new EmailMessage { ToEmail = toEmail, ToName = displayName, Subject = subject, HtmlBody = html, TextBody = text };
    }

    public static EmailMessage CreateInvitationEmail(string toEmail, string tenantIdentifier, string inviteUrl, DateTime expiresAtUtc)
    {
        var remaining = Math.Max(1, (int)Math.Ceiling((expiresAtUtc - DateTime.UtcNow).TotalHours));
        return CreateMessage(
            toEmail,
            toEmail,
            "GarajPass'a davet edildiniz",
            "Ekibe katılmak için davet",
            $"Merhaba,",
            $"{tenantIdentifier} işletmesinin GarajPass hesabına katılmaya davet edildiniz.",
            "Daveti Kabul Et",
            inviteUrl,
            $"Bu bağlantı {remaining} saat geçerlidir. Bu daveti siz talep etmediyseniz e-postayı yok sayabilirsiniz.");
    }

    private static EmailMessage CreateMessage(
        string toEmail,
        string displayName,
        string subject,
        string title,
        string greeting,
        string description,
        string buttonText,
        string actionUrl,
        string footer)
    {
        var html = $"""
            <div style="font-family:Segoe UI,Arial,sans-serif;background:#f8fafc;padding:24px;color:#0f172a">
              <div style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:20px;padding:32px;border:1px solid #e2e8f0">
                <div style="font-size:13px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;color:#f97316;margin-bottom:12px">GarajPass</div>
                <h1 style="font-size:24px;margin:0 0 12px">{title}</h1>
                <p style="font-size:15px;line-height:1.7;margin:0 0 12px">{greeting}</p>
                <p style="font-size:15px;line-height:1.7;margin:0 0 24px">{description}</p>
                <a href="{actionUrl}" style="display:inline-block;background:#0f172a;color:#ffffff;text-decoration:none;padding:14px 20px;border-radius:14px;font-weight:700">{buttonText}</a>
                <p style="font-size:13px;line-height:1.7;color:#475569;margin:24px 0 0">Bağlantı çalışmazsa bu adresi kopyalayın:</p>
                <p style="font-size:13px;line-height:1.7;color:#2563eb;word-break:break-word;margin:8px 0 0">{actionUrl}</p>
                <p style="font-size:13px;line-height:1.7;color:#475569;margin:24px 0 0">{footer}</p>
              </div>
            </div>
            """;

        var text = string.Create(
            CultureInfo.InvariantCulture,
            $"{greeting} {description} {buttonText}: {actionUrl} {footer}");

        return new EmailMessage
        {
            ToEmail = toEmail,
            ToName = displayName,
            Subject = subject,
            HtmlBody = html,
            TextBody = text
        };
    }
}
