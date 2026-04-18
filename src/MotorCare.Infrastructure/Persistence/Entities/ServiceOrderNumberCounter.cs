namespace MotorCare.Infrastructure.Persistence.Entities;

public class ServiceOrderNumberCounter
{
    public string TenantId { get; private set; } = string.Empty;
    public DateTime CounterDate { get; private set; }
    public int LastValue { get; private set; }

    private ServiceOrderNumberCounter()
    {
    }
}
