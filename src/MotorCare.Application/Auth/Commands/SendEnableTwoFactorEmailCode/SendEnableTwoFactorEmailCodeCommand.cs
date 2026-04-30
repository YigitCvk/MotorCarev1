using MediatR;

namespace MotorCare.Application.Auth.Commands.SendEnableTwoFactorEmailCode;

public sealed record SendEnableTwoFactorEmailCodeCommand : IRequest<AuthActionMessageDto>;
