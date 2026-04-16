using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;
using MotorCare.Domain.ServiceOrders.Entities;

namespace MotorCare.Domain.ServiceOrders;

public class ServiceOrder : AggregateRoot, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public string OrderNo { get; private set; } = string.Empty;
    public Guid VehicleId { get; private set; }
    public Guid CustomerId { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public int VehicleKm { get; private set; }
    
    public string? Complaint { get; private set; }
    public string? WorkDescription { get; private set; }
    public string? InternalNote { get; private set; }

    public decimal LaborTotal { get; private set; }
    public decimal PartsTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public decimal PaidTotal { get; private set; }
    public decimal RemainingTotal => GrandTotal - PaidTotal;

    private readonly List<ServiceOperationItem> _operations = new();
    public IReadOnlyCollection<ServiceOperationItem> Operations => _operations.AsReadOnly();

    private readonly List<ServicePartItem> _parts = new();
    public IReadOnlyCollection<ServicePartItem> Parts => _parts.AsReadOnly();

    private readonly List<ServicePayment> _payments = new();
    public IReadOnlyCollection<ServicePayment> Payments => _payments.AsReadOnly();

    private ServiceOrder() { }

    public ServiceOrder(string tenantId, string orderNo, Guid vehicleId, Guid customerId, int vehicleKm, string? complaint)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("Tenant ID is required.");
        if (string.IsNullOrWhiteSpace(orderNo)) throw new ArgumentException("Order number is required.");
        
        Id = Guid.NewGuid();
        TenantId = tenantId;
        OrderNo = orderNo;
        VehicleId = vehicleId;
        CustomerId = customerId;
        VehicleKm = vehicleKm;
        Complaint = complaint;
        Status = ServiceOrderStatus.Open;
        OpenedAt = DateTimeOffset.UtcNow;
    }

    public void AddOperation(string description, decimal price)
    {
        EnsureModifiable();
        _operations.Add(new ServiceOperationItem(description, price));
        RecalculateTotals();
    }

    public void AddPart(string partName, string? partNumber, decimal unitPrice, int quantity)
    {
        EnsureModifiable();
        _parts.Add(new ServicePartItem(partName, partNumber, unitPrice, quantity));
        RecalculateTotals();
    }

    public void AddPayment(decimal amount, PaymentMethod method, DateTimeOffset paymentDate)
    {
        if (Status == ServiceOrderStatus.Cancelled)
            throw new DomainException("Cannot process payments for a cancelled order.");
            
        if (amount <= 0) throw new DomainException("Payment amount must be greater than zero.");
        if (amount > RemainingTotal) throw new DomainException($"Payment amount ({amount}) exceeds the remaining total ({RemainingTotal}).");

        _payments.Add(new ServicePayment(amount, method, paymentDate));
        
        PaidTotal = _payments.Sum(p => p.Amount);
    }

    public void MarkAsCompleted()
    {
        if (Status == ServiceOrderStatus.Cancelled) throw new DomainException("Cannot complete a cancelled order.");
        
        Status = ServiceOrderStatus.Completed;
        ClosedAt = DateTimeOffset.UtcNow;
    }

    public void SetDiscount(decimal discount)
    {
        EnsureModifiable();
        if (discount < 0) throw new DomainException("Discount cannot be negative.");
        if (discount > (LaborTotal + PartsTotal)) throw new DomainException("Discount cannot exceed labor and parts total.");
        
        DiscountTotal = discount;
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        LaborTotal = _operations.Sum(o => o.Price);
        PartsTotal = _parts.Sum(p => p.TotalPrice);
        GrandTotal = (LaborTotal + PartsTotal) - DiscountTotal;
        
        if (GrandTotal < 0) GrandTotal = 0;
        
        PaidTotal = _payments.Sum(p => p.Amount);
    }

    private void EnsureModifiable()
    {
        if (Status == ServiceOrderStatus.Completed || Status == ServiceOrderStatus.Cancelled)
        {
            throw new DomainException($"Cannot modify Service Order in {Status} state.");
        }
    }
}
