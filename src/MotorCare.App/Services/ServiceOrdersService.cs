using System.Globalization;
using System.Text;
using MotorCare.App.Models.Customers;
using MotorCare.App.Models.ServiceOrders;
using MotorCare.App.Models.Vehicles;

namespace MotorCare.App.Services;

public sealed class ServiceOrdersService
{
    private readonly ApiClient _apiClient;

    public ServiceOrdersService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<IReadOnlyList<ServiceOrderResponse>?> GetServiceOrdersAsync(
        string? searchText,
        string? status,
        DateTime? openedFrom,
        DateTime? openedTo,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query.Add($"q={Uri.EscapeDataString(searchText)}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query.Add($"status={Uri.EscapeDataString(status)}");
        }

        if (openedFrom.HasValue)
        {
            query.Add($"openedFrom={Uri.EscapeDataString(openedFrom.Value.ToString("O", CultureInfo.InvariantCulture))}");
        }

        if (openedTo.HasValue)
        {
            query.Add($"openedTo={Uri.EscapeDataString(openedTo.Value.ToString("O", CultureInfo.InvariantCulture))}");
        }

        var uriBuilder = new StringBuilder("/api/service-orders");
        if (query.Count > 0)
        {
            uriBuilder.Append('?').Append(string.Join('&', query));
        }

        return _apiClient.GetAsync<IReadOnlyList<ServiceOrderResponse>>(uriBuilder.ToString(), authorized: true, cancellationToken);
    }

    public Task<IReadOnlyList<CustomerLookupResponse>?> SearchCustomersAsync(string? searchText, CancellationToken cancellationToken = default)
    {
        var uri = string.IsNullOrWhiteSpace(searchText)
            ? "/api/customers"
            : $"/api/customers?q={Uri.EscapeDataString(searchText)}";

        return _apiClient.GetAsync<IReadOnlyList<CustomerLookupResponse>>(uri, authorized: true, cancellationToken);
    }

    public Task<VehicleLookupResponse?> GetVehicleByPlateAsync(string plate, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<VehicleLookupResponse>($"/api/vehicles/{Uri.EscapeDataString(plate)}", authorized: true, cancellationToken);
    }

    public Task<Guid> CreateServiceOrderAsync(CreateServiceOrderRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<CreateServiceOrderRequest, Guid>("/api/service-orders", request, authorized: true, cancellationToken)!;
    }

    public Task<ServiceOrderResponse?> GetServiceOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<ServiceOrderResponse>($"/api/service-orders/{id}", authorized: true, cancellationToken);
    }
}
