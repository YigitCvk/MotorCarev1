using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IUserRepository userRepository,
        ITenantProvider tenantProvider,
        ILogger<GetUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var users = await _userRepository.GetAllByTenantAsync(tenantId, cancellationToken);

        _logger.LogInformation(
            "Users fetched for TenantId={TenantId} Count={Count}",
            tenantId,
            users.Count);

        return users.Select(u => new UserDto(
            u.Id,
            u.FullName,
            u.Email,
            u.Role.ToString(),
            MapRoleText(u.Role),
            u.IsActive,
            u.IsEmailVerified,
            u.LastLoginAt,
            u.CreatedAt)).ToList();
    }

    private static string MapRoleText(UserRole role) => role switch
    {
        UserRole.Owner => "Sahip",
        UserRole.Admin => "Yönetici",
        UserRole.Receptionist => "Danışman",
        UserRole.Technician => "Teknisyen",
        UserRole.Manager => "Müdür",
        _ => role.ToString()
    };
}
