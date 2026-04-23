using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Inventory;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inventory.Commands.CreateInventoryItem;

public sealed class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Guid>
{
    private readonly IInventoryRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateInventoryItemCommandHandler> _logger;

    public CreateInventoryItemCommandHandler(
        IInventoryRepository repository,
        ITenantProvider tenantProvider,
        ILogger<CreateInventoryItemCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        if (await _repository.GetByNameAsync(tenantId, request.Name, cancellationToken) is not null)
        {
            throw new ConflictException("Bu isimle bir parça zaten kayıtlı.");
        }

        if (!string.IsNullOrWhiteSpace(request.Sku) &&
            await _repository.GetBySkuAsync(tenantId, request.Sku, cancellationToken) is not null)
        {
            throw new ConflictException("Bu SKU ile bir parça zaten kayıtlı.");
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode) &&
            await _repository.GetByBarcodeAsync(tenantId, request.Barcode, cancellationToken) is not null)
        {
            throw new ConflictException("Bu barkod ile bir parça zaten kayıtlı.");
        }

        var item = new InventoryItem(
            tenantId,
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

        await _repository.AddAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Inventory.InventoryItemCreated,
            "Inventory item created. ItemId={ItemId} Name={Name} Sku={Sku} StockQuantity={StockQuantity}",
            item.Id,
            item.Name,
            item.Sku,
            item.StockQuantity);

        return item.Id;
    }
}
