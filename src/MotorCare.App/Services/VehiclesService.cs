using MotorCare.App.Models.Vehicles;

namespace MotorCare.App.Services;

public sealed class VehiclesService(ApiClient apiClient)
{
    public Task<IReadOnlyList<VehicleLookupResponse>?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        => apiClient.GetAsync<IReadOnlyList<VehicleLookupResponse>>($"/api/customers/{customerId}/vehicles", authorized: true, ct);

    public Task<VehicleLookupResponse?> GetByPlateAsync(string plate, CancellationToken ct = default)
        => apiClient.GetAsync<VehicleLookupResponse>(
            $"/api/vehicles/{Uri.EscapeDataString(plate)}", authorized: true, ct);

    public Task<Guid> CreateAsync(CreateVehicleRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateVehicleRequest, Guid>("/api/vehicles", request, authorized: true, ct)!;
}
