using MotorCare.App.Models.Common;
using MotorCare.App.Models.Inspections;

namespace MotorCare.App.Services;

public sealed class InspectionsService(ApiClient apiClient)
{
    public Task<PagedResult<MotorcycleInspectionListItem>?> GetInspectionsAsync(
        string? q = null,
        MotorcycleInspectionPackageType? packageType = null,
        MotorcycleInspectionStatus? status = null,
        Guid? customerId = null,
        Guid? vehicleId = null,
        DateTimeOffset? createdFrom = null,
        DateTimeOffset? createdTo = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) parts.Add($"q={Uri.EscapeDataString(q)}");
        if (packageType.HasValue) parts.Add($"packageType={(int)packageType.Value}");
        if (status.HasValue) parts.Add($"status={(int)status.Value}");
        if (customerId.HasValue) parts.Add($"customerId={customerId.Value}");
        if (vehicleId.HasValue) parts.Add($"vehicleId={vehicleId.Value}");
        if (createdFrom.HasValue) parts.Add($"createdFrom={Uri.EscapeDataString(createdFrom.Value.ToString("O"))}");
        if (createdTo.HasValue) parts.Add($"createdTo={Uri.EscapeDataString(createdTo.Value.ToString("O"))}");
        parts.Add($"pageNumber={pageNumber}");
        parts.Add($"pageSize={pageSize}");
        return apiClient.GetAsync<PagedResult<MotorcycleInspectionListItem>>($"/api/inspections?{string.Join("&", parts)}", authorized: true, ct);
    }

    public Task<MotorcycleInspectionDetail?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => apiClient.GetAsync<MotorcycleInspectionDetail>($"/api/inspections/{id}", authorized: true, ct);

    public Task<CreateMotorcycleInspectionResponse?> CreateAsync(CreateMotorcycleInspectionRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateMotorcycleInspectionRequest, CreateMotorcycleInspectionResponse>("/api/inspections", request, authorized: true, ct);

    public Task UpdateAsync(Guid id, UpdateMotorcycleInspectionRequest request, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/inspections/{id}", request, authorized: true, ct);

    public Task UpdateItemAsync(Guid id, Guid itemId, UpdateMotorcycleInspectionItemRequest request, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/inspections/{id}/items/{itemId}", request, authorized: true, ct);

    public Task CompleteAsync(Guid id, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/inspections/{id}/complete", new { }, authorized: true, ct);

    public Task CancelAsync(Guid id, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/inspections/{id}/cancel", new { }, authorized: true, ct);
}
