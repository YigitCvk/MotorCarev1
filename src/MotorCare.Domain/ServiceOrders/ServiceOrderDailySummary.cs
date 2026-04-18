namespace MotorCare.Domain.ServiceOrders;

public sealed record ServiceOrderDailySummary(
    int TotalServiceOrdersToday,
    int OpenServiceOrders,
    int CompletedServiceOrdersToday,
    decimal TotalPaymentsToday,
    decimal TotalRevenueToday,
    decimal PendingAmount);
