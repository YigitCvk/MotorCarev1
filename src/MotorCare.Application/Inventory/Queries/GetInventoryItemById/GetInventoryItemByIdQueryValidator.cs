using FluentValidation;

namespace MotorCare.Application.Inventory.Queries.GetInventoryItemById;

public sealed class GetInventoryItemByIdQueryValidator : AbstractValidator<GetInventoryItemByIdQuery>
{
    public GetInventoryItemByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
