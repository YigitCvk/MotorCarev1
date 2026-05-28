using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Services.Queries.GetServiceCatalogItems;

public sealed class GetServiceCatalogItemsQueryHandler : IRequestHandler<GetServiceCatalogItemsQuery, PagedResult<ServiceCatalogItemDto>>
{
    private readonly IServiceCatalogRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetServiceCatalogItemsQueryHandler(IServiceCatalogRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<PagedResult<ServiceCatalogItemDto>> Handle(GetServiceCatalogItemsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var pagination = PaginationRequest.Of(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _repository.GetPagedAsync(
            tenantId,
            request.SearchText,
            request.Category,
            request.IsActive,
            pagination.SafePageNumber,
            pagination.SafePageSize,
            cancellationToken);

        var dtoItems = items
            .Select(item => new ServiceCatalogItemDto(
                item.Id,
                item.Name,
                item.Category,
                ServiceCategoryTextMapper.ToText(item.Category),
                item.Description,
                item.DefaultDurationMinutes,
                item.DefaultPrice,
                item.Price,
                item.Currency,
                item.IsActive))
            .ToList();

        return PagedResult<ServiceCatalogItemDto>.Create(dtoItems, pagination.SafePageNumber, pagination.SafePageSize, totalCount);
    }
}
