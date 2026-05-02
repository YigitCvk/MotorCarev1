namespace MotorCare.App.Models.ServiceOrders;

public sealed class ServiceOrderAttachment
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string AttachmentTypeText { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? UploadedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsImage { get; set; }
    public bool IsPdf { get; set; }
}
