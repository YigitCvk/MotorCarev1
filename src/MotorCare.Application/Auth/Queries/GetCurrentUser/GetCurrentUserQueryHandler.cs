using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(ICurrentUserProvider currentUserProvider, IUserRepository userRepository)
    {
        _currentUserProvider = currentUserProvider;
        _userRepository = userRepository;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
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

        return new CurrentUserDto(
            user.Id,
            _currentUserProvider.GetTenantId() ?? string.Empty,
            tenantIdentifier,
            user.FullName,
            user.Email,
            user.Role.ToString());
    }
}
