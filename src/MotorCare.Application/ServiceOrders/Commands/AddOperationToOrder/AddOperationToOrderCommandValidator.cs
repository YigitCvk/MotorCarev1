using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.AddOperationToOrder;

public class AddOperationToOrderCommandValidator : AbstractValidator<AddOperationToOrderCommand>
{
    public AddOperationToOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
