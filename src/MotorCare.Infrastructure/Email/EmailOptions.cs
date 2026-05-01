namespace MotorCare.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Smtp";
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "GarajPass";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public bool UseDefaultCredentials { get; set; }
    public string AppBaseUrl { get; set; } = string.Empty;
    public bool SendEmails { get; set; } = true;
    public bool LogEmailBodyInDevelopment { get; set; } = true;
}
