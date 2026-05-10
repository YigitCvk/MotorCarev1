using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inventory.Commands.DeactivateInventoryItem;

public sealed class DeactivateInventoryItemCommandHandler : IRequestHandler<DeactivateInventoryItemCommand>
{
    private readonly IInventoryRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<DeactivateInventoryItemCommandHandler> _logger;

    public DeactivateInventoryItemCommandHandler(
        IInventoryRepository repository,
        ITenantProvider tenantProvider,
        ILogger<DeactivateInventoryItemCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(DeactivateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var item = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("Parça bulunamadı.");

        item.Deactivate();
        _repository.Update(item);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Inventory.InventoryItemDeactivated,
            "Inventory item {InventoryItemId} deactivated for tenant {TenantId}.",
            item.Id,
            tenantId);
    }
}
