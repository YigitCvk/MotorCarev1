using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.UploadServiceOrderAttachment;

public class UploadServiceOrderAttachmentCommandValidator : AbstractValidator<UploadServiceOrderAttachmentCommand>
{
    public UploadServiceOrderAttachmentCommandValidator()
    {
        RuleFor(x => x.ServiceOrderId).NotEmpty();
        RuleFor(x => x.File).NotNull();
        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(ServiceOrderAttachmentFileRules.MaxFileSizeBytes)
            .When(x => x.File is not null);
        RuleFor(x => x.File.FileName)
            .NotEmpty()
            .MaximumLength(ServiceOrderAttachmentFileRules.MaxFileNameLength)
            .Must(ServiceOrderAttachmentFileRules.IsAllowedExtension)
            .WithMessage("Only jpg, jpeg, png, webp and pdf files are allowed.")
            .When(x => x.File is not null);
        RuleFor(x => x.File.ContentType)
            .NotEmpty()
            .Must(ServiceOrderAttachmentFileRules.IsAllowedContentType)
            .WithMessage("File content type is not allowed.")
            .When(x => x.File is not null);
        RuleFor(x => x.File)
            .Must(file => ServiceOrderAttachmentFileRules.IsAllowedExtensionContentTypePair(file.FileName, file.ContentType))
            .WithMessage("File extension and content type do not match.")
            .When(x => x.File is not null);
        RuleFor(x => x.AttachmentType).IsInEnum();
        RuleFor(x => x.Description)
            .MaximumLength(ServiceOrderAttachmentFileRules.MaxDescriptionLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
