namespace MotorCare.App.Models.ServiceOrders;

public class ServiceOrderListItem
{
    public Guid Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? VehiclePlate { get; set; }
    public string? VehicleDisplay { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public int VehicleKm { get; set; }
    public string? Complaint { get; set; }
    public string? WorkDescription { get; set; }
    public string? InternalNote { get; set; }
    public decimal LaborTotal { get; set; }
    public decimal PartsTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidTotal { get; set; }
    public decimal RemainingTotal { get; set; }
}
