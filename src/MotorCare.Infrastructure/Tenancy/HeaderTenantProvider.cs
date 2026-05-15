using Microsoft.AspNetCore.Http;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Infrastructure.Security;

namespace MotorCare.Infrastructure.Tenancy;

public class HeaderTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetTenantId()
    {
        var authenticatedTenant = _httpContextAccessor.HttpContext?.User
            .FindFirst(JwtTokenGenerator.TenantIdentifierClaim)?.Value;
        if (!string.IsNullOrWhiteSpace(authenticatedTenant))
        {
            return authenticatedTenant;
        }

        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId) == true)
        {
            return tenantId.ToString();
        }

        return null;
    }
}
