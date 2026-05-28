using MediatR;

namespace MotorCare.Application.Auth.Commands.VerifyTwoFactorEmail;

public sealed record VerifyTwoFactorEmailCommand(string Ticket, string Code) : IRequest<AuthResponseDto>;
