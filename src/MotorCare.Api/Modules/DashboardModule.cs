using Carter;
using MediatR;
using System.Security.Claims;
using MotorCare.Api.Authorization;
using MotorCare.Application.Dashboard.Queries.GetDailySummary;
using MotorCare.Application.ServiceOrders.Queries.GetOpenBalances;
using MotorCare.Application.ServiceOrders.Queries.GetPaymentSummary;
using MotorCare.Infrastructure.Security;

namespace MotorCare.Api.Modules;

public sealed class DashboardModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .WithOpenApi();

        group.MapGet("/daily", async (
            IMediator mediator,
            HttpContext httpContext,
            ILogger<DashboardModule> logger,
            CancellationToken ct) =>
        {
            try
            {
                var result = await mediator.Send(new GetDailySummaryQuery(), ct);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Dashboard daily summary failed. TraceId={TraceId} TenantId={TenantId} TenantIdentifier={TenantIdentifier} UserId={UserId} RequestPath={RequestPath} ExceptionType={ExceptionType} ExceptionMessage={ExceptionMessage} InnerException={InnerException}",
                    httpContext.TraceIdentifier,
                    httpContext.User.FindFirstValue(JwtTokenGenerator.TenantIdClaim),
                    httpContext.User.FindFirstValue(JwtTokenGenerator.TenantIdentifierClaim),
                    httpContext.User.FindFirstValue(JwtTokenGenerator.UserIdClaim),
                    httpContext.Request.Path.Value,
                    ex.GetType().Name,
                    ex.Message,
                    ex.InnerException?.Message);
                throw;
            }
        })
        .WithName("GetDailySummary")
        .RequireAuthorization(AuthorizationPolicies.DashboardRead)
        .Produces<DailySummaryDto>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/payment-summary", async (
            DateTimeOffset from,
            DateTimeOffset to,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPaymentSummaryQuery(from, to), ct);
            return Results.Ok(result);
        })
        .WithName("GetPaymentSummary")
        .RequireAuthorization(AuthorizationPolicies.DashboardRead)
        .Produces<PaymentSummaryDto>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/open-balances", async (
            int? take,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetOpenBalancesQuery(take ?? 50), ct);
            return Results.Ok(result);
        })
        .WithName("GetOpenBalances")
        .RequireAuthorization(AuthorizationPolicies.DashboardRead)
        .Produces<IReadOnlyList<OpenBalanceDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
