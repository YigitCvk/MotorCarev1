using FluentValidation;

namespace MotorCare.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.TenantIdentifier).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Code).NotEmpty().Matches("^[0-9]{6}$").WithMessage("Doğrulama kodu 6 haneli rakamdan oluşmalıdır.");
    }
}
