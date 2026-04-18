using MediatR;

namespace MotorCare.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Unit>;
