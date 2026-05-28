using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Auth.Queries.GetSecurityStatus;

public sealed class GetSecurityStatusQueryHandler : IRequestHandler<GetSecurityStatusQuery, SecurityStatusDto>
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IUserRepository _userRepository;

    public GetSecurityStatusQueryHandler(ICurrentUserProvider currentUserProvider, IUserRepository userRepository)
    {
        _currentUserProvider = currentUserProvider;
        _userRepository = userRepository;
    }

    public async Task<SecurityStatusDto> Handle(GetSecurityStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.GetUserId()
            ?? throw new UnauthorizedAccessException("Current user is not available.");

        var tenantIdentifier = _currentUserProvider.GetTenantIdentifier()
            ?? throw new UnauthorizedAccessException("Current tenant is not available.");

        var user = await _userRepository.GetByIdAsync(userId, tenantIdentifier, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Users.User), userId);

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("The user is inactive.");
        }

        return new SecurityStatusDto(
            user.Email,
            user.IsEmailVerified,
            user.TwoFactorEnabled,
            user.TwoFactorProvider?.ToString());
    }
}
