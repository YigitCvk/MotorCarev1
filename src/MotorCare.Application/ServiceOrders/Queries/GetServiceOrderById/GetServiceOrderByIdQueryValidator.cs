using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderById;

public class GetServiceOrderByIdQueryValidator : AbstractValidator<GetServiceOrderByIdQuery>
{
    public GetServiceOrderByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
