using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;

namespace MotorCare.Domain.ServiceOrders.Entities;

public class ServicePayment : AuditableEntity
{
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTimeOffset PaymentDate { get; private set; }

    private ServicePayment() { }

    internal ServicePayment(decimal amount, PaymentMethod method, DateTimeOffset paymentDate)
    {
        if (amount <= 0) throw new DomainException("Payment amount must be greater than zero.");

        Id = Guid.NewGuid();
        Amount = amount;
        Method = method;
        PaymentDate = paymentDate;
    }
}
