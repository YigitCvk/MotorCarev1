using MediatR;

namespace MotorCare.Application.Auth.Commands.ConfirmDisableTwoFactorEmail;

public sealed record ConfirmDisableTwoFactorEmailCommand(string Code) : IRequest<AuthActionMessageDto>;
