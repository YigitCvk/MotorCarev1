using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;

namespace MotorCare.Domain.ServiceOrders;

public class ServiceOrderAttachment : BaseEntity, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public Guid ServiceOrderId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public ServiceOrderAttachmentType AttachmentType { get; private set; }
    public string? Description { get; private set; }
    public Guid? UploadedByUserId { get; private set; }
    public string? UploadedByUserName { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    private ServiceOrderAttachment()
    {
    }

    public ServiceOrderAttachment(
        string tenantId,
        Guid serviceOrderId,
        string fileName,
        string originalFileName,
        string filePath,
        string contentType,
        long fileSize,
        ServiceOrderAttachmentType attachmentType,
        string? description,
        Guid? uploadedByUserId,
        string? uploadedByUserName,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new DomainException("Tenant ID is required.");
        if (serviceOrderId == Guid.Empty) throw new DomainException("Service order ID is required.");
        if (string.IsNullOrWhiteSpace(fileName)) throw new DomainException("File name is required.");
        if (string.IsNullOrWhiteSpace(originalFileName)) throw new DomainException("Original file name is required.");
        if (string.IsNullOrWhiteSpace(filePath)) throw new DomainException("File path is required.");
        if (string.IsNullOrWhiteSpace(contentType)) throw new DomainException("Content type is required.");
        if (fileSize <= 0) throw new DomainException("File size must be greater than zero.");
        if (!Enum.IsDefined(attachmentType)) throw new DomainException("Attachment type is invalid.");
        if (description?.Length > 500) throw new DomainException("Attachment description cannot exceed 500 characters.");

        Id = Guid.NewGuid();
        TenantId = tenantId.Trim();
        ServiceOrderId = serviceOrderId;
        FileName = fileName.Trim();
        OriginalFileName = originalFileName.Trim();
        FilePath = filePath.Trim();
        ContentType = contentType.Trim();
        FileSize = fileSize;
        AttachmentType = attachmentType;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UploadedByUserId = uploadedByUserId;
        UploadedByUserName = string.IsNullOrWhiteSpace(uploadedByUserName) ? null : uploadedByUserName.Trim();
        CreatedAt = createdAt.ToUniversalTime();
    }

    public void MarkDeleted(Guid? deletedByUserId, DateTimeOffset deletedAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = deletedAt.ToUniversalTime();
        DeletedByUserId = deletedByUserId;
    }
}
