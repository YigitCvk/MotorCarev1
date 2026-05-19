using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.RemoveConsumableFromOrder;

public class RemoveConsumableFromOrderCommandValidator : AbstractValidator<RemoveConsumableFromOrderCommand>
{
    public RemoveConsumableFromOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ConsumableId).NotEmpty();
    }
}
