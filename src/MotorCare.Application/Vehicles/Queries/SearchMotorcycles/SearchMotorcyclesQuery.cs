using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Vehicles.Queries.SearchMotorcycles;

public sealed record SearchMotorcyclesQuery(string? Query, int MaxResults = 20) : IRequest<IReadOnlyList<MotorcycleCatalogSuggestionDto>>;

public sealed class SearchMotorcyclesQueryHandler : IRequestHandler<SearchMotorcyclesQuery, IReadOnlyList<MotorcycleCatalogSuggestionDto>>
{
    private readonly IMotorcycleModelCatalogRepository _repository;
    private readonly ILogger<SearchMotorcyclesQueryHandler> _logger;

    public SearchMotorcyclesQueryHandler(
        IMotorcycleModelCatalogRepository repository,
        ILogger<SearchMotorcyclesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MotorcycleCatalogSuggestionDto>> Handle(SearchMotorcyclesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.SearchAsync(request.Query, Math.Clamp(request.MaxResults, 1, 30), cancellationToken);

            _logger.LogInformation(
                EventIdStore.Vehicle.MotorcycleCatalogModelSearch,
                "Motorcycle catalog general search completed. Query={Query} ResultsCount={ResultsCount}",
                request.Query,
                items.Count);

            return items
                .Select(item => new MotorcycleCatalogSuggestionDto(
                    item.Id,
                    item.Brand,
                    item.Model,
                    $"{item.Brand} {item.Model}",
                    item.Segment,
                    item.EngineCc))
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                EventIdStore.Vehicle.MotorcycleCatalogSearchError,
                ex,
                "Motorcycle catalog general search failed. Query={Query}",
                request.Query);

            throw;
        }
    }
}
