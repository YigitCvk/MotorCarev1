using System.Web;
using MotorCare.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace MotorCare.Infrastructure.Email;

public sealed class AuthLinkBuilder : IAuthLinkBuilder
{
    private readonly EmailOptions _options;

    public AuthLinkBuilder(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public string BuildEmailVerificationUrl(string email, string token)
        => BuildUrl("/verify-email", email, token);

    public string BuildInviteUrl(string token)
    {
        var baseUrl = (_options.AppBaseUrl ?? string.Empty).TrimEnd('/');
        return $"{baseUrl}/accept-invite?token={HttpUtility.UrlEncode(token)}";
    }

    private string BuildUrl(string path, string email, string token)
    {
        var baseUrl = (_options.AppBaseUrl ?? string.Empty).TrimEnd('/');
        return $"{baseUrl}{path}?email={HttpUtility.UrlEncode(email)}&token={HttpUtility.UrlEncode(token)}";
    }
}
