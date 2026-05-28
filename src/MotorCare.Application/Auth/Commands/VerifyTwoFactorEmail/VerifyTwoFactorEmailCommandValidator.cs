using FluentValidation;

namespace MotorCare.Application.Auth.Commands.VerifyTwoFactorEmail;

public sealed class VerifyTwoFactorEmailCommandValidator : AbstractValidator<VerifyTwoFactorEmailCommand>
{
    public VerifyTwoFactorEmailCommandValidator()
    {
        RuleFor(x => x.Ticket).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}
