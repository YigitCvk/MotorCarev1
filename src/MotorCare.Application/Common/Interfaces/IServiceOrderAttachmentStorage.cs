using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.Common.Interfaces;

public interface IServiceOrderAttachmentStorage
{
    Task<StoredAttachmentFile> SaveAsync(
        string tenantId,
        Guid serviceOrderId,
        IUploadedFile file,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(ServiceOrderAttachment attachment, CancellationToken cancellationToken = default);

    string GetDownloadUrl(Guid serviceOrderId, Guid attachmentId);
}

public sealed record StoredAttachmentFile(
    string FileName,
    string OriginalFileName,
    string FilePath,
    string ContentType,
    long FileSize);
