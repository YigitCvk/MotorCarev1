using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.DeleteServiceOrderAttachment;

public class DeleteServiceOrderAttachmentCommandValidator : AbstractValidator<DeleteServiceOrderAttachmentCommand>
{
    public DeleteServiceOrderAttachmentCommandValidator()
    {
        RuleFor(x => x.ServiceOrderId).NotEmpty();
        RuleFor(x => x.AttachmentId).NotEmpty();
    }
}
