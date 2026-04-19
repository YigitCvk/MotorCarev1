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

    public Task<IReadOnlyList<ServiceOrderListItem>?> GetServiceOrdersAsync(
        string? searchText,
        string? status,
        DateTime? openedFrom,
        DateTime? openedTo,
        Guid? customerId = null,
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
            var startUtc = DateTime.SpecifyKind(openedFrom.Value.Date, DateTimeKind.Utc);
            query.Add($"openedFrom={Uri.EscapeDataString(new DateTimeOffset(startUtc).ToString("O", CultureInfo.InvariantCulture))}");
        }

        if (openedTo.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(openedTo.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query.Add($"openedTo={Uri.EscapeDataString(new DateTimeOffset(endUtc).ToString("O", CultureInfo.InvariantCulture))}");
        }

        if (customerId.HasValue)
        {
            query.Add($"customerId={customerId.Value}");
        }

        var uriBuilder = new StringBuilder("/api/service-orders");
        if (query.Count > 0)
        {
            uriBuilder.Append('?').Append(string.Join('&', query));
        }

        return _apiClient.GetAsync<IReadOnlyList<ServiceOrderListItem>>(uriBuilder.ToString(), authorized: true, cancellationToken);
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

    public Task<ServiceOrderDetail?> GetServiceOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<ServiceOrderDetail>($"/api/service-orders/{id}", authorized: true, cancellationToken);
    }

    public Task AddOperationAsync(Guid id, AddOperationRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync($"/api/service-orders/{id}/operations", request, authorized: true, cancellationToken);
    }

    public Task AddPartAsync(Guid id, AddPartRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync($"/api/service-orders/{id}/parts", request, authorized: true, cancellationToken);
    }

    public Task AddPaymentAsync(Guid id, AddPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new AddPaymentRequest
        {
            Amount = request.Amount,
            Method = request.Method,
            PaymentDate = request.PaymentDate?.ToUniversalTime()
        };

        return _apiClient.PostAsync($"/api/service-orders/{id}/payments", payload, authorized: true, cancellationToken);
    }

    public Task UpdateStatusAsync(Guid id, UpdateServiceOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PutAsync($"/api/service-orders/{id}/status", request, authorized: true, cancellationToken);
    }
}
