using MotorCare.App.Models.Common;
using MotorCare.App.Models.Services;

namespace MotorCare.App.Services;

public sealed class ServicesService(ApiClient apiClient)
{
    public async Task<IReadOnlyList<ServiceCatalogItem>> GetActiveServicesAsync(int pageSize = 100, CancellationToken ct = default)
    {
        var result = await GetServicesAsync(null, null, true, 1, pageSize, ct);
        return result?.Items ?? [];
    }

    public Task<PagedResult<ServiceCatalogItem>?> GetServicesAsync(
        string? searchText,
        ServiceCategory? category,
        bool? isActive,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchText))
            parts.Add($"q={Uri.EscapeDataString(searchText)}");

        if (category.HasValue)
            parts.Add($"category={(int)category.Value}");

        if (isActive.HasValue)
            parts.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        parts.Add($"pageNumber={pageNumber}");
        parts.Add($"pageSize={pageSize}");

        return apiClient.GetAsync<PagedResult<ServiceCatalogItem>>($"/api/services?{string.Join("&", parts)}", authorized: true, ct);
    }

    public Task<ServiceCatalogItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => apiClient.GetAsync<ServiceCatalogItem>($"/api/services/{id}", authorized: true, ct);

    public Task<Guid> CreateAsync(CreateServiceCatalogItemRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateServiceCatalogItemRequest, Guid>("/api/services", request, authorized: true, ct)!;

    public Task UpdateAsync(Guid id, UpdateServiceCatalogItemRequest request, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/services/{id}", request, authorized: true, ct);

    public Task ActivateAsync(Guid id, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/services/{id}/activate", new { }, authorized: true, ct);

    public Task DeactivateAsync(Guid id, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/services/{id}/deactivate", new { }, authorized: true, ct);
}
