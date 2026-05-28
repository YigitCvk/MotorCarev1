using MediatR;

namespace MotorCare.Application.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string TenantIdentifier, string Email, string Code) : IRequest<AuthActionMessageDto>;
