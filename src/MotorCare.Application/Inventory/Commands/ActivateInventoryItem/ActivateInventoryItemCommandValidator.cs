using FluentValidation;

namespace MotorCare.Application.Inventory.Commands.ActivateInventoryItem;

public sealed class ActivateInventoryItemCommandValidator : AbstractValidator<ActivateInventoryItemCommand>
{
    public ActivateInventoryItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
