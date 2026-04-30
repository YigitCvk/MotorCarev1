using FluentValidation;

namespace MotorCare.Application.Auth.Commands.ConfirmDisableTwoFactorEmail;

public sealed class ConfirmDisableTwoFactorEmailCommandValidator : AbstractValidator<ConfirmDisableTwoFactorEmailCommand>
{
    public ConfirmDisableTwoFactorEmailCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(4, 12);
    }
}
