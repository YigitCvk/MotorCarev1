using MotorCare.App.Models.Customers;

namespace MotorCare.App.Services;

public sealed class CustomersService(ApiClient apiClient)
{
    public Task<IReadOnlyList<CustomerLookupResponse>?> SearchCustomersAsync(
        string? searchText,
        CancellationToken ct = default)
    {
        var uri = string.IsNullOrWhiteSpace(searchText)
            ? "/api/customers"
            : $"/api/customers?q={Uri.EscapeDataString(searchText)}";

        return apiClient.GetAsync<IReadOnlyList<CustomerLookupResponse>>(uri, authorized: true, ct);
    }

    public Task<CustomerLookupResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => apiClient.GetAsync<CustomerLookupResponse>($"/api/customers/{id}", authorized: true, ct);

    public Task<Guid> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateCustomerRequest, Guid>("/api/customers", request, authorized: true, ct)!;

    public Task UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/customers/{id}", request, authorized: true, ct);
}
