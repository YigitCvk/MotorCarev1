namespace MotorCare.App.Models.Vehicles;

public sealed class VehicleServiceHistoryResponse
{
    public Guid VehicleId { get; set; }
    public string Plate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string VehicleDisplay { get; set; } = string.Empty;
    public int? CurrentKm { get; set; }
    public int TotalServiceOrderCount { get; set; }
    public DateTimeOffset? LastServiceDate { get; set; }
    public decimal TotalSpent { get; set; }
    public List<VehicleServiceHistoryItem> ServiceOrders { get; set; } = [];
}

public sealed class VehicleServiceHistoryItem
{
    public Guid Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string? Complaint { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidTotal { get; set; }
    public decimal RemainingTotal { get; set; }
}
