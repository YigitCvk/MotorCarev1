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
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<UpdateServiceOrderStatusCommandHandler> _logger;

    public UpdateServiceOrderStatusCommandHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider,
        ILogger<UpdateServiceOrderStatusCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateServiceOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var order = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.ServiceOrders.ServiceOrder), request.Id);
        var previousStatus = order.Status;

        if (previousStatus == request.Status)
        {
            throw new DomainException("Servis emri zaten seçilen durumda.");
        }

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
        await _repository.AddStatusHistoryAsync(
            new Domain.ServiceOrders.ServiceOrderStatusHistory(
                tenantId,
                order.Id,
                previousStatus,
                order.Status,
                request.Note,
                _currentUserProvider.GetUserId(),
                _currentUserProvider.GetEmail(),
                DateTimeOffset.UtcNow),
            cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.ServiceOrderStatusUpdated,
            "Service order status updated. ServiceOrderId={ServiceOrderId} TenantId={TenantId} FromStatus={FromStatus} ToStatus={ToStatus} ChangedByUserId={ChangedByUserId} ChangedAt={ChangedAt}",
            request.Id,
            tenantId,
            previousStatus,
            request.Status,
            _currentUserProvider.GetUserId(),
            DateTimeOffset.UtcNow);

        return Unit.Value;
    }
}
