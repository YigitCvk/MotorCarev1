namespace MotorCare.App.Models.Dashboard;

public sealed class DailySummaryResponse
{
    public int TodayAppointmentsCount { get; set; }
    public int ActiveServiceOrdersCount { get; set; }
    public int CompletedServiceOrdersCount { get; set; }
    public int DeliveryWaitingCount { get; set; }
    public decimal DailyRevenue { get; set; }
    public decimal TotalPaymentsToday { get; set; }
    public decimal PendingAmount { get; set; }
    public List<DashboardAppointmentItem> TodayAppointments { get; set; } = [];
    public List<DashboardServiceOrderItem> RecentServiceOrders { get; set; } = [];
}

public sealed class DashboardAppointmentItem
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Plate { get; set; }
    public int Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
}

public sealed class DashboardServiceOrderItem
{
    public Guid Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? VehiclePlate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
    public decimal GrandTotal { get; set; }
}
