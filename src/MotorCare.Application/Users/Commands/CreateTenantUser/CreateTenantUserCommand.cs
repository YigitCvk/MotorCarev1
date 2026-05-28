using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Users.Commands.CreateTenantUser;

public sealed record CreateTenantUserCommand(
    string FullName,
    string Email,
    string Password,
    UserRole Role) : IRequest<Guid>;
