using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Users.Commands.UpdateUserRole;

public sealed class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateUserRoleCommandHandler> _logger;

    public UpdateUserRoleCommandHandler(
        IUserRepository userRepository,
        ITenantProvider tenantProvider,
        ILogger<UpdateUserRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var user = await _userRepository.GetByIdAsync(request.UserId, tenantId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.Role == UserRole.Owner && request.Role != UserRole.Owner)
            throw new DomainException("Owner rolündeki kullanıcının rolü değiştirilemez.");

        user.UpdateRole(request.Role);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.User.UserRoleUpdated,
            "User role updated. UserId={UserId} TenantId={TenantId} NewRole={NewRole}",
            request.UserId,
            tenantId,
            request.Role);

        return Unit.Value;
    }
}
