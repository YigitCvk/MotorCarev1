namespace MotorCare.App.Models.ServiceOrders;

public sealed class ServiceOrderStatusHistoryItem
{
    public Guid Id { get; set; }
    public string? FromStatus { get; set; }
    public string? FromStatusText { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public string ToStatusText { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? ChangedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
