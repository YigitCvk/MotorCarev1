using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderAttachmentDownload;

public class GetServiceOrderAttachmentDownloadQueryValidator : AbstractValidator<GetServiceOrderAttachmentDownloadQuery>
{
    public GetServiceOrderAttachmentDownloadQueryValidator()
    {
        RuleFor(x => x.ServiceOrderId).NotEmpty();
        RuleFor(x => x.AttachmentId).NotEmpty();
    }
}
