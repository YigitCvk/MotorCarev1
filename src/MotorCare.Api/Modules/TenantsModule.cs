using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Tenants.Commands.UpdateCurrentTenantProfile;
using MotorCare.Application.Tenants.Queries.GetCurrentTenantProfile;

namespace MotorCare.Api.Modules;

public sealed class TenantsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .WithOpenApi();

        group.MapGet("/current/profile", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCurrentTenantProfileQuery(), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetCurrentTenantProfile")
        .RequireAuthorization()
        .Produces<TenantProfileDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/current/profile", async (
            UpdateCurrentTenantProfileCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("UpdateCurrentTenantProfile")
        .RequireAuthorization(AuthorizationPolicies.TenantManagement)
        .Produces<TenantProfileDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }
}
