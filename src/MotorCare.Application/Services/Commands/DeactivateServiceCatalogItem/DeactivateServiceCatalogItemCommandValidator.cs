using FluentValidation;

namespace MotorCare.Application.Services.Commands.DeactivateServiceCatalogItem;

public sealed class DeactivateServiceCatalogItemCommandValidator : AbstractValidator<DeactivateServiceCatalogItemCommand>
{
    public DeactivateServiceCatalogItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
