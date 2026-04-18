using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.RemovePartFromOrder;

public class RemovePartFromOrderCommandValidator : AbstractValidator<RemovePartFromOrderCommand>
{
    public RemovePartFromOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PartId).NotEmpty();
    }
}
