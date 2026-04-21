using MotorCare.App.Models.Common;
using MotorCare.App.Models.Inventory;

namespace MotorCare.App.Services;

public sealed class InventoryService(ApiClient apiClient)
{
    public Task<PagedResult<InventoryItem>?> GetInventoryItemsAsync(
        string? searchText,
        string? category,
        bool? isActive,
        bool lowStockOnly,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchText))
            parts.Add($"q={Uri.EscapeDataString(searchText)}");

        if (!string.IsNullOrWhiteSpace(category))
            parts.Add($"category={Uri.EscapeDataString(category)}");

        if (isActive.HasValue)
            parts.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        if (lowStockOnly)
            parts.Add("lowStockOnly=true");

        parts.Add($"pageNumber={pageNumber}");
        parts.Add($"pageSize={pageSize}");

        return apiClient.GetAsync<PagedResult<InventoryItem>>($"/api/inventory?{string.Join("&", parts)}", authorized: true, ct);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetActiveItemsAsync(int pageSize = 100, CancellationToken ct = default)
    {
        var result = await GetInventoryItemsAsync(null, null, true, false, 1, pageSize, ct);
        return result?.Items ?? [];
    }

    public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => apiClient.GetAsync<InventoryItem>($"/api/inventory/{id}", authorized: true, ct);

    public Task<Guid> CreateAsync(CreateInventoryItemRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateInventoryItemRequest, Guid>("/api/inventory", request, authorized: true, ct)!;

    public Task UpdateAsync(Guid id, UpdateInventoryItemRequest request, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/inventory/{id}", request, authorized: true, ct);

    public Task ActivateAsync(Guid id, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/inventory/{id}/activate", new { }, authorized: true, ct);

    public Task DeactivateAsync(Guid id, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/inventory/{id}/deactivate", new { }, authorized: true, ct);

    public Task AdjustStockAsync(Guid id, AdjustInventoryStockRequest request, CancellationToken ct = default)
        => apiClient.PostAsync($"/api/inventory/{id}/adjust-stock", request, authorized: true, ct);
}
