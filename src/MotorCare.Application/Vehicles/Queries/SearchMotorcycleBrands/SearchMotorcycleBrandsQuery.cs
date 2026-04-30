using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Vehicles.Queries.SearchMotorcycleBrands;

public sealed record SearchMotorcycleBrandsQuery(string? Search, int MaxResults = 20) : IRequest<IReadOnlyList<string>>;

public sealed class SearchMotorcycleBrandsQueryHandler : IRequestHandler<SearchMotorcycleBrandsQuery, IReadOnlyList<string>>
{
    private readonly IMotorcycleModelCatalogRepository _repository;
    private readonly ILogger<SearchMotorcycleBrandsQueryHandler> _logger;

    public SearchMotorcycleBrandsQueryHandler(
        IMotorcycleModelCatalogRepository repository,
        ILogger<SearchMotorcycleBrandsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> Handle(SearchMotorcycleBrandsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var brands = await _repository.SearchBrandsAsync(request.Search, Math.Clamp(request.MaxResults, 1, 30), cancellationToken);

            _logger.LogInformation(
                EventIdStore.Vehicle.MotorcycleCatalogBrandSearch,
                "Motorcycle brand search completed. Search={Search} ResultsCount={ResultsCount}",
                request.Search,
                brands.Count);

            return brands;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                EventIdStore.Vehicle.MotorcycleCatalogSearchError,
                ex,
                "Motorcycle brand search failed. Search={Search}",
                request.Search);

            throw;
        }
    }
}
