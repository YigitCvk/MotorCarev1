using MediatR;

namespace MotorCare.Application.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string TenantIdentifier,
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword) : IRequest<AuthActionMessageDto>;
