namespace MotorCare.App.Models.ServiceOrders;

public sealed class UpdateServiceOrderStatusRequest
{
    public string Status { get; set; } = "Open";
}
