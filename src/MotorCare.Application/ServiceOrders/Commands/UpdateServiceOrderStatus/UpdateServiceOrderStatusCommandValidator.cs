using FluentValidation;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.ServiceOrders.Commands.UpdateServiceOrderStatus;

public class UpdateServiceOrderStatusCommandValidator : AbstractValidator<UpdateServiceOrderStatusCommand>
{
    public UpdateServiceOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(status => status != ServiceOrderStatus.Open)
            .WithMessage("Transition to Open is not supported.");
    }
}
