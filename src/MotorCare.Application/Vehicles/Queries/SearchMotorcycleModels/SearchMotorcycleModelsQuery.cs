using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Vehicles.Queries.SearchMotorcycleModels;

public sealed record SearchMotorcycleModelsQuery(string Brand, string? Search, int MaxResults = 20) : IRequest<IReadOnlyList<MotorcycleCatalogSuggestionDto>>;

public sealed class SearchMotorcycleModelsQueryHandler : IRequestHandler<SearchMotorcycleModelsQuery, IReadOnlyList<MotorcycleCatalogSuggestionDto>>
{
    private readonly IMotorcycleModelCatalogRepository _repository;
    private readonly ILogger<SearchMotorcycleModelsQueryHandler> _logger;

    public SearchMotorcycleModelsQueryHandler(
        IMotorcycleModelCatalogRepository repository,
        ILogger<SearchMotorcycleModelsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MotorcycleCatalogSuggestionDto>> Handle(SearchMotorcycleModelsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.SearchModelsAsync(request.Brand, request.Search, Math.Clamp(request.MaxResults, 1, 30), cancellationToken);

            _logger.LogInformation(
                EventIdStore.Vehicle.MotorcycleCatalogModelSearch,
                "Motorcycle model search completed. Brand={Brand} Search={Search} ResultsCount={ResultsCount}",
                request.Brand,
                request.Search,
                items.Count);

            return items
                .Select(Map)
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                EventIdStore.Vehicle.MotorcycleCatalogSearchError,
                ex,
                "Motorcycle model search failed. Brand={Brand} Search={Search}",
                request.Brand,
                request.Search);

            throw;
        }
    }

    private static MotorcycleCatalogSuggestionDto Map(Domain.Vehicles.MotorcycleModelCatalogItem item)
        => new(
            item.Id,
            item.Brand,
            item.Model,
            $"{item.Brand} {item.Model}",
            item.Segment,
            item.EngineCc);
}
