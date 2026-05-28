using FluentValidation;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Users.Commands.CreateTenantUser;

public sealed class CreateTenantUserCommandValidator : AbstractValidator<CreateTenantUserCommand>
{
    public CreateTenantUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(r => Enum.IsDefined(typeof(UserRole), r))
            .WithMessage("Geçersiz kullanıcı rolü.");
    }
}
