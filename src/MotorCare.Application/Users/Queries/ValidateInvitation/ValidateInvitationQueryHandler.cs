using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Application.Users.Queries.ValidateInvitation;

public sealed class ValidateInvitationQueryHandler
    : IRequestHandler<ValidateInvitationQuery, InvitationValidationDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityTokenFactory _securityTokenFactory;

    public ValidateInvitationQueryHandler(
        IUserRepository userRepository,
        ISecurityTokenFactory securityTokenFactory)
    {
        _userRepository = userRepository;
        _securityTokenFactory = securityTokenFactory;
    }

    public async Task<InvitationValidationDto?> Handle(
        ValidateInvitationQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return null;
        }

        var tokenHash = _securityTokenFactory.Hash(request.Token.Trim());
        var token = await _userRepository.GetActiveSecurityTokenByHashAsync(
            tokenHash,
            UserSecurityTokenPurpose.UserInvitation,
            cancellationToken);

        if (token is null)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user is null || user.IsEmailVerified)
        {
            return null;
        }

        return new InvitationValidationDto(user.Email, user.Role.ToString());
    }
}
