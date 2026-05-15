namespace MotorCare.App.Authorization;

public static class AppRoleAccess
{
    public static bool CanReadDashboard(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Accountant" or "ReadOnly" or "Manager";

    public static bool CanReadServiceOrders(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician" or "Accountant" or "ReadOnly" or "Manager";

    public static bool CanWriteServiceOrders(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician";

    public static bool CanReadCustomers(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician" or "Inspector" or "Accountant" or "ReadOnly" or "Manager";

    public static bool CanWriteCustomers(string? role)
        => role is "Owner" or "Admin" or "Receptionist";

    public static bool CanReadInspections(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Inspector" or "ReadOnly" or "Manager";

    public static bool CanReadInventory(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Technician" or "ReadOnly" or "Manager";

    public static bool CanReadFinance(string? role)
        => role is "Owner" or "Admin" or "Receptionist" or "Accountant";

    public static bool CanImport(string? role)
        => role is "Owner" or "Admin";
}
