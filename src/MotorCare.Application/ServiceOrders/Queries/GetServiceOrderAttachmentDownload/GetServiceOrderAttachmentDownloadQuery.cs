using MediatR;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderAttachmentDownload;

public sealed record ServiceOrderAttachmentDownloadDto(
    Stream Stream,
    string ContentType,
    string OriginalFileName);

public sealed record GetServiceOrderAttachmentDownloadQuery(Guid ServiceOrderId, Guid AttachmentId)
    : IRequest<ServiceOrderAttachmentDownloadDto?>;
