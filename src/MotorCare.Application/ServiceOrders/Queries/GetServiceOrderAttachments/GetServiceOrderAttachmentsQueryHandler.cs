using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderAttachments;

public class GetServiceOrderAttachmentsQueryHandler : IRequestHandler<GetServiceOrderAttachmentsQuery, IReadOnlyList<ServiceOrderAttachmentDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IServiceOrderAttachmentStorage _storage;
    private readonly ILogger<GetServiceOrderAttachmentsQueryHandler> _logger;

    public GetServiceOrderAttachmentsQueryHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        IServiceOrderAttachmentStorage storage,
        ILogger<GetServiceOrderAttachmentsQueryHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _storage = storage;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ServiceOrderAttachmentDto>> Handle(GetServiceOrderAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        if (!await _repository.ExistsAsync(request.ServiceOrderId, tenantId, cancellationToken))
        {
            throw new NotFoundException(nameof(ServiceOrder), request.ServiceOrderId);
        }

        var attachments = await _repository.GetAttachmentsAsync(request.ServiceOrderId, tenantId, cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.ServiceOrderAttachmentsFetched,
            "Service order attachments fetched. ServiceOrderId={ServiceOrderId} TenantId={TenantId} AttachmentCount={AttachmentCount}",
            request.ServiceOrderId,
            tenantId,
            attachments.Count);

        return attachments
            .Select(attachment => ServiceOrderAttachmentMapper.ToDto(
                attachment,
                _storage.GetDownloadUrl(attachment.ServiceOrderId, attachment.Id)))
            .ToList();
    }
}
