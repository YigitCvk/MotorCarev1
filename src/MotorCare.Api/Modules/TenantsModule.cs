using Carter;
using MediatR;
using MotorCare.Application.Tenants.Commands.CreateTenant;
using MotorCare.Application.Tenants.Queries.GetTenantByIdentifier;

namespace MotorCare.Api.Modules;

public sealed class TenantsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .WithOpenApi();

        group.MapPost("/", async (CreateTenantCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.CreatedAtRoute("GetTenantByIdentifier", new { identifier = command.Identifier }, id);
        })
        .WithName("CreateTenant")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{identifier}", async (string identifier, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTenantByIdentifierQuery(identifier), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetTenantByIdentifier")
        .Produces<TenantDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }
}
