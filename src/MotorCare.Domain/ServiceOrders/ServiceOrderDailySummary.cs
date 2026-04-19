namespace MotorCare.Domain.ServiceOrders;

public sealed record ServiceOrderDailySummary(
    int TotalServiceOrdersToday,
    int ActiveServiceOrders,
    int CompletedServiceOrdersToday,
    int DeliveryWaitingCount,
    decimal TotalPaymentsToday,
    decimal TotalRevenueToday,
    decimal PendingAmount);
