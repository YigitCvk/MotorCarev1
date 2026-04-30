using FluentValidation;

namespace MotorCare.Application.Services.Commands.CreateServiceCatalogItem;

public sealed class CreateServiceCatalogItemCommandValidator : AbstractValidator<CreateServiceCatalogItemCommand>
{
    public CreateServiceCatalogItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DefaultDurationMinutes).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(5);
    }
}
