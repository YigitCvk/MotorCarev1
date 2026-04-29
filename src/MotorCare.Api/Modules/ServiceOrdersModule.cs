using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Common.Models;
using MotorCare.Application.ServiceOrders.Commands.AddOperationToOrder;
using MotorCare.Application.ServiceOrders.Commands.AddPartToOrder;
using MotorCare.Application.ServiceOrders.Commands.AddPaymentToOrder;
using MotorCare.Application.ServiceOrders.Commands.CreateServiceOrder;
using MotorCare.Application.ServiceOrders.Commands.TrackConsumableCatalogUsage;
using MotorCare.Application.ServiceOrders.Commands.RemoveOperationFromOrder;
using MotorCare.Application.ServiceOrders.Commands.RemovePartFromOrder;
using MotorCare.Application.ServiceOrders.Commands.SetOrderDiscount;
using MotorCare.Application.ServiceOrders.Commands.UpdateServiceOrderStatus;
using MotorCare.Application.ServiceOrders;
using MotorCare.Application.ServiceOrders.Queries.SearchConsumableCatalog;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderById;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrders;
using MotorCare.Domain.Enums;

namespace MotorCare.Api.Modules;

public sealed class ServiceOrdersModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/service-orders")
            .WithTags("ServiceOrders")
            .WithOpenApi();

        group.MapPost("/", async (CreateServiceOrderCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.CreatedAtRoute("GetServiceOrderById", new { id }, id);
        })
        .WithName("CreateServiceOrder")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetServiceOrderByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetServiceOrderById")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<ServiceOrderDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", async (
            Guid? customerId,
            ServiceOrderStatus? status,
            string? q,
            DateTimeOffset? openedFrom,
            DateTimeOffset? openedTo,
            int? pageNumber,
            int? pageSize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetServiceOrdersQuery(customerId, status, q, openedFrom, openedTo, pageNumber ?? 1, pageSize ?? 20), ct);
            return Results.Ok(result);
        })
        .WithName("GetServiceOrders")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<PagedResult<ServiceOrderDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/consumable-catalog/search", async (
            string? query,
            string? category,
            int? maxResults,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new SearchConsumableCatalogQuery(query, category, maxResults ?? 20), ct);
            return Results.Ok(result);
        })
        .WithName("SearchConsumableCatalog")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<IReadOnlyList<ConsumableCatalogItemDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/consumable-catalog/track", async (
            TrackConsumableCatalogUsageRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            if (request.Items.Count > 0)
            {
                await mediator.Send(
                    new TrackConsumableCatalogUsageCommand(
                        request.Items.Select(x => new ConsumableCatalogItemInput(
                            x.Category, x.SubCategory, x.Brand, x.ProductName, x.Specification, x.Notes)).ToList()),
                    ct);
            }

            return Results.NoContent();
        })
        .WithName("TrackConsumableCatalogUsage")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/status", async (Guid id, UpdateServiceOrderStatusRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new UpdateServiceOrderStatusCommand(id, request.Status), ct);
            return Results.NoContent();
        })
        .WithName("UpdateServiceOrderStatus")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/operations", async (Guid id, AddOperationToOrderRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new AddOperationToOrderCommand(id, request.Description, request.Price), ct);
            return Results.NoContent();
        })
        .WithName("AddOperationToOrder")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/parts", async (Guid id, AddPartToOrderRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new AddPartToOrderCommand(id, request.PartName, request.PartNumber, request.UnitPrice, request.Quantity), ct);
            return Results.NoContent();
        })
        .WithName("AddPartToOrder")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/payments", async (Guid id, AddPaymentToOrderRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new AddPaymentToOrderCommand(id, request.Amount, request.Method, request.PaymentDate), ct);
            return Results.NoContent();
        })
        .WithName("AddPaymentToOrder")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderPayments)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/discount", async (Guid id, SetOrderDiscountRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new SetOrderDiscountCommand(id, request.Discount), ct);
            return Results.NoContent();
        })
        .WithName("SetOrderDiscount")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderPayments)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}/operations/{operationId:guid}", async (Guid id, Guid operationId, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new RemoveOperationFromOrderCommand(id, operationId), ct);
            return Results.NoContent();
        })
        .WithName("RemoveOperationFromOrder")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}/parts/{partId:guid}", async (Guid id, Guid partId, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new RemovePartFromOrderCommand(id, partId), ct);
            return Results.NoContent();
        })
        .WithName("RemovePartFromOrder")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public sealed record UpdateServiceOrderStatusRequest(ServiceOrderStatus Status);

    public sealed record AddOperationToOrderRequest(string Description, decimal Price);

    public sealed record AddPartToOrderRequest(string PartName, string? PartNumber, decimal UnitPrice, int Quantity);

    public sealed record AddPaymentToOrderRequest(decimal Amount, PaymentMethod Method, DateTimeOffset? PaymentDate);

    public sealed record SetOrderDiscountRequest(decimal Discount);

    public sealed record TrackConsumableCatalogItemRequest(string Category, string? SubCategory, string Brand, string ProductName, string? Specification, string? Notes);

    public sealed record TrackConsumableCatalogUsageRequest(IReadOnlyList<TrackConsumableCatalogItemRequest> Items);

    public sealed record SearchConsumableCatalogRequest(string? Query, string? Category, int? MaxResults);
}
