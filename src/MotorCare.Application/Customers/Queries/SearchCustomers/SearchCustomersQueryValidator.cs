using FluentValidation;

namespace MotorCare.Application.Customers.Queries.SearchCustomers;

public class SearchCustomersQueryValidator : AbstractValidator<SearchCustomersQuery>
{
    public SearchCustomersQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MinimumLength(2)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));
    }
}
