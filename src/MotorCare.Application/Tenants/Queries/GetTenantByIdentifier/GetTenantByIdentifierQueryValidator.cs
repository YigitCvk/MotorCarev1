using FluentValidation;

namespace MotorCare.Application.Tenants.Queries.GetTenantByIdentifier;

public class GetTenantByIdentifierQueryValidator : AbstractValidator<GetTenantByIdentifierQuery>
{
    public GetTenantByIdentifierQueryValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty().MaximumLength(50);
    }
}
