using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.ServiceOrders.Commands.UploadServiceOrderAttachment;

public sealed record UploadServiceOrderAttachmentCommand(
    Guid ServiceOrderId,
    IUploadedFile File,
    ServiceOrderAttachmentType AttachmentType,
    string? Description) : IRequest<ServiceOrderAttachmentDto>;
