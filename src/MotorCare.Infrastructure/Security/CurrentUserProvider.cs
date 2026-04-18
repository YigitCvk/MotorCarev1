using Microsoft.AspNetCore.Http;
using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Infrastructure.Security;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirst(JwtTokenGenerator.UserIdClaim)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public string? GetTenantId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(JwtTokenGenerator.TenantIdClaim)?.Value;
    }

    public string? GetTenantIdentifier()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(JwtTokenGenerator.TenantIdentifierClaim)?.Value;
    }

    public string? GetEmail()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(JwtTokenGenerator.EmailClaim)?.Value;
    }

    public string? GetRole()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(JwtTokenGenerator.RoleClaim)?.Value;
    }
}
