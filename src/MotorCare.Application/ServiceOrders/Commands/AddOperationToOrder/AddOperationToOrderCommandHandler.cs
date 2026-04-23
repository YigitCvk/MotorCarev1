using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Commands.AddOperationToOrder;

public class AddOperationToOrderCommandHandler : IRequestHandler<AddOperationToOrderCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AddOperationToOrderCommandHandler> _logger;

    public AddOperationToOrderCommandHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ILogger<AddOperationToOrderCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(AddOperationToOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);

        order.AddOperation(request.Description, request.Price);

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.OperationAdded,
            "Operation added. ServiceOrderId={ServiceOrderId} Description={Description} Price={Price}",
            request.Id,
            request.Description,
            request.Price);

        return Unit.Value;
    }
}
