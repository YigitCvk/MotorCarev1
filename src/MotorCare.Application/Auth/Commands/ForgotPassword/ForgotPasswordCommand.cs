using MediatR;

namespace MotorCare.Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string TenantIdentifier, string Email) : IRequest<AuthActionMessageDto>;
