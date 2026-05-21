using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Infrastructure.Security;

namespace MotorCare.Infrastructure.Tenancy;

public class HeaderTenantProvider : ITenantProvider
{
    private const string HeaderName = "X-Tenant-Id";
    private const string AllowHeaderTenantFallbackKey = "Tenancy:AllowHeaderTenantFallback";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly bool _allowHeaderTenantFallback;

    public HeaderTenantProvider(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _httpContextAccessor = httpContextAccessor;
        _allowHeaderTenantFallback =
            !environment.IsProduction() &&
            configuration.GetValue<bool>(AllowHeaderTenantFallbackKey);
    }

    public string? GetTenantId()
    {
        var authenticatedTenant = _httpContextAccessor.HttpContext?.User
            .FindFirst(JwtTokenGenerator.TenantIdentifierClaim)?.Value;
        if (!string.IsNullOrWhiteSpace(authenticatedTenant))
        {
            return authenticatedTenant;
        }

        if (!_allowHeaderTenantFallback)
        {
            return null;
        }

        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(HeaderName, out var tenantId) == true)
        {
            var value = tenantId.ToString().Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }
}
