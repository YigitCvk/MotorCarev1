using System.Security.Claims;
using MotorCare.Domain.Repositories;
using MotorCare.Infrastructure.Security;

namespace MotorCare.Api.Middleware;

public sealed class ActiveTenantUserGuardMiddleware
{
    private static readonly (string Method, string Path)[] PublicEndpoints =
    [
        ("POST", "/api/auth/login"),
        ("POST", "/api/auth/refresh-token"),
        ("POST", "/api/onboarding/tenant"),
        ("GET", "/health")
    ];

    private readonly RequestDelegate _next;

    public ActiveTenantUserGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantRepository tenantRepository,
        IUserRepository userRepository)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userIdValue = user.FindFirstValue(JwtTokenGenerator.UserIdClaim);
        var tenantIdValue = user.FindFirstValue(JwtTokenGenerator.TenantIdClaim);
        var tenantIdentifier = user.FindFirstValue(JwtTokenGenerator.TenantIdentifierClaim);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            !Guid.TryParse(tenantIdValue, out var tenantId) ||
            string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated token is missing required claims.");
            return;
        }

        var tenant = await tenantRepository.GetByIdAsync(tenantId, context.RequestAborted);
        if (tenant is null)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The tenant referenced by the token no longer exists.");
            return;
        }

        if (!tenant.IsActive)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "The tenant is inactive.");
            return;
        }

        if (!string.Equals(tenant.Identifier, tenantIdentifier, StringComparison.Ordinal))
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "The authenticated tenant context is invalid.");
            return;
        }

        var tenantUser = await userRepository.GetByIdAsync(userId, tenantIdentifier, context.RequestAborted);
        if (tenantUser is null)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The user referenced by the token no longer exists.");
            return;
        }

        if (!tenantUser.IsActive)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "The user is inactive.");
            return;
        }

        if (!string.Equals(tenantUser.TenantId, tenant.Identifier, StringComparison.Ordinal))
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "The user does not belong to the authenticated tenant.");
            return;
        }

        await _next(context);
    }

    private static bool ShouldSkip(HttpContext context)
    {
        var path = context.Request.Path;

        if (path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return PublicEndpoints.Any(endpoint =>
            string.Equals(context.Request.Method, endpoint.Method, StringComparison.OrdinalIgnoreCase) &&
            path.Equals(endpoint.Path, StringComparison.OrdinalIgnoreCase));
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(new
        {
            type = statusCode == StatusCodes.Status401Unauthorized
                ? "https://tools.ietf.org/html/rfc9110#section-15.5.2"
                : "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            title,
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier
        });
    }
}
