using MediatR;

namespace MotorCare.Application.ServiceOrders.Commands.DeleteServiceOrderAttachment;

public sealed record DeleteServiceOrderAttachmentCommand(Guid ServiceOrderId, Guid AttachmentId) : IRequest<Unit>;
