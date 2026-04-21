using FluentValidation;

namespace MotorCare.Application.Inventory.Commands.AdjustInventoryStock;

public sealed class AdjustInventoryStockCommandValidator : AbstractValidator<AdjustInventoryStockCommand>
{
    public AdjustInventoryStockCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.QuantityDelta).NotEqual(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(250);
    }
}
