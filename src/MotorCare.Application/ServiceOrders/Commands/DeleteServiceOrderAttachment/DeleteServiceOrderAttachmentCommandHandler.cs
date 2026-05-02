using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.ServiceOrders.Commands.DeleteServiceOrderAttachment;

public class DeleteServiceOrderAttachmentCommandHandler : IRequestHandler<DeleteServiceOrderAttachmentCommand, Unit>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<DeleteServiceOrderAttachmentCommandHandler> _logger;

    public DeleteServiceOrderAttachmentCommandHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider,
        ILogger<DeleteServiceOrderAttachmentCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteServiceOrderAttachmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var attachment = await _repository.GetAttachmentAsync(
                request.ServiceOrderId,
                request.AttachmentId,
                tenantId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(ServiceOrderAttachment), request.AttachmentId);

        attachment.MarkDeleted(_currentUserProvider.GetUserId(), DateTimeOffset.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.ServiceOrderAttachmentDeleted,
            "Service order attachment soft deleted. ServiceOrderId={ServiceOrderId} AttachmentId={AttachmentId} AttachmentType={AttachmentType} DeletedByUserId={DeletedByUserId} FileSize={FileSize} ContentType={ContentType}",
            attachment.ServiceOrderId,
            attachment.Id,
            attachment.AttachmentType,
            attachment.DeletedByUserId,
            attachment.FileSize,
            attachment.ContentType);

        return Unit.Value;
    }
}
