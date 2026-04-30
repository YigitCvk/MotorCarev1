using MediatR;

namespace MotorCare.Application.Auth.Commands.SendDisableTwoFactorEmailCode;

public sealed record SendDisableTwoFactorEmailCodeCommand : IRequest<AuthActionMessageDto>;
