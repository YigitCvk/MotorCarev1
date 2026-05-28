using MediatR;

namespace MotorCare.Application.Auth.Commands.AcceptInvite;

public sealed record AcceptInviteCommand(
    string Token,
    string FullName,
    string Password,
    string ConfirmPassword) : IRequest<AuthActionMessageDto>;
