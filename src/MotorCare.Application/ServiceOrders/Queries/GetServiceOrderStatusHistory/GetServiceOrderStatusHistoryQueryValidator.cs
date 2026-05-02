using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderStatusHistory;

public sealed class GetServiceOrderStatusHistoryQueryValidator : AbstractValidator<GetServiceOrderStatusHistoryQuery>
{
    public GetServiceOrderStatusHistoryQueryValidator()
    {
        RuleFor(x => x.ServiceOrderId)
            .NotEmpty()
            .WithMessage("Servis emri zorunludur.");
    }
}
