using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inventory.Commands.AdjustInventoryStock;

public sealed class AdjustInventoryStockCommandHandler : IRequestHandler<AdjustInventoryStockCommand>
{
    private readonly IInventoryRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AdjustInventoryStockCommandHandler> _logger;

    public AdjustInventoryStockCommandHandler(
        IInventoryRepository repository,
        ITenantProvider tenantProvider,
        ILogger<AdjustInventoryStockCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(AdjustInventoryStockCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var item = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Parça bulunamadı.");

        item.AdjustStock(request.QuantityDelta, request.Reason);

        _repository.Update(item);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Inventory.InventoryStockAdjusted,
            "Inventory stock adjusted. ItemId={ItemId} Delta={Delta} Reason={Reason} NewStock={NewStock}",
            item.Id,
            request.QuantityDelta,
            request.Reason,
            item.StockQuantity);

        if (item.IsLowStock)
        {
            _logger.LogWarning(
                EventIdStore.Inventory.LowStockDetected,
                "Low stock detected. ItemId={ItemId} Name={Name} StockQuantity={StockQuantity} MinimumStockLevel={MinimumStockLevel}",
                item.Id,
                item.Name,
                item.StockQuantity,
                item.MinimumStockLevel);
        }
    }
}
