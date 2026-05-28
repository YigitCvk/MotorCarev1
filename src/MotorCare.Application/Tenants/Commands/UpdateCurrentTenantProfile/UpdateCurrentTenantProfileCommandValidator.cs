using FluentValidation;

namespace MotorCare.Application.Tenants.Commands.UpdateCurrentTenantProfile;

public sealed class UpdateCurrentTenantProfileCommandValidator : AbstractValidator<UpdateCurrentTenantProfileCommand>
{
    public UpdateCurrentTenantProfileCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.LegalName).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.LegalName));
        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .Must(value => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("Logo URL must be an absolute HTTP or HTTPS URL.");
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.Email)
            .MaximumLength(150)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Address).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Address));
        RuleFor(x => x.TaxOffice).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.TaxOffice));
        RuleFor(x => x.TaxNumber).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.TaxNumber));
        RuleFor(x => x.Website)
            .MaximumLength(250)
            .Must(value => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrWhiteSpace(x.Website))
            .WithMessage("Website must be an absolute HTTP or HTTPS URL.");
    }
}
