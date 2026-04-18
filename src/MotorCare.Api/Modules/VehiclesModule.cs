using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Vehicles.Commands.CreateVehicle;
using MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

namespace MotorCare.Api.Modules;

public sealed class VehiclesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vehicles")
            .WithTags("Vehicles")
            .WithOpenApi();

        group.MapPost("/", async (CreateVehicleCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.CreatedAtRoute("GetVehicleByPlate", new { plate = command.Plate }, id);
        })
        .WithName("CreateVehicle")
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{plate}", async (string plate, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetVehicleByPlateQuery(plate), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetVehicleByPlate")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<VehicleDto>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
