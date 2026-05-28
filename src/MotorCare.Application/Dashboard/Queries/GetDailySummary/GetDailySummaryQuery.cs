using MediatR;

namespace MotorCare.Application.Dashboard.Queries.GetDailySummary;

public sealed record DailySummaryDto(
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
    IReadOnlyList<RecentServiceOrderDto> RecentServiceOrders);

public sealed record RecentServiceOrderDto(
    Guid Id,
    string OrderNo,
    string Status,
    DateTimeOffset OpenedAt,
    decimal GrandTotal);

public sealed record GetDailySummaryQuery : IRequest<DailySummaryDto>;
