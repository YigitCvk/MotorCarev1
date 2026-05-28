using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Queries.GetPaymentSummary;

public class GetPaymentSummaryQueryValidator : AbstractValidator<GetPaymentSummaryQuery>
{
    public GetPaymentSummaryQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty().GreaterThanOrEqualTo(x => x.From);
    }
}
