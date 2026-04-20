using FluentValidation;

namespace MotorCare.Application.Services.Queries.GetServiceCatalogItemById;

public sealed class GetServiceCatalogItemByIdQueryValidator : AbstractValidator<GetServiceCatalogItemByIdQuery>
{
    public GetServiceCatalogItemByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
