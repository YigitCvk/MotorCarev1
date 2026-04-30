using MediatR;

namespace MotorCare.Application.Auth.Commands.ResendEmailVerification;

public sealed record ResendEmailVerificationCommand(string TenantIdentifier, string Email) : IRequest<AuthActionMessageDto>;
