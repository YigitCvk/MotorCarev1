using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Inventory.Commands.DeactivateInventoryItem;

public sealed class DeactivateInventoryItemCommandHandler : IRequestHandler<DeactivateInventoryItemCommand>
{
    private readonly IInventoryRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public DeactivateInventoryItemCommandHandler(IInventoryRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
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
    }
}
