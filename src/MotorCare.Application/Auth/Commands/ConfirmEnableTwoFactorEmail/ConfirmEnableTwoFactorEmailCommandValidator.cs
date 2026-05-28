using FluentValidation;

namespace MotorCare.Application.Auth.Commands.ConfirmEnableTwoFactorEmail;

public sealed class ConfirmEnableTwoFactorEmailCommandValidator : AbstractValidator<ConfirmEnableTwoFactorEmailCommand>
{
    public ConfirmEnableTwoFactorEmailCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(4, 12);
    }
}
