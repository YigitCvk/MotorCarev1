using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Commands.AddPartToOrder;

public class AddPartToOrderCommandHandler : IRequestHandler<AddPartToOrderCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public AddPartToOrderCommandHandler(IServiceOrderRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Unit> Handle(AddPartToOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);

        order.AddPart(request.PartName, request.PartNumber, request.UnitPrice, request.Quantity);

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
