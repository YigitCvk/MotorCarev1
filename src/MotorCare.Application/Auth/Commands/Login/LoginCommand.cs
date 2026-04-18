using MediatR;

namespace MotorCare.Application.Auth.Commands.Login;

public sealed record LoginCommand(string TenantIdentifier, string Email, string Password) : IRequest<AuthResponseDto>;
