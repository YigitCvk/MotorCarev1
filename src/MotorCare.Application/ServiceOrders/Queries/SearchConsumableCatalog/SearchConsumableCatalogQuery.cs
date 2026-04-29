using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Queries.SearchConsumableCatalog;

public sealed record SearchConsumableCatalogQuery(
    string? Query,
    string? Category,
    int MaxResults = 20) : IRequest<IReadOnlyList<ConsumableCatalogItemDto>>;

public sealed class SearchConsumableCatalogQueryHandler : IRequestHandler<SearchConsumableCatalogQuery, IReadOnlyList<ConsumableCatalogItemDto>>
{
    private readonly IConsumableCatalogRepository _repository;
    private readonly ILogger<SearchConsumableCatalogQueryHandler> _logger;

    public SearchConsumableCatalogQueryHandler(
        IConsumableCatalogRepository repository,
        ILogger<SearchConsumableCatalogQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ConsumableCatalogItemDto>> Handle(SearchConsumableCatalogQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.SearchAsync(request.Query, request.Category, request.MaxResults, cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.ConsumableSuggestionsFetched,
            "Consumable catalog searched. Query={Query}, Category={Category}, ResultsCount={Count}",
            request.Query,
            request.Category,
            items.Count);

        return items
            .Select(item => new ConsumableCatalogItemDto(
                item.Category,
                item.SubCategory,
                item.Brand,
                item.ProductName,
                item.Specification,
                item.Notes))
            .ToList()
            .AsReadOnly();
    }
}
