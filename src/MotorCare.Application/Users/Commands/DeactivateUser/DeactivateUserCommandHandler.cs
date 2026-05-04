using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Common;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<DeactivateUserCommandHandler> _logger;

    public DeactivateUserCommandHandler(
        IUserRepository userRepository,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider,
        ILogger<DeactivateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var user = await _userRepository.GetByIdAsync(request.UserId, tenantId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        var currentUserId = _currentUserProvider.GetUserId();
        if (currentUserId.HasValue && currentUserId.Value == request.UserId)
            throw new DomainException("Kendi hesabınızı devre dışı bırakamazsınız.");

        user.Deactivate();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.User.UserDeactivated,
            "User deactivated. UserId={UserId} TenantId={TenantId}",
            request.UserId,
            tenantId);

        return Unit.Value;
    }
}
