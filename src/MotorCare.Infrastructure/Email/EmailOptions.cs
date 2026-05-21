using System.Net.Mail;

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

    public static IReadOnlyList<string> ValidateForEnvironment(EmailOptions? options, string environmentName)
    {
        if (!string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase))
        {
            return Array.Empty<string>();
        }

        var failures = new List<string>();

        if (options is null)
        {
            failures.Add("Email section is required in Production.");
            return failures;
        }

        if (!options.SendEmails)
        {
            failures.Add("Email:SendEmails must be true in Production.");
        }

        if (!string.Equals(options.Provider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Email:Provider must be Smtp in Production.");
        }

        if (string.IsNullOrWhiteSpace(options.FromEmail))
        {
            failures.Add("Email:FromEmail is required in Production.");
        }
        else if (!IsValidEmail(options.FromEmail))
        {
            failures.Add("Email:FromEmail must be a valid email address in Production.");
        }

        if (string.IsNullOrWhiteSpace(options.SmtpHost))
        {
            failures.Add("Email:SmtpHost is required in Production.");
        }

        if (options.SmtpPort is < 1 or > 65535)
        {
            failures.Add("Email:SmtpPort must be between 1 and 65535 in Production.");
        }

        if (!options.UseDefaultCredentials)
        {
            if (string.IsNullOrWhiteSpace(options.SmtpUsername))
            {
                failures.Add("Email:SmtpUsername is required in Production when default credentials are disabled.");
            }

            if (string.IsNullOrWhiteSpace(options.SmtpPassword))
            {
                failures.Add("Email:SmtpPassword is required in Production when default credentials are disabled.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.AppBaseUrl) ||
            !Uri.TryCreate(options.AppBaseUrl, UriKind.Absolute, out var appBaseUri) ||
            string.IsNullOrWhiteSpace(appBaseUri.Host))
        {
            failures.Add("Email:AppBaseUrl must be an absolute URL in Production.");
        }

        return failures;
    }

    public static void ThrowIfInvalidForEnvironment(EmailOptions? options, string environmentName)
    {
        var failures = ValidateForEnvironment(options, environmentName);
        if (failures.Count > 0)
        {
            throw new InvalidOperationException($"Email configuration is invalid: {string.Join("; ", failures)}");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
