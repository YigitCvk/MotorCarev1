using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.AddPartToOrder;

public class AddPartToOrderCommandValidator : AbstractValidator<AddPartToOrderCommand>
{
    public AddPartToOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PartName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PartNumber).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.PartNumber));
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
