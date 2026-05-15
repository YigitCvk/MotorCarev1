namespace MotorCare.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string OwnerOnly = nameof(OwnerOnly);
    public const string TenantManagement = nameof(TenantManagement);
    public const string UserManagement = nameof(UserManagement);
    public const string CustomerRead = nameof(CustomerRead);
    public const string CustomerOperations = nameof(CustomerOperations);
    public const string ServiceOrderRead = nameof(ServiceOrderRead);
    public const string ServiceOrderWrite = nameof(ServiceOrderWrite);
    public const string ServiceOrderPayments = nameof(ServiceOrderPayments);
    public const string InspectionRead = nameof(InspectionRead);
    public const string InspectionWrite = nameof(InspectionWrite);
    public const string InventoryRead = nameof(InventoryRead);
    public const string InventoryWrite = nameof(InventoryWrite);
    public const string DashboardRead = nameof(DashboardRead);
    public const string ImportOperations = nameof(ImportOperations);
}
