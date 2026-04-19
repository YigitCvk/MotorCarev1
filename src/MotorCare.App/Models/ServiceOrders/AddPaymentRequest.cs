namespace MotorCare.App.Models.ServiceOrders;

public sealed class AddPaymentRequest
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public DateTimeOffset? PaymentDate { get; set; }
}
