using MediatR;

namespace MotorCare.Application.Dashboard.Queries.GetDailySummary;

public sealed record DailySummaryDto(
    int TotalServiceOrdersToday,
    int OpenServiceOrders,
    int CompletedServiceOrdersToday,
    decimal TotalPaymentsToday,
    decimal TotalRevenueToday,
    decimal PendingAmount);

public sealed record GetDailySummaryQuery : IRequest<DailySummaryDto>;
