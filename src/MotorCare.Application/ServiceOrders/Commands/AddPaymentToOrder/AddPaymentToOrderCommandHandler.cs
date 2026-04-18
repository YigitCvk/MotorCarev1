using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Commands.AddPaymentToOrder;

public class AddPaymentToOrderCommandHandler : IRequestHandler<AddPaymentToOrderCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public AddPaymentToOrderCommandHandler(IServiceOrderRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Unit> Handle(AddPaymentToOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);

        order.AddPayment(request.Amount, request.Method, request.PaymentDate ?? DateTimeOffset.UtcNow);

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
