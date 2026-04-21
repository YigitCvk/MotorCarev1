using FluentValidation;

namespace MotorCare.Application.Inventory.Queries.GetInventoryItems;

public sealed class GetInventoryItemsQueryValidator : AbstractValidator<GetInventoryItemsQuery>
{
    public GetInventoryItemsQueryValidator()
    {
        RuleFor(x => x.SearchText).MaximumLength(160);
        RuleFor(x => x.Category).MaximumLength(100);
    }
}
