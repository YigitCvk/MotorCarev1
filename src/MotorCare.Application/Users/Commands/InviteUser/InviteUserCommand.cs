using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Users.Commands.InviteUser;

public sealed record InviteUserCommand(string Email, UserRole Role, string? FullName = null) : IRequest<Unit>;
