using MediatR;

namespace MotorCare.Application.Users.Queries.ValidateInvitation;

public sealed record InvitationValidationDto(
    string Email,
    string Role);

public sealed record ValidateInvitationQuery(string Token) : IRequest<InvitationValidationDto?>;
