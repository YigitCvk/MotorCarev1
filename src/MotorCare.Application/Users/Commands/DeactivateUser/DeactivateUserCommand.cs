using MediatR;

namespace MotorCare.Application.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest<Unit>;
