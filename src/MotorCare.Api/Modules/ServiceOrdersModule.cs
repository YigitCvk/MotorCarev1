using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MotorCare.Api.Authorization;
using MotorCare.Api.Files;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;
using MotorCare.Application.PublicRecords;
using MotorCare.Application.ServiceOrders.Commands.DeleteServiceOrderAttachment;
using MotorCare.Application.ServiceOrders.Commands.AddOperationToOrder;
using MotorCare.Application.ServiceOrders.Commands.AddPartToOrder;
using MotorCare.Application.ServiceOrders.Commands.AddPaymentToOrder;
using MotorCare.Application.ServiceOrders.Commands.CreateServiceOrder;
using MotorCare.Application.ServiceOrders.Commands.TrackConsumableCatalogUsage;
using MotorCare.Application.ServiceOrders.Commands.RemoveOperationFromOrder;
using MotorCare.Application.ServiceOrders.Commands.RemovePartFromOrder;
using MotorCare.Application.ServiceOrders.Commands.SetOrderDiscount;
using MotorCare.Application.ServiceOrders.Commands.UpdateServiceOrderStatus;
using MotorCare.Application.ServiceOrders.Commands.UploadServiceOrderAttachment;
using MotorCare.Application.ServiceOrders;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderAttachmentDownload;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderAttachments;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderActivityFeed;
using MotorCare.Application.ServiceOrders.Queries.SearchConsumableCatalog;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderById;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrders;
using MotorCare.Application.ServiceOrders.Queries.GetServiceOrderStatusHistory;
using MotorCare.Domain.Enums;
using MotorCare.Domain.PublicRecords;

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

        group.MapGet("/{id:guid}/status-history", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetServiceOrderStatusHistoryQuery(id), ct);
            return Results.Ok(result);
        })
        .WithName("GetServiceOrderStatusHistory")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<IReadOnlyList<ServiceOrderStatusHistoryDto>>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/public-access", async (
            Guid id,
            IPublicRecordAccessService publicRecordAccessService,
            ITenantProvider tenantProvider,
            CancellationToken ct) =>
        {
            var tenantId = tenantProvider.GetTenantId();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Results.Unauthorized();
            }

            var access = await publicRecordAccessService.GetOrCreateForServiceOrderAsync(id, tenantId, ct);
            return access is null ? Results.NotFound() : Results.Ok(access);
        })
        .WithName("GetOrCreateServiceOrderPublicAccess")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<PublicRecordAccessDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}/public-access", async (
            Guid id,
            IPublicRecordAccessService publicRecordAccessService,
            ITenantProvider tenantProvider,
            CancellationToken ct) =>
        {
            var tenantId = tenantProvider.GetTenantId();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Results.Unauthorized();
            }

            var access = await publicRecordAccessService.GetForRecordAsync(PublicRecordType.ServiceOrder, id, tenantId, ct);
            return access is null ? Results.NotFound() : Results.Ok(access);
        })
        .WithName("GetServiceOrderPublicAccess")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<PublicRecordAccessDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/public-access/enable", async (
            Guid id,
            IPublicRecordAccessService publicRecordAccessService,
            ITenantProvider tenantProvider,
            CancellationToken ct) =>
        {
            var tenantId = tenantProvider.GetTenantId();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Results.Unauthorized();
            }

            var access = await publicRecordAccessService.EnableAsync(PublicRecordType.ServiceOrder, id, tenantId, ct);
            return access is null ? Results.NotFound() : Results.Ok(access);
        })
        .WithName("EnableServiceOrderPublicAccess")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<PublicRecordAccessDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/public-access/disable", async (
            Guid id,
            IPublicRecordAccessService publicRecordAccessService,
            ITenantProvider tenantProvider,
            CancellationToken ct) =>
        {
            var tenantId = tenantProvider.GetTenantId();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return Results.Unauthorized();
            }

            await publicRecordAccessService.DisableAsync(PublicRecordType.ServiceOrder, id, tenantId, ct);
            return Results.NoContent();
        })
        .WithName("DisableServiceOrderPublicAccess")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}/activity-feed", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetServiceOrderActivityFeedQuery(id), ct);
            return Results.Ok(result);
        })
        .WithName("GetServiceOrderActivityFeed")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<IReadOnlyList<ServiceOrderActivityFeedItem>>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}/attachments", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetServiceOrderAttachmentsQuery(id), ct);
            return Results.Ok(result);
        })
        .WithName("GetServiceOrderAttachments")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces<IReadOnlyList<ServiceOrderAttachmentDto>>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/attachments", async (Guid id, HttpRequest request, IMediator mediator, CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest("multipart/form-data bekleniyor.");
            }

            if (request.ContentLength > ServiceOrderAttachmentFileRules.MaxMultipartBodySizeBytes)
            {
                return Results.BadRequest("Dosya boyutu 5 MB sınırını aşamaz.");
            }

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("File");
            if (file is null)
            {
                return Results.BadRequest("File alanı zorunludur.");
            }

            var attachmentTypeValue = form["AttachmentType"].ToString();
            if (!Enum.TryParse<ServiceOrderAttachmentType>(attachmentTypeValue, ignoreCase: true, out var attachmentType) ||
                !Enum.IsDefined(attachmentType))
            {
                return Results.BadRequest("Dosya türü geçersiz.");
            }

            var description = form["Description"].ToString();
            var result = await mediator.Send(
                new UploadServiceOrderAttachmentCommand(
                    id,
                    new FormFileUpload(file),
                    attachmentType,
                    string.IsNullOrWhiteSpace(description) ? null : description),
                ct);

            return Results.Created(result.FileUrl, result);
        })
        .WithName("UploadServiceOrderAttachment")
        .Accepts<ServiceOrderAttachmentFormRequest>("multipart/form-data")
        .WithMetadata(
            new RequestSizeLimitAttribute(ServiceOrderAttachmentFileRules.MaxMultipartBodySizeBytes),
            new RequestFormLimitsAttribute
            {
                MultipartBodyLengthLimit = ServiceOrderAttachmentFileRules.MaxFileSizeBytes,
                ValueLengthLimit = ServiceOrderAttachmentFileRules.MaxDescriptionLength
            })
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces<ServiceOrderAttachmentDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}/attachments/{attachmentId:guid}/download", async (
            Guid id,
            Guid attachmentId,
            bool? download,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetServiceOrderAttachmentDownloadQuery(id, attachmentId), ct);
            if (result is null)
            {
                return Results.NotFound();
            }

            return Results.File(
                result.Stream,
                result.ContentType,
                fileDownloadName: download == true ? result.OriginalFileName : null,
                enableRangeProcessing: true);
        })
        .WithName("DownloadServiceOrderAttachment")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead)
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}/attachments/{attachmentId:guid}", async (Guid id, Guid attachmentId, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteServiceOrderAttachmentCommand(id, attachmentId), ct);
            return Results.NoContent();
        })
        .WithName("DeleteServiceOrderAttachment")
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
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
            await mediator.Send(new UpdateServiceOrderStatusCommand(id, request.Status, request.Note), ct);
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

    public sealed record UpdateServiceOrderStatusRequest(ServiceOrderStatus Status, string? Note = null);

    public sealed record AddOperationToOrderRequest(string Description, decimal Price);

    public sealed record AddPartToOrderRequest(string PartName, string? PartNumber, decimal UnitPrice, int Quantity);

    public sealed record AddPaymentToOrderRequest(decimal Amount, PaymentMethod Method, DateTimeOffset? PaymentDate);

    public sealed record SetOrderDiscountRequest(decimal Discount);

    public sealed record ServiceOrderAttachmentFormRequest(IFormFile File, ServiceOrderAttachmentType AttachmentType, string? Description);

    public sealed record TrackConsumableCatalogItemRequest(string Category, string? SubCategory, string Brand, string ProductName, string? Specification, string? Notes);

    public sealed record TrackConsumableCatalogUsageRequest(IReadOnlyList<TrackConsumableCatalogItemRequest> Items);

    public sealed record SearchConsumableCatalogRequest(string? Query, string? Category, int? MaxResults);
}
