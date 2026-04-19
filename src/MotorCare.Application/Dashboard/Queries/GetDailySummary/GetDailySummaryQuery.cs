using MediatR;

namespace MotorCare.Application.Dashboard.Queries.GetDailySummary;

public sealed record DailySummaryDto(
    int TodayAppointmentsCount,
    int ActiveServiceOrdersCount,
    int CompletedServiceOrdersCount,
    int DeliveryWaitingCount,
    decimal DailyRevenue,
    decimal TotalPaymentsToday,
    decimal PendingAmount,
    IReadOnlyList<DashboardAppointmentItemDto> TodayAppointments,
    IReadOnlyList<DashboardServiceOrderItemDto> RecentServiceOrders);

public sealed record DashboardAppointmentItemDto(
    Guid Id,
    string CustomerName,
    string Phone,
    string? Plate,
    int Type,
    string TypeText,
    int Status,
    string StatusText,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt);

public sealed record DashboardServiceOrderItemDto(
    Guid Id,
    string OrderNo,
    string? CustomerName,
    string? VehiclePlate,
    string Status,
    string StatusText,
    DateTimeOffset OpenedAt,
    decimal GrandTotal);

public sealed record GetDailySummaryQuery : IRequest<DailySummaryDto>;
