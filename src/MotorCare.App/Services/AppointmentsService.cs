using MotorCare.App.Models.Appointments;

namespace MotorCare.App.Services;

public sealed class AppointmentsService(ApiClient apiClient)
{
    public Task<IReadOnlyList<AppointmentDto>?> GetAppointmentsAsync(
        DateTimeOffset? startFrom = null,
        DateTimeOffset? endTo = null,
        AppointmentStatus? status = null,
        AppointmentType? type = null,
        string? q = null,
        CancellationToken ct = default)
    {
        var parts = new List<string>();
        if (startFrom.HasValue) parts.Add($"startFrom={Uri.EscapeDataString(startFrom.Value.ToString("O"))}");
        if (endTo.HasValue) parts.Add($"endTo={Uri.EscapeDataString(endTo.Value.ToString("O"))}");
        if (status.HasValue) parts.Add($"status={(int)status.Value}");
        if (type.HasValue) parts.Add($"type={(int)type.Value}");
        if (!string.IsNullOrWhiteSpace(q)) parts.Add($"q={Uri.EscapeDataString(q)}");

        var uri = parts.Count > 0
            ? $"/api/appointments?{string.Join("&", parts)}"
            : "/api/appointments";

        return apiClient.GetAsync<IReadOnlyList<AppointmentDto>>(uri, authorized: true, ct);
    }

    public Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => apiClient.GetAsync<AppointmentDto>($"/api/appointments/{id}", authorized: true, ct);

    public Task<AppointmentDto?> CreateAsync(CreateAppointmentRequest request, CancellationToken ct = default)
        => apiClient.PostAsync<CreateAppointmentRequest, AppointmentDto>("/api/appointments", request, authorized: true, ct);

    public Task<AppointmentDto?> UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken ct = default)
        => apiClient.PutAsync<UpdateAppointmentRequest, AppointmentDto>($"/api/appointments/{id}", request, authorized: true, ct);

    public Task UpdateStatusAsync(Guid id, AppointmentStatus status, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/appointments/{id}/status", new UpdateAppointmentStatusRequest { Status = status }, authorized: true, ct);

    public Task<ConvertToServiceOrderResponse?> ConvertToServiceOrderAsync(Guid id, int vehicleKm, CancellationToken ct = default)
        => apiClient.PostAsync<ConvertToServiceOrderRequest, ConvertToServiceOrderResponse>(
            $"/api/appointments/{id}/convert-to-service-order",
            new ConvertToServiceOrderRequest { VehicleKm = vehicleKm },
            authorized: true,
            ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => apiClient.PutAsync($"/api/appointments/{id}/status", new UpdateAppointmentStatusRequest { Status = AppointmentStatus.Cancelled }, authorized: true, ct);
}
