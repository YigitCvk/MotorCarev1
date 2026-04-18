using FluentValidation;

namespace MotorCare.Application.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.TenantIdentifier).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(150).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}
