using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderActivityFeed;

public sealed class GetServiceOrderActivityFeedQueryValidator : AbstractValidator<GetServiceOrderActivityFeedQuery>
{
    public GetServiceOrderActivityFeedQueryValidator()
    {
        RuleFor(x => x.ServiceOrderId).NotEmpty();
    }
}
