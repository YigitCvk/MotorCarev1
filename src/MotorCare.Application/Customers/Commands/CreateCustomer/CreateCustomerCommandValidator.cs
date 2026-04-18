using FluentValidation;

namespace MotorCare.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).MaximumLength(150).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Whatsapp).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Whatsapp));
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
