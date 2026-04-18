using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.SetOrderDiscount;

public class SetOrderDiscountCommandValidator : AbstractValidator<SetOrderDiscountCommand>
{
    public SetOrderDiscountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
    }
}
