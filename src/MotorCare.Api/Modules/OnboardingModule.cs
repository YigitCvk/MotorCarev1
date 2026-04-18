using Carter;
using MediatR;
using MotorCare.Application.Tenants.Commands.CreateTenantWithOwner;

namespace MotorCare.Api.Modules;

public sealed class OnboardingModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/onboarding")
            .WithTags("Onboarding")
            .WithOpenApi();

        group.MapPost("/tenant", async (CreateTenantWithOwnerCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/tenants/{result.TenantIdentifier}", result);
        })
        .WithName("CreateTenantWithOwner")
        .Produces<CreateTenantWithOwnerResultDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }
}
