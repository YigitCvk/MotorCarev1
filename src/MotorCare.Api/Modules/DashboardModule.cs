using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Dashboard.Queries.GetDailySummary;
using MotorCare.Application.ServiceOrders.Queries.GetOpenBalances;
using MotorCare.Application.ServiceOrders.Queries.GetPaymentSummary;

namespace MotorCare.Api.Modules;

public sealed class DashboardModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .WithOpenApi();

        group.MapGet("/daily", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDailySummaryQuery(), ct);
            return Results.Ok(result);
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
