namespace MotorCare.Domain.ServiceOrders;

public sealed record ServiceOrderDailySummary(
    int TotalServiceOrdersToday,
    int ActiveServiceOrders,
    int OpenServiceOrders,
    int InProgressServiceOrders,
    int WaitingForPartsServiceOrders,
    int CompletedServiceOrdersToday,
    int DeliveryWaitingCount,
    decimal TotalPaymentsToday,
    decimal TotalPaymentsThisMonth,
    decimal TotalRevenueToday,
    decimal PendingAmount);
