using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Commands.AddConsumableToOrder;

public sealed class AddConsumableToOrderCommandHandler : IRequestHandler<AddConsumableToOrderCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AddConsumableToOrderCommandHandler> _logger;

    public AddConsumableToOrderCommandHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ILogger<AddConsumableToOrderCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(AddConsumableToOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);

        order.AddConsumable(
            request.Category,
            request.ProductName,
            request.UnitPrice,
            request.Quantity,
            request.Brand,
            request.SubCategory,
            request.Specification,
            request.Notes);

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Consumable added to service order {ServiceOrderId} for tenant {TenantId}. Category={Category} ProductName={ProductName} Quantity={Quantity} UnitPrice={UnitPrice}",
            order.Id,
            tenantId,
            request.Category,
            request.ProductName,
            request.Quantity,
            request.UnitPrice);

        return Unit.Value;
    }
}
