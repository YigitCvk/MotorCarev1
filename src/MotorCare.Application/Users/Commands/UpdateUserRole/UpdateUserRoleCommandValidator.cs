using FluentValidation;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Users.Commands.UpdateUserRole;

public sealed class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(r => r != UserRole.Owner)
            .WithMessage("Owner rolü bu işlemle atanamaz.");
    }
}
