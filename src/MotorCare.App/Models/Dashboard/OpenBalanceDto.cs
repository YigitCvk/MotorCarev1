namespace MotorCare.App.Models.Dashboard;

public sealed class OpenBalanceDto
{
    public Guid ServiceOrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? VehiclePlate { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidTotal { get; set; }
    public decimal RemainingTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
}
