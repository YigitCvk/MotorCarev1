using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Commands.AddPartToOrder;

public class AddPartToOrderCommandHandler : IRequestHandler<AddPartToOrderCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AddPartToOrderCommandHandler> _logger;

    public AddPartToOrderCommandHandler(
        IServiceOrderRepository repository,
        IInventoryRepository inventoryRepository,
        ITenantProvider tenantProvider,
        ILogger<AddPartToOrderCommandHandler> logger)
    {
        _repository = repository;
        _inventoryRepository = inventoryRepository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(AddPartToOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);

        if (request.InventoryItemId.HasValue)
        {
            var inventoryItem = await _inventoryRepository.GetByIdAsync(request.InventoryItemId.Value, tenantId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Inventory.InventoryItem), request.InventoryItemId.Value);

            inventoryItem.AdjustStock(-request.Quantity, "Servis emri parça kullanımı");
            _inventoryRepository.Update(inventoryItem);
        }

        order.AddPart(request.PartName, request.PartNumber, request.UnitPrice, request.Quantity, request.InventoryItemId);

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.PartAdded,
            "Part added to service order {ServiceOrderId} for tenant {TenantId}. PartName={PartName} Quantity={Quantity} UnitPrice={UnitPrice}",
            order.Id,
            tenantId,
            request.PartName,
            request.Quantity,
            request.UnitPrice);

        return Unit.Value;
    }
}
