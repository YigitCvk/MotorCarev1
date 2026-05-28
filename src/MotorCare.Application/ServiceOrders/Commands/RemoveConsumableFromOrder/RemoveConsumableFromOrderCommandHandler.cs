using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Commands.RemoveConsumableFromOrder;

public class RemoveConsumableFromOrderCommandHandler : IRequestHandler<RemoveConsumableFromOrderCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<RemoveConsumableFromOrderCommandHandler> _logger;

    public RemoveConsumableFromOrderCommandHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ILogger<RemoveConsumableFromOrderCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveConsumableFromOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);

        order.RemoveConsumable(request.ConsumableId);

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Consumable {ConsumableId} removed from service order {ServiceOrderId} for tenant {TenantId}.",
            request.ConsumableId,
            order.Id,
            tenantId);

        return Unit.Value;
    }
}
