using MediatR;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderAttachments;

public sealed record GetServiceOrderAttachmentsQuery(Guid ServiceOrderId) : IRequest<IReadOnlyList<ServiceOrderAttachmentDto>>;
