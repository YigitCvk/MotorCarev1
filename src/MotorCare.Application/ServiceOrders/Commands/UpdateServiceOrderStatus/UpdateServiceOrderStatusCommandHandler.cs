using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Commands.UpdateServiceOrderStatus;

public class UpdateServiceOrderStatusCommandHandler : IRequestHandler<UpdateServiceOrderStatusCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public UpdateServiceOrderStatusCommandHandler(IServiceOrderRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Unit> Handle(UpdateServiceOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);

        switch (request.Status)
        {
            case ServiceOrderStatus.InProgress:
                if (order.Status == ServiceOrderStatus.WaitingForParts)
                {
                    order.ResumeProgress();
                }
                else
                {
                    order.StartProgress();
                }
                break;
            case ServiceOrderStatus.WaitingForParts:
                order.WaitForParts();
                break;
            case ServiceOrderStatus.Completed:
                order.MarkAsCompleted();
                break;
            case ServiceOrderStatus.Delivered:
                order.MarkAsDelivered();
                break;
            case ServiceOrderStatus.Cancelled:
                order.Cancel();
                break;
            case ServiceOrderStatus.Open:
            default:
                throw new DomainException("Transition to the requested status is not supported.");
        }

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
