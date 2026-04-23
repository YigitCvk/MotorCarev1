using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Commands.SetOrderDiscount;

public class SetOrderDiscountCommandHandler : IRequestHandler<SetOrderDiscountCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<SetOrderDiscountCommandHandler> _logger;

    public SetOrderDiscountCommandHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ILogger<SetOrderDiscountCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(SetOrderDiscountCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);

        order.SetDiscount(request.Discount);

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.DiscountUpdated,
            "Discount updated. ServiceOrderId={ServiceOrderId} Discount={Discount}",
            request.Id,
            request.Discount);

        return Unit.Value;
    }
}
