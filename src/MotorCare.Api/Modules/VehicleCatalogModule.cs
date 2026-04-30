using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Vehicles;
using MotorCare.Application.Vehicles.Queries.SearchMotorcycleBrands;
using MotorCare.Application.Vehicles.Queries.SearchMotorcycleModels;
using MotorCare.Application.Vehicles.Queries.SearchMotorcycles;

namespace MotorCare.Api.Modules;

public sealed class VehicleCatalogModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vehicle-catalog")
            .WithTags("VehicleCatalog")
            .WithOpenApi();

        group.MapGet("/motorcycles/brands", async (
            string? search,
            int? maxResults,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new SearchMotorcycleBrandsQuery(search, maxResults ?? 20), ct);
            return Results.Ok(result);
        })
        .WithName("SearchMotorcycleBrands")
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces<IReadOnlyList<string>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/motorcycles/models", async (
            string brand,
            string? search,
            int? maxResults,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new SearchMotorcycleModelsQuery(brand, search, maxResults ?? 20), ct);
            return Results.Ok(result);
        })
        .WithName("SearchMotorcycleModels")
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces<IReadOnlyList<MotorcycleCatalogSuggestionDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/motorcycles/search", async (
            string? query,
            int? maxResults,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new SearchMotorcyclesQuery(query, maxResults ?? 20), ct);
            return Results.Ok(result);
        })
        .WithName("SearchMotorcycles")
        .RequireAuthorization(AuthorizationPolicies.CustomerOperations)
        .Produces<IReadOnlyList<MotorcycleCatalogSuggestionDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }
}
