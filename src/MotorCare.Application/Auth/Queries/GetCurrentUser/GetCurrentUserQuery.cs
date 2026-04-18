using MediatR;

namespace MotorCare.Application.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto>;
