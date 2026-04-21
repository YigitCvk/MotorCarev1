using FluentValidation;

namespace MotorCare.Application.Inventory.Commands.CreateInventoryItem;

public sealed class CreateInventoryItemCommandValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Sku).MaximumLength(80);
        RuleFor(x => x.Barcode).MaximumLength(120);
        RuleFor(x => x.Category).MaximumLength(100);
        RuleFor(x => x.Brand).MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStockLevel).GreaterThanOrEqualTo(0);
    }
}
