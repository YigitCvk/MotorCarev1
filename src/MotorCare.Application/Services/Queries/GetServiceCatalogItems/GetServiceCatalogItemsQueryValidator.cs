using FluentValidation;

namespace MotorCare.Application.Services.Queries.GetServiceCatalogItems;

public sealed class GetServiceCatalogItemsQueryValidator : AbstractValidator<GetServiceCatalogItemsQuery>
{
    public GetServiceCatalogItemsQueryValidator()
    {
        RuleFor(x => x.SearchText).MaximumLength(150);
    }
}
