using System.Globalization;
using MotorCare.App.Models.Dashboard;

namespace MotorCare.App.Services;

public sealed class DashboardService
{
    private readonly ApiClient _apiClient;

    public DashboardService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<DailySummaryResponse?> GetDailySummaryAsync(CancellationToken ct = default)
        => _apiClient.GetAsync<DailySummaryResponse>("/api/dashboard/daily", authorized: true, ct);

    public Task<PaymentSummaryDto?> GetPaymentSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var fromUtc = new DateTimeOffset(DateTime.SpecifyKind(from.Date, DateTimeKind.Utc));
        var toUtc = new DateTimeOffset(DateTime.SpecifyKind(to.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc));
        var uri = $"/api/dashboard/payment-summary?from={Uri.EscapeDataString(fromUtc.ToString("O", CultureInfo.InvariantCulture))}&to={Uri.EscapeDataString(toUtc.ToString("O", CultureInfo.InvariantCulture))}";
        return _apiClient.GetAsync<PaymentSummaryDto>(uri, authorized: true, ct);
    }

    public Task<IReadOnlyList<OpenBalanceDto>?> GetOpenBalancesAsync(int take = 50, CancellationToken ct = default)
        => _apiClient.GetAsync<IReadOnlyList<OpenBalanceDto>>($"/api/dashboard/open-balances?take={take}", authorized: true, ct);
}
