using MotorCare.Domain.Enums;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.ServiceOrders;

public sealed record ServiceOrderAttachmentDto(
    Guid Id,
    Guid ServiceOrderId,
    string FileName,
    string OriginalFileName,
    string FileUrl,
    string ContentType,
    long FileSize,
    string AttachmentType,
    string AttachmentTypeText,
    string? Description,
    string? UploadedByUserName,
    DateTimeOffset CreatedAt,
    bool IsImage,
    bool IsPdf);

public static class ServiceOrderAttachmentMapper
{
    public static ServiceOrderAttachmentDto ToDto(ServiceOrderAttachment attachment, string fileUrl)
    {
        return new ServiceOrderAttachmentDto(
            attachment.Id,
            attachment.ServiceOrderId,
            attachment.FileName,
            attachment.OriginalFileName,
            fileUrl,
            attachment.ContentType,
            attachment.FileSize,
            attachment.AttachmentType.ToString(),
            GetAttachmentTypeText(attachment.AttachmentType),
            attachment.Description,
            attachment.UploadedByUserName,
            attachment.CreatedAt,
            attachment.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase),
            string.Equals(attachment.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase));
    }

    public static string GetAttachmentTypeText(ServiceOrderAttachmentType type) => type switch
    {
        ServiceOrderAttachmentType.BeforeServicePhoto => "İşlem Öncesi Fotoğraf",
        ServiceOrderAttachmentType.AfterServicePhoto => "İşlem Sonrası Fotoğraf",
        ServiceOrderAttachmentType.DamagePhoto => "Hasar Fotoğrafı",
        ServiceOrderAttachmentType.PartInvoice => "Parça Faturası",
        ServiceOrderAttachmentType.ServiceDocument => "Servis Evrakı",
        ServiceOrderAttachmentType.InspectionPhoto => "Ekspertiz Fotoğrafı",
        ServiceOrderAttachmentType.Other => "Diğer",
        _ => "Diğer"
    };
}
