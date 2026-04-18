using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.AddPaymentToOrder;

public class AddPaymentToOrderCommandValidator : AbstractValidator<AddPaymentToOrderCommand>
{
    public AddPaymentToOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).IsInEnum();
    }
}
