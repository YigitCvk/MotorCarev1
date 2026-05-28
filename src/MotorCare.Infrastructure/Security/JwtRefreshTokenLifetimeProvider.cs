using Microsoft.Extensions.Options;
using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Infrastructure.Security;

public sealed class JwtRefreshTokenLifetimeProvider : IRefreshTokenLifetimeProvider
{
    private readonly JwtOptions _options;

    public JwtRefreshTokenLifetimeProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public DateTimeOffset GetExpiresAt(DateTimeOffset issuedAt)
    {
        return issuedAt.AddDays(_options.RefreshTokenDays);
    }
}
