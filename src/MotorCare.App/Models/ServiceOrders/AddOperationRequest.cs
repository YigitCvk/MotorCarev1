namespace MotorCare.App.Models.ServiceOrders;

public sealed class AddOperationRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
