using FluentValidation;

namespace MotorCare.Application.Services.Commands.UpdateServiceCatalogItem;

public sealed class UpdateServiceCatalogItemCommandValidator : AbstractValidator<UpdateServiceCatalogItemCommand>
{
    public UpdateServiceCatalogItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DefaultDurationMinutes).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(5);
    }
}
