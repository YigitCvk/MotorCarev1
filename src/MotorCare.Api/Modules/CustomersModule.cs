using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Customers.Commands.CreateCustomer;
using MotorCare.Application.Customers.Commands.UpdateCustomer;
using MotorCare.Application.Customers.Queries.GetCustomerById;
using MotorCare.Application.Customers.Queries.SearchCustomers;

namespace MotorCare.Api.Modules;

public sealed class CustomersModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers")
            .WithOpenApi();

        group.MapPost("/", async (CreateCustomerCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.CreatedAtRoute("GetCustomerById", new { id }, id);
        })
        .WithName("CreateCustomer")
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new UpdateCustomerCommand(
                id,
                request.FullName,
                request.Phone,
                request.Email,
                request.Whatsapp,
                request.Notes);

            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("UpdateCustomer")
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCustomerByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetCustomerById")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<CustomerDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", async (string? q, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SearchCustomersQuery(q), ct);
            return Results.Ok(result);
        })
        .WithName("SearchCustomers")
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces<IReadOnlyList<CustomerDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public sealed record UpdateCustomerRequest(
        string FullName,
        string Phone,
        string? Email,
        string? Whatsapp,
        string? Notes);
}
