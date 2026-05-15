using MotorCare.App.Models.Tenants;

namespace MotorCare.App.Services;

public sealed class TenantsService
{
    private readonly ApiClient _apiClient;

    public TenantsService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<TenantProfile?> GetCurrentProfileAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<TenantProfile>("/api/tenants/current/profile", authorized: true, cancellationToken);

    public Task<TenantProfile?> UpdateCurrentProfileAsync(
        UpdateTenantProfileRequest request,
        CancellationToken cancellationToken = default)
        => _apiClient.PutAsync<UpdateTenantProfileRequest, TenantProfile>(
            "/api/tenants/current/profile",
            request,
            authorized: true,
            cancellationToken);
}
