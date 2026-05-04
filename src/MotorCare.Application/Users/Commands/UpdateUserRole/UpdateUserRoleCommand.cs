using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Users.Commands.UpdateUserRole;

public sealed record UpdateUserRoleCommand(Guid UserId, UserRole Role) : IRequest<Unit>;
