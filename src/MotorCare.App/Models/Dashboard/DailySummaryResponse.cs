namespace MotorCare.App.Models.Dashboard;

public sealed class DailySummaryResponse
{
    public int TotalCustomerCount { get; set; }
    public int TotalVehicleCount { get; set; }
    public int OpenServiceOrderCount { get; set; }
    public int TodayAppointmentCount { get; set; }
    public int CompletedServiceCountThisMonth { get; set; }
    public decimal TotalPaymentsThisMonth { get; set; }
    public int CriticalInspectionCount { get; set; }
    public int TotalServiceOrdersToday { get; set; }
    public int CompletedServiceOrdersToday { get; set; }
    public int InProgressServiceOrdersCount { get; set; }
    public decimal TotalPaymentsToday { get; set; }
    public int ActiveServiceOrdersCount { get; set; }
    public IReadOnlyList<RecentServiceOrderResponse> RecentServiceOrders { get; set; } = [];
}

public sealed class RecentServiceOrderResponse
{
    public Guid Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
    public decimal GrandTotal { get; set; }
}
