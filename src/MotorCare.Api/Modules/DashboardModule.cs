using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Dashboard.Queries.GetDailySummary;

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
    }
}
