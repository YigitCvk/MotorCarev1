using FluentValidation;

namespace MotorCare.Application.Inventory.Commands.DeactivateInventoryItem;

public sealed class DeactivateInventoryItemCommandValidator : AbstractValidator<DeactivateInventoryItemCommand>
{
    public DeactivateInventoryItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
