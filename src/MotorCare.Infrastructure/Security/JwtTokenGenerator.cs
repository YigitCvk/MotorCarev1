using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;

namespace MotorCare.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    public const string UserIdClaim = "userId";
    public const string TenantIdClaim = "tenantId";
    public const string TenantIdentifierClaim = "tenantIdentifier";
    public const string EmailClaim = "email";
    public const string RoleClaim = "role";

    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateAccessToken(User user, Tenant tenant)
    {
        var claims = new List<Claim>
        {
            new(UserIdClaim, user.Id.ToString()),
            new(TenantIdClaim, tenant.Id.ToString()),
            new(TenantIdentifierClaim, tenant.Identifier),
            new(EmailClaim, user.Email),
            new(RoleClaim, user.Role.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
