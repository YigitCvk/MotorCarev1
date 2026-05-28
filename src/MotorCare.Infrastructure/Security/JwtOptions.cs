using System.Text;

namespace MotorCare.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;

    public static IReadOnlyList<string> Validate(JwtOptions? options)
    {
        var failures = new List<string>();

        if (options is null)
        {
            failures.Add("Jwt section is required.");
            return failures;
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Jwt:Audience is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Key))
        {
            failures.Add("Jwt:Key is required.");
        }
        else if (Encoding.UTF8.GetByteCount(options.Key) < 32)
        {
            failures.Add("Jwt:Key must be at least 32 bytes for HS256 signing.");
        }

        if (options.AccessTokenMinutes <= 0)
        {
            failures.Add("Jwt:AccessTokenMinutes must be greater than zero.");
        }

        if (options.RefreshTokenDays <= 0)
        {
            failures.Add("Jwt:RefreshTokenDays must be greater than zero.");
        }

        return failures;
    }

    public static void ThrowIfInvalid(JwtOptions? options)
    {
        var failures = Validate(options);
        if (failures.Count > 0)
        {
            throw new InvalidOperationException($"JWT configuration is invalid: {string.Join("; ", failures)}");
        }
    }
}
