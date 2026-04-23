using Serilog.Context;
using MotorCare.Infrastructure.Security;

namespace MotorCare.Api.Middleware;

public sealed class UserContextLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userId           = user.FindFirst(JwtTokenGenerator.UserIdClaim)?.Value;
        var tenantId         = user.FindFirst(JwtTokenGenerator.TenantIdClaim)?.Value;
        var tenantIdentifier = user.FindFirst(JwtTokenGenerator.TenantIdentifierClaim)?.Value;
        var role             = user.FindFirst(JwtTokenGenerator.RoleClaim)?.Value;

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("TenantIdentifier", tenantIdentifier))
        using (LogContext.PushProperty("UserRole", role))
        {
            await _next(context);
        }
    }
}
