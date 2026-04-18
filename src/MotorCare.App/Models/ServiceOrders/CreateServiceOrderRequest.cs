namespace MotorCare.App.Models.ServiceOrders;

public sealed class CreateServiceOrderRequest
{
    public Guid VehicleId { get; set; }
    public Guid CustomerId { get; set; }
    public int VehicleKm { get; set; }
    public string? Complaint { get; set; }
}
