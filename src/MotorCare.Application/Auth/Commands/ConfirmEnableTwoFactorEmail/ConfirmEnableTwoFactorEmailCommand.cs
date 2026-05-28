using MediatR;

namespace MotorCare.Application.Auth.Commands.ConfirmEnableTwoFactorEmail;

public sealed record ConfirmEnableTwoFactorEmailCommand(string Code) : IRequest<AuthActionMessageDto>;
