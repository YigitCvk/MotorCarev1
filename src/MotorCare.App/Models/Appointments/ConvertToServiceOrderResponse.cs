namespace MotorCare.App.Models.Appointments;

public class ConvertToServiceOrderResponse
{
    public Guid ServiceOrderId { get; set; }
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    public Guid ResolveServiceOrderId()
    {
        if (ServiceOrderId != Guid.Empty)
        {
            return ServiceOrderId;
        }

        if (Id != Guid.Empty)
        {
            return Id;
        }

        return OrderId;
    }
}
