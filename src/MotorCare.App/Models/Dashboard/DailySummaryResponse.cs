namespace MotorCare.App.Models.Dashboard;

public sealed class DailySummaryResponse
{
    public int TotalServiceOrdersToday { get; set; }
    public int OpenServiceOrders { get; set; }
    public int CompletedServiceOrdersToday { get; set; }
    public decimal TotalPaymentsToday { get; set; }
    public decimal TotalRevenueToday { get; set; }
    public decimal PendingAmount { get; set; }
}
