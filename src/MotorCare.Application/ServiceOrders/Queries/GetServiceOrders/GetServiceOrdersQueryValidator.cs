using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrders;

public class GetServiceOrdersQueryValidator : AbstractValidator<GetServiceOrdersQuery>
{
    public GetServiceOrdersQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty).When(x => x.CustomerId.HasValue);
        RuleFor(x => x.SearchText).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.SearchText));
        RuleFor(x => x)
            .Must(x => !x.OpenedFrom.HasValue || !x.OpenedTo.HasValue || x.OpenedFrom <= x.OpenedTo)
            .WithMessage("OpenedFrom must be earlier than or equal to OpenedTo.");
    }
}
