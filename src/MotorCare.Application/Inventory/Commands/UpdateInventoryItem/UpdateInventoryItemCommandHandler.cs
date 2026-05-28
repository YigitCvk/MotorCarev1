using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inventory.Commands.UpdateInventoryItem;

public sealed class UpdateInventoryItemCommandHandler : IRequestHandler<UpdateInventoryItemCommand>
{
    private readonly IInventoryRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateInventoryItemCommandHandler> _logger;

    public UpdateInventoryItemCommandHandler(
        IInventoryRepository repository,
        ITenantProvider tenantProvider,
        ILogger<UpdateInventoryItemCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var item = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Parça bulunamadı.");

        var byName = await _repository.GetByNameAsync(tenantId, request.Name, cancellationToken);
        if (byName is not null && byName.Id != request.Id)
        {
            throw new ConflictException("Bu isimle bir parça zaten kayıtlı.");
        }

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var bySku = await _repository.GetBySkuAsync(tenantId, request.Sku, cancellationToken);
            if (bySku is not null && bySku.Id != request.Id)
            {
                throw new ConflictException("Bu SKU ile bir parça zaten kayıtlı.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var byBarcode = await _repository.GetByBarcodeAsync(tenantId, request.Barcode, cancellationToken);
            if (byBarcode is not null && byBarcode.Id != request.Id)
            {
                throw new ConflictException("Bu barkod ile bir parça zaten kayıtlı.");
            }
        }

        item.Update(
            request.Name,
            request.Sku,
            request.Barcode,
            request.Category,
            request.Brand,
            request.Unit,
            request.UnitPrice,
            request.StockQuantity,
            request.MinimumStockLevel,
            request.IsActive);

        _repository.Update(item);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Inventory.InventoryItemUpdated,
            "Inventory item {InventoryItemId} updated for tenant {TenantId}. StockQuantity={StockQuantity} MinimumStockLevel={MinimumStockLevel} IsActive={IsActive}",
            item.Id,
            tenantId,
            item.StockQuantity,
            item.MinimumStockLevel,
            item.IsActive);
    }
}
