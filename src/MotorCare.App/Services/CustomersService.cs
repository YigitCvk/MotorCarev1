using MotorCare.App.Models.Common;
using MotorCare.App.Models.Customers;

namespace MotorCare.App.Services;

public sealed class CustomersService(ApiClient apiClient)
{
    public Task<PagedResult<CustomerLookupResponse>?> SearchCustomersAsync(
        string? searchText,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(searchText))
            parts.Add($"q={Uri.EscapeDataString(searchText)}");
        parts.Add($"pageNumber={pageNumber}");
        parts.Add($"pageSize={pageSize}");

        var uri = $"/api/customers?{string.Join("&", parts)}";
        return apiClient.GetAsync<PagedResult<CustomerLookupResponse>>(uri, authorized: true, ct);
    }

    public Task<CustomerLookupResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => apiClient.GetAsync<CustomerLookupResponse>($"/api/customers/{id}", authorized: true, ct);

    public Task<Guid> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateCustomerRequest, Guid>("/api/customers", request, authorized: true, ct)!;

    public Task UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/customers/{id}", request, authorized: true, ct);

    public Task<CustomerSummary?> GetCustomerSummaryAsync(Guid id, CancellationToken ct = default)
        => apiClient.GetAsync<CustomerSummary>($"/api/customers/{id}/summary", authorized: true, ct);
}
