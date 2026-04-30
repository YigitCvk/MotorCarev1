using MediatR;

namespace MotorCare.Application.Auth.Commands.ResendTwoFactorEmail;

public sealed record ResendTwoFactorEmailCommand(string Ticket) : IRequest<AuthActionMessageDto>;
