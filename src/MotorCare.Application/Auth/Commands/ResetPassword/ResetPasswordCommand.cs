using MediatR;

namespace MotorCare.Application.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<AuthActionMessageDto>;
