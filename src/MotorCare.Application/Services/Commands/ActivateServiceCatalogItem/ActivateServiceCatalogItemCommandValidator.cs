using FluentValidation;

namespace MotorCare.Application.Services.Commands.ActivateServiceCatalogItem;

public sealed class ActivateServiceCatalogItemCommandValidator : AbstractValidator<ActivateServiceCatalogItemCommand>
{
    public ActivateServiceCatalogItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
