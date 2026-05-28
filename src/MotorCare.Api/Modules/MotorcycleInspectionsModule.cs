using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Common.Models;
using MotorCare.Application.Inspections;
using MotorCare.Application.Inspections.Commands.CancelMotorcycleInspection;
using MotorCare.Application.Inspections.Commands.CompleteMotorcycleInspection;
using MotorCare.Application.Inspections.Commands.CreateMotorcycleInspection;
using MotorCare.Application.Inspections.Commands.UpdateMotorcycleInspection;
using MotorCare.Application.Inspections.Commands.UpdateMotorcycleInspectionItem;
using MotorCare.Application.Inspections.Queries.GetMotorcycleInspectionById;
using MotorCare.Application.Inspections.Queries.GetMotorcycleInspections;
using MotorCare.Application.PublicRecords;
using MotorCare.Domain.Enums;
using MotorCare.Domain.PublicRecords;
using MotorCare.Domain.Repositories;
using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Api.Modules;

public sealed class MotorcycleInspectionsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inspections")
            .WithTags("Inspections")
            .WithOpenApi();

        group.MapGet("/", async (
            string? q,
            MotorcycleInspectionPackageType? packageType,
            MotorcycleInspectionStatus? status,
            Guid? customerId,
            Guid? vehicleId,
            DateTimeOffset? createdFrom,
            DateTimeOffset? createdTo,
            int? pageNumber,
            int? pageSize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new GetMotorcycleInspectionsQuery(
                    q,
                    packageType,
                    status,
                    customerId,
                    vehicleId,
                    createdFrom,
                    createdTo,
                    pageNumber ?? 1,
                    pageSize ?? 20),
                ct);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionRead)
        .Produces<PagedResult<MotorcycleInspectionListItemDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetMotorcycleInspectionByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionRead)
        .Produces<MotorcycleInspectionDto>()
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

            var access = await publicRecordAccessService.GetOrCreateForInspectionAsync(id, tenantId, ct);
            return access is null ? Results.NotFound() : Results.Ok(access);
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionWrite)
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

            var access = await publicRecordAccessService.GetForRecordAsync(PublicRecordType.MotorcycleInspection, id, tenantId, ct);
            return access is null ? Results.NotFound() : Results.Ok(access);
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionRead)
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

            var access = await publicRecordAccessService.EnableAsync(PublicRecordType.MotorcycleInspection, id, tenantId, ct);
            return access is null ? Results.NotFound() : Results.Ok(access);
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionWrite)
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

            await publicRecordAccessService.DisableAsync(PublicRecordType.MotorcycleInspection, id, tenantId, ct);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/", async (
            CreateMotorcycleInspectionRequest request,
            IMediator mediator,
            IMotorcycleInspectionRepository repository,
            ITenantProvider tenantProvider,
            CancellationToken ct) =>
        {
            var id = await mediator.Send(
                new CreateMotorcycleInspectionCommand(
                    request.CustomerId,
                    request.VehicleId,
                    request.CustomerName,
                    request.Phone,
                    request.Plate,
                    request.Brand,
                    request.Model,
                    request.Year,
                    request.Mileage,
                    request.ChassisNumber,
                    request.EngineNumber,
                    request.Query5664,
                    request.MileageQuery,
                    request.PackageType,
                    request.GeneralNotes,
                    request.TestRideNotes,
                    request.CosmeticNotes),
                ct);

            var tenantId = tenantProvider.GetTenantId();
            var inspection = string.IsNullOrWhiteSpace(tenantId)
                ? null
                : await repository.GetByIdAsync(id, tenantId, ct);

            return Results.Created($"/api/inspections/{id}", new CreateMotorcycleInspectionResponse(
                id,
                inspection?.InspectionNo ?? string.Empty));
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionWrite)
        .Produces<CreateMotorcycleInspectionResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", async (Guid id, UpdateMotorcycleInspectionRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(
                new UpdateMotorcycleInspectionCommand(
                    id,
                    request.CustomerId,
                    request.VehicleId,
                    request.CustomerName,
                    request.Phone,
                    request.Plate,
                    request.Brand,
                    request.Model,
                    request.Year,
                    request.Mileage,
                    request.ChassisNumber,
                    request.EngineNumber,
                    request.Query5664,
                    request.MileageQuery,
                    request.PackageType,
                    request.GeneralNotes,
                    request.TestRideNotes,
                    request.CosmeticNotes),
                ct);

            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, UpdateMotorcycleInspectionItemRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new UpdateMotorcycleInspectionItemCommand(id, itemId, request.Result, request.Notes), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/complete", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new CompleteMotorcycleInspectionCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionWrite);

        group.MapPut("/{id:guid}/cancel", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new CancelMotorcycleInspectionCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.InspectionWrite);
    }

    public sealed record CreateMotorcycleInspectionRequest(
        Guid? CustomerId,
        Guid? VehicleId,
        string CustomerName,
        string Phone,
        string Plate,
        string? Brand,
        string? Model,
        int? Year,
        int? Mileage,
        string? ChassisNumber,
        string? EngineNumber,
        string? Query5664,
        string? MileageQuery,
        MotorcycleInspectionPackageType PackageType,
        string? GeneralNotes,
        string? TestRideNotes,
        string? CosmeticNotes);

    public sealed record UpdateMotorcycleInspectionRequest(
        Guid? CustomerId,
        Guid? VehicleId,
        string CustomerName,
        string Phone,
        string Plate,
        string? Brand,
        string? Model,
        int? Year,
        int? Mileage,
        string? ChassisNumber,
        string? EngineNumber,
        string? Query5664,
        string? MileageQuery,
        MotorcycleInspectionPackageType PackageType,
        string? GeneralNotes,
        string? TestRideNotes,
        string? CosmeticNotes);

    public sealed record UpdateMotorcycleInspectionItemRequest(
        MotorcycleInspectionResult Result,
        string? Notes);

    public sealed record CreateMotorcycleInspectionResponse(
        Guid Id,
        string InspectionNo);
}
