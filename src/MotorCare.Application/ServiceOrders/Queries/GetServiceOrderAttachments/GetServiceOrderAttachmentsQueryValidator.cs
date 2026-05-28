using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderAttachments;

public class GetServiceOrderAttachmentsQueryValidator : AbstractValidator<GetServiceOrderAttachmentsQuery>
{
    public GetServiceOrderAttachmentsQueryValidator()
    {
        RuleFor(x => x.ServiceOrderId).NotEmpty();
    }
}
