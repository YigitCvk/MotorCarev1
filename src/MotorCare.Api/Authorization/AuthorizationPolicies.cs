namespace MotorCare.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string OwnerOnly = nameof(OwnerOnly);
    public const string TenantManagement = nameof(TenantManagement);
    public const string CustomerOperations = nameof(CustomerOperations);
    public const string ServiceOrderRead = nameof(ServiceOrderRead);
    public const string ServiceOrderWrite = nameof(ServiceOrderWrite);
    public const string ServiceOrderPayments = nameof(ServiceOrderPayments);
    public const string DashboardRead = nameof(DashboardRead);
    public const string ImportOperations = nameof(ImportOperations);
}
