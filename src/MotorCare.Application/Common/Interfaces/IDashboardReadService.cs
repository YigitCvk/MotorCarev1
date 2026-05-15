namespace MotorCare.Application.Common.Interfaces;

public interface IDashboardReadService
{
    Task<DashboardOverviewReadModel> GetOverviewAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}

public sealed record DashboardOverviewReadModel(
    int TotalCustomerCount,
    int TotalVehicleCount,
    int OpenServiceOrderCount,
    int TodayAppointmentCount,
    int CompletedServiceCountThisMonth,
    decimal TotalPaymentsThisMonth,
    int CriticalInspectionCount,
    int TotalServiceOrdersToday,
    int CompletedServiceOrdersToday,
    int InProgressServiceOrdersCount,
    decimal TotalPaymentsToday,
    int ActiveServiceOrdersCount,
    IReadOnlyList<DashboardRecentServiceOrderReadModel> RecentServiceOrders);

public sealed record DashboardRecentServiceOrderReadModel(
    Guid Id,
    string OrderNo,
    string Status,
    DateTimeOffset OpenedAt,
    decimal GrandTotal);
