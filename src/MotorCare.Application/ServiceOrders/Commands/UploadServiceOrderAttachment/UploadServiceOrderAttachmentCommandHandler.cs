using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.ServiceOrders.Commands.UploadServiceOrderAttachment;

public class UploadServiceOrderAttachmentCommandHandler : IRequestHandler<UploadServiceOrderAttachmentCommand, ServiceOrderAttachmentDto>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IServiceOrderAttachmentStorage _storage;
    private readonly ILogger<UploadServiceOrderAttachmentCommandHandler> _logger;

    public UploadServiceOrderAttachmentCommandHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider,
        IServiceOrderAttachmentStorage storage,
        ILogger<UploadServiceOrderAttachmentCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
        _storage = storage;
        _logger = logger;
    }

    public async Task<ServiceOrderAttachmentDto> Handle(UploadServiceOrderAttachmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        if (!await _repository.ExistsAsync(request.ServiceOrderId, tenantId, cancellationToken))
        {
            throw new NotFoundException(nameof(ServiceOrder), request.ServiceOrderId);
        }

        var storedFile = await _storage.SaveAsync(tenantId, request.ServiceOrderId, request.File, cancellationToken);
        var attachment = new ServiceOrderAttachment(
            tenantId,
            request.ServiceOrderId,
            storedFile.FileName,
            storedFile.OriginalFileName,
            storedFile.FilePath,
            storedFile.ContentType,
            storedFile.FileSize,
            request.AttachmentType,
            request.Description,
            _currentUserProvider.GetUserId(),
            _currentUserProvider.GetEmail(),
            DateTimeOffset.UtcNow);

        await _repository.AddAttachmentAsync(attachment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceOrder.ServiceOrderAttachmentUploaded,
            "Service order attachment uploaded. ServiceOrderId={ServiceOrderId} AttachmentId={AttachmentId} AttachmentType={AttachmentType} UploadedByUserId={UploadedByUserId} FileSize={FileSize} ContentType={ContentType}",
            attachment.ServiceOrderId,
            attachment.Id,
            attachment.AttachmentType,
            attachment.UploadedByUserId,
            attachment.FileSize,
            attachment.ContentType);

        return ServiceOrderAttachmentMapper.ToDto(
            attachment,
            _storage.GetDownloadUrl(attachment.ServiceOrderId, attachment.Id));
    }
}
