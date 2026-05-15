using MotorCare.Domain.Enums;

namespace MotorCare.Application.Common.Security;

public static class RoleCapabilities
{
    public static readonly IReadOnlyCollection<UserRole> UserManagement =
    [
        UserRole.Owner,
        UserRole.Admin
    ];

    public static readonly IReadOnlyCollection<UserRole> CustomerRead =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist,
        UserRole.Technician,
        UserRole.Inspector,
        UserRole.Accountant,
        UserRole.ReadOnly,
        UserRole.Manager
    ];

    public static readonly IReadOnlyCollection<UserRole> CustomerWrite =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist
    ];

    public static readonly IReadOnlyCollection<UserRole> ServiceOrderRead =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist,
        UserRole.Technician,
        UserRole.Accountant,
        UserRole.ReadOnly,
        UserRole.Manager
    ];

    public static readonly IReadOnlyCollection<UserRole> ServiceOrderWrite =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist,
        UserRole.Technician
    ];

    public static readonly IReadOnlyCollection<UserRole> ServiceOrderPayments =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist,
        UserRole.Accountant
    ];

    public static readonly IReadOnlyCollection<UserRole> InspectionRead =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist,
        UserRole.Inspector,
        UserRole.ReadOnly,
        UserRole.Manager
    ];

    public static readonly IReadOnlyCollection<UserRole> InspectionWrite =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Inspector
    ];

    public static readonly IReadOnlyCollection<UserRole> InventoryRead =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist,
        UserRole.Technician,
        UserRole.ReadOnly,
        UserRole.Manager
    ];

    public static readonly IReadOnlyCollection<UserRole> InventoryWrite =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist
    ];

    public static readonly IReadOnlyCollection<UserRole> DashboardRead =
    [
        UserRole.Owner,
        UserRole.Admin,
        UserRole.Receptionist,
        UserRole.Accountant,
        UserRole.ReadOnly,
        UserRole.Manager
    ];

    public static readonly IReadOnlyCollection<UserRole> ImportOperations =
    [
        UserRole.Owner,
        UserRole.Admin
    ];

    public static string[] Names(IEnumerable<UserRole> roles)
        => roles.Select(role => role.ToString()).ToArray();
}
