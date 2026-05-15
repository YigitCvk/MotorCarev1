using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Common.Models;
using MotorCare.Application.Services;
using MotorCare.Application.Services.Commands.ActivateServiceCatalogItem;
using MotorCare.Application.Services.Commands.CreateServiceCatalogItem;
using MotorCare.Application.Services.Commands.DeactivateServiceCatalogItem;
using MotorCare.Application.Services.Commands.UpdateServiceCatalogItem;
using MotorCare.Application.Services.Queries.GetServiceCatalogItemById;
using MotorCare.Application.Services.Queries.GetServiceCatalogItems;
using MotorCare.Domain.Enums;

namespace MotorCare.Api.Modules;

public sealed class ServicesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/services")
            .WithTags("Services")
            .WithOpenApi();

        group.MapGet("/", async (
            string? q,
            ServiceCategory? category,
            bool? isActive,
            int? pageNumber,
            int? pageSize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new GetServiceCatalogItemsQuery(q, category, isActive, pageNumber ?? 1, pageSize ?? 20),
                ct);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.CustomerRead)
        .Produces<PagedResult<ServiceCatalogItemDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetServiceCatalogItemByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.CustomerRead)
        .Produces<ServiceCatalogItemDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/", async (CreateServiceCatalogItemRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(
                new CreateServiceCatalogItemCommand(
                    request.Name,
                    request.Category,
                    request.Description,
                    request.DefaultDurationMinutes,
                    request.EffectivePrice,
                    request.Currency,
                    request.IsActive),
                ct);

            return Results.Created($"/api/services/{id}", id);
        })
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", async (Guid id, UpdateServiceCatalogItemRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(
                new UpdateServiceCatalogItemCommand(
                    id,
                    request.Name,
                    request.Category,
                    request.Description,
                    request.DefaultDurationMinutes,
                    request.EffectivePrice,
                    request.Currency,
                    request.IsActive),
                ct);

            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/activate", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new ActivateServiceCatalogItemCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/deactivate", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeactivateServiceCatalogItemCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public sealed class CreateServiceCatalogItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public ServiceCategory Category { get; set; }
        public string? Description { get; set; }
        public int DefaultDurationMinutes { get; set; }
        public decimal DefaultPrice { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool IsActive { get; set; } = true;
        public decimal EffectivePrice => Price != 0 ? Price : DefaultPrice;
    }

    public sealed class UpdateServiceCatalogItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public ServiceCategory Category { get; set; }
        public string? Description { get; set; }
        public int DefaultDurationMinutes { get; set; }
        public decimal DefaultPrice { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool IsActive { get; set; }
        public decimal EffectivePrice => Price != 0 ? Price : DefaultPrice;
    }
}
