using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inventory.Queries.GetInventoryItems;

public sealed class GetInventoryItemsQueryHandler : IRequestHandler<GetInventoryItemsQuery, PagedResult<InventoryItemDto>>
{
    private readonly IInventoryRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetInventoryItemsQueryHandler(IInventoryRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<PagedResult<InventoryItemDto>> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var pagination = PaginationRequest.Of(request.PageNumber, request.PageSize);
        var (items, totalCount) = await _repository.GetPagedAsync(
            tenantId,
            request.SearchText,
            request.Category,
            request.IsActive,
            request.LowStockOnly,
            pagination.SafePageNumber,
            pagination.SafePageSize,
            cancellationToken);

        var dtoItems = items
            .Select(item => new InventoryItemDto(
                item.Id,
                item.Name,
                item.Sku,
                item.Barcode,
                item.Category,
                item.Brand,
                item.Unit,
                item.UnitPrice,
                item.StockQuantity,
                item.MinimumStockLevel,
                item.IsLowStock,
                item.IsActive))
            .ToList();

        return PagedResult<InventoryItemDto>.Create(dtoItems, pagination.SafePageNumber, pagination.SafePageSize, totalCount);
    }
}
