using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderAttachmentDownload;

public class GetServiceOrderAttachmentDownloadQueryHandler : IRequestHandler<GetServiceOrderAttachmentDownloadQuery, ServiceOrderAttachmentDownloadDto?>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IServiceOrderAttachmentStorage _storage;
    private readonly ILogger<GetServiceOrderAttachmentDownloadQueryHandler> _logger;

    public GetServiceOrderAttachmentDownloadQueryHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        IServiceOrderAttachmentStorage storage,
        ILogger<GetServiceOrderAttachmentDownloadQueryHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _storage = storage;
        _logger = logger;
    }

    public async Task<ServiceOrderAttachmentDownloadDto?> Handle(GetServiceOrderAttachmentDownloadQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var attachment = await _repository.GetAttachmentAsync(
            request.ServiceOrderId,
            request.AttachmentId,
            tenantId,
            cancellationToken);

        if (attachment is null)
        {
            return null;
        }

        try
        {
            var stream = await _storage.OpenReadAsync(attachment, cancellationToken);

            _logger.LogInformation(
                EventIdStore.ServiceOrder.ServiceOrderAttachmentDownloaded,
                "Service order attachment download opened. ServiceOrderId={ServiceOrderId} AttachmentId={AttachmentId} AttachmentType={AttachmentType} FileSize={FileSize} ContentType={ContentType}",
                attachment.ServiceOrderId,
                attachment.Id,
                attachment.AttachmentType,
                attachment.FileSize,
                attachment.ContentType);

            return new ServiceOrderAttachmentDownloadDto(stream, attachment.ContentType, attachment.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException("Attachment file was not found.");
        }
    }
}
