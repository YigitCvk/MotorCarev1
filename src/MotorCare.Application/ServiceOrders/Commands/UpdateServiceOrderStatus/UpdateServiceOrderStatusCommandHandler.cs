using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
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
    private readonly ILogger<UpdateServiceOrderStatusCommandHandler> _logger;

    public UpdateServiceOrderStatusCommandHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ILogger<UpdateServiceOrderStatusCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
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
                _logger.LogWarning(
                    EventIdStore.ServiceOrder.BusinessRuleBlocked,
                    "Service order status transition blocked. ServiceOrderId={ServiceOrderId} RequestedStatus={RequestedStatus}",
                    request.Id,
                    request.Status);
                throw new DomainException("Transition to the requested status is not supported.");
        }

        _repository.Update(order);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.ServiceOrderStatusUpdated,
            "Service order status updated. ServiceOrderId={ServiceOrderId} NewStatus={NewStatus}",
            request.Id,
            request.Status);

        return Unit.Value;
    }
}
