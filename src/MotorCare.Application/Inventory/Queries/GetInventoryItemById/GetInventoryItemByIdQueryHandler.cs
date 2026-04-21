using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inventory.Queries.GetInventoryItemById;

public sealed class GetInventoryItemByIdQueryHandler : IRequestHandler<GetInventoryItemByIdQuery, InventoryItemDto?>
{
    private readonly IInventoryRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetInventoryItemByIdQueryHandler(IInventoryRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<InventoryItemDto?> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var item = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken);
        return item is null
            ? null
            : new InventoryItemDto(
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
                item.IsActive);
    }
}
