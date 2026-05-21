namespace MotorCare.App.Authorization;

public static class AppRoleAccess
{
    public static bool IsKnownRole(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician" or "Manager" or "Inspector" or "Accountant" or "ReadOnly";

    public static bool CanReadSettings(string? role)
        => IsKnownRole(role);

    public static bool CanManageTenant(string? role)
        => role is "Owner";

    public static bool CanManageUsers(string? role)
        => role is "Owner" or "Admin";

    public static bool CanReadDashboard(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Accountant" or "ReadOnly" or "Manager";

    public static string GetDefaultRoute(string? role)
    {
        if (CanReadDashboard(role))
        {
            return "/dashboard";
        }

        if (CanReadServiceOrders(role))
        {
            return "/service-orders";
        }

        if (CanReadInspections(role))
        {
            return "/inspections";
        }

        if (CanReadCustomers(role))
        {
            return "/customers";
        }

        return "/settings";
    }

    public static bool CanReadServiceOrders(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician" or "Accountant" or "ReadOnly" or "Manager";

    public static bool CanWriteServiceOrders(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician";

    public static bool CanManageServiceOrderPayments(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Accountant";

    public static bool CanReadCustomers(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician" or "Inspector" or "Accountant" or "ReadOnly" or "Manager";

    public static bool CanWriteCustomers(string? role)
        => role is "Owner" or "Admin" or "Receptionist";

    public static bool CanReadServices(string? role)
        => CanReadCustomers(role);

    public static bool CanWriteServices(string? role)
        => CanWriteCustomers(role);

    public static bool CanReadInspections(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Inspector" or "ReadOnly" or "Manager";

    public static bool CanWriteInspections(string? role)
        => role is "Owner" or "Admin" or "Inspector";

    public static bool CanReadInventory(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician" or "ReadOnly" or "Manager";

    public static bool CanWriteInventory(string? role)
        => role is "Owner" or "Admin" or "Receptionist";

    public static bool CanReadFinance(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Accountant";

    public static bool CanImport(string? role)
        => role is "Owner" or "Admin";

    public static bool CanAccessRoute(string? role, string path)
    {
        var normalizedPath = NormalizePath(path);

        if (normalizedPath == "/forbidden")
        {
            return IsKnownRole(role);
        }

        if (normalizedPath == "/dashboard")
        {
            return CanReadDashboard(role);
        }

        if (normalizedPath == "/appointments/create")
        {
            return CanWriteServiceOrders(role);
        }

        if (normalizedPath.StartsWith("/appointments", StringComparison.Ordinal))
        {
            return CanReadServiceOrders(role);
        }

        if (normalizedPath == "/service-orders/create")
        {
            return CanWriteServiceOrders(role);
        }

        if (normalizedPath.StartsWith("/service-orders", StringComparison.Ordinal))
        {
            return CanReadServiceOrders(role);
        }

        if (normalizedPath == "/customers/create")
        {
            return CanWriteCustomers(role);
        }

        if (normalizedPath.StartsWith("/customers", StringComparison.Ordinal))
        {
            return CanReadCustomers(role);
        }

        if (normalizedPath == "/vehicles/create")
        {
            return CanWriteCustomers(role);
        }

        if (normalizedPath.StartsWith("/vehicles", StringComparison.Ordinal))
        {
            return CanReadCustomers(role);
        }

        if (normalizedPath == "/inspections/create")
        {
            return CanWriteInspections(role);
        }

        if (normalizedPath.StartsWith("/inspections", StringComparison.Ordinal))
        {
            return CanReadInspections(role);
        }

        if (normalizedPath == "/services/create" ||
            (normalizedPath.StartsWith("/services/", StringComparison.Ordinal) &&
             normalizedPath.EndsWith("/edit", StringComparison.Ordinal)))
        {
            return CanWriteServices(role);
        }

        if (normalizedPath.StartsWith("/services", StringComparison.Ordinal))
        {
            return CanReadServices(role);
        }

        if (normalizedPath == "/inventory/create" ||
            (normalizedPath.StartsWith("/inventory/", StringComparison.Ordinal) &&
             normalizedPath.EndsWith("/edit", StringComparison.Ordinal)))
        {
            return CanWriteInventory(role);
        }

        if (normalizedPath.StartsWith("/inventory", StringComparison.Ordinal))
        {
            return CanReadInventory(role);
        }

        if (normalizedPath.StartsWith("/imports", StringComparison.Ordinal))
        {
            return CanImport(role);
        }

        if (normalizedPath is "/finance" or "/collections")
        {
            return CanReadFinance(role);
        }

        if (normalizedPath.StartsWith("/reports", StringComparison.Ordinal))
        {
            return CanReadDashboard(role);
        }

        if (normalizedPath.StartsWith("/settings/users", StringComparison.Ordinal))
        {
            return CanManageUsers(role);
        }

        if (normalizedPath.StartsWith("/settings", StringComparison.Ordinal))
        {
            return CanReadSettings(role);
        }

        return IsKnownRole(role);
    }

    private static string NormalizePath(string path)
    {
        var pathOnly = path.Split('?', '#')[0].Trim();
        if (string.IsNullOrWhiteSpace(pathOnly))
        {
            return "/";
        }

        if (!pathOnly.StartsWith('/'))
        {
            pathOnly = "/" + pathOnly;
        }

        return pathOnly.Length == 1 ? pathOnly : pathOnly.TrimEnd('/');
    }
}
