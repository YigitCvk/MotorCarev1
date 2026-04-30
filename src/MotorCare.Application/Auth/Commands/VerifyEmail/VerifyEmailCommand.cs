using MediatR;

namespace MotorCare.Application.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Email, string Token) : IRequest<AuthActionMessageDto>;
