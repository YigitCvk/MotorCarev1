using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Common.Models;
using MotorCare.Application.Inventory;
using MotorCare.Application.Inventory.Commands.ActivateInventoryItem;
using MotorCare.Application.Inventory.Commands.AdjustInventoryStock;
using MotorCare.Application.Inventory.Commands.CreateInventoryItem;
using MotorCare.Application.Inventory.Commands.DeactivateInventoryItem;
using MotorCare.Application.Inventory.Commands.UpdateInventoryItem;
using MotorCare.Application.Inventory.Queries.GetInventoryItemById;
using MotorCare.Application.Inventory.Queries.GetInventoryItems;

namespace MotorCare.Api.Modules;

public sealed class InventoryModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .WithOpenApi()
            .RequireAuthorization(AuthorizationPolicies.CustomerOperations);

        group.MapGet("/", async (
            string? q,
            string? category,
            bool? isActive,
            bool? lowStockOnly,
            int? pageNumber,
            int? pageSize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new GetInventoryItemsQuery(q, category, isActive, lowStockOnly ?? false, pageNumber ?? 1, pageSize ?? 20),
                ct);

            return Results.Ok(result);
        })
        .Produces<PagedResult<InventoryItemDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetInventoryItemByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .Produces<InventoryItemDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/", async (CreateInventoryItemRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(
                new CreateInventoryItemCommand(
                    request.Name,
                    request.Sku,
                    request.Barcode,
                    request.Category,
                    request.Brand,
                    request.Unit,
                    request.UnitPrice,
                    request.StockQuantity,
                    request.MinimumStockLevel,
                    request.IsActive),
                ct);

            return Results.Created($"/api/inventory/{id}", id);
        })
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", async (Guid id, UpdateInventoryItemRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(
                new UpdateInventoryItemCommand(
                    id,
                    request.Name,
                    request.Sku,
                    request.Barcode,
                    request.Category,
                    request.Brand,
                    request.Unit,
                    request.UnitPrice,
                    request.StockQuantity,
                    request.MinimumStockLevel,
                    request.IsActive),
                ct);

            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/activate", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new ActivateInventoryItemCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPut("/{id:guid}/deactivate", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeactivateInventoryItemCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/adjust-stock", async (Guid id, AdjustInventoryStockRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new AdjustInventoryStockCommand(id, request.QuantityDelta, request.Reason), ct);
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public sealed record CreateInventoryItemRequest(
        string Name,
        string? Sku,
        string? Barcode,
        string? Category,
        string? Brand,
        string Unit,
        decimal UnitPrice,
        decimal StockQuantity,
        decimal MinimumStockLevel,
        bool IsActive);

    public sealed record UpdateInventoryItemRequest(
        string Name,
        string? Sku,
        string? Barcode,
        string? Category,
        string? Brand,
        string Unit,
        decimal UnitPrice,
        decimal StockQuantity,
        decimal MinimumStockLevel,
        bool IsActive);

    public sealed record AdjustInventoryStockRequest(decimal QuantityDelta, string Reason);
}
