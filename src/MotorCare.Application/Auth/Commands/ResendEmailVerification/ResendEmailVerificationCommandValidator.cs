using FluentValidation;

namespace MotorCare.Application.Auth.Commands.ResendEmailVerification;

public sealed class ResendEmailVerificationCommandValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    public ResendEmailVerificationCommandValidator()
    {
        RuleFor(x => x.TenantIdentifier).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
