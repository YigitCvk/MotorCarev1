namespace MotorCare.App.Models.ServiceOrders;

public sealed class CreateServiceOrderRequest
{
    public Guid VehicleId { get; set; }
    public Guid CustomerId { get; set; }
    public int VehicleKm { get; set; }
    public string? Complaint { get; set; }
    public IReadOnlyList<CreateServiceOrderConsumableRequest> Consumables { get; set; } = [];
}

public sealed class CreateServiceOrderConsumableRequest
{
    public string Category { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? SubCategory { get; set; }
    public string? Specification { get; set; }
    public string? Notes { get; set; }
}
