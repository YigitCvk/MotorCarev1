using FluentValidation;

namespace MotorCare.Application.Auth.Commands.ResendTwoFactorEmail;

public sealed class ResendTwoFactorEmailCommandValidator : AbstractValidator<ResendTwoFactorEmailCommand>
{
    public ResendTwoFactorEmailCommandValidator()
    {
        RuleFor(x => x.Ticket).NotEmpty();
    }
}
