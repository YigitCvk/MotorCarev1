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
    public decimal RemainingTotal => Math.Max(0, GrandTotal - PaidTotal);

    private readonly List<ServiceOperationItem> _operations = new();
    public IReadOnlyCollection<ServiceOperationItem> Operations => _operations;

    private readonly List<ServicePartItem> _parts = new();
    public IReadOnlyCollection<ServicePartItem> Parts => _parts;

    private readonly List<ServicePayment> _payments = new();
    public IReadOnlyCollection<ServicePayment> Payments => _payments;

    private ServiceOrder() { }

    public ServiceOrder(string tenantId, string orderNo, Guid vehicleId, Guid customerId, int vehicleKm, string? complaint)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new DomainException("Tenant ID is required.");
        if (string.IsNullOrWhiteSpace(orderNo)) throw new DomainException("Order number is required.");
        if (vehicleId == Guid.Empty) throw new DomainException("Vehicle ID is required.");
        if (customerId == Guid.Empty) throw new DomainException("Customer ID is required.");
        if (vehicleKm < 0) throw new DomainException("Vehicle km cannot be negative.");
        
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

    // --- Operations ---

    public void AddOperation(string description, decimal price)
    {
        EnsureModifiable();
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");
        if (price < 0) throw new DomainException("Price cannot be negative.");
        _operations.Add(new ServiceOperationItem(description, price));
        RecalculateTotals();
    }

    public void RemoveOperation(Guid operationId)
    {
        EnsureModifiable();
        var operation = _operations.FirstOrDefault(o => o.Id == operationId)
            ?? throw new DomainException($"Operation with id '{operationId}' not found in this service order.");

        EnsurePaidTotalWillFit(CalculateGrandTotal(LaborTotal - operation.Price, PartsTotal, DiscountTotal));

        _operations.Remove(operation);
        RecalculateTotals();
    }

    // --- Parts ---

    public void AddPart(string partName, string? partNumber, decimal unitPrice, int quantity)
    {
        EnsureModifiable();
        if (string.IsNullOrWhiteSpace(partName)) throw new DomainException("Part name is required.");
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");
        _parts.Add(new ServicePartItem(partName, partNumber, unitPrice, quantity));
        RecalculateTotals();
    }

    public void RemovePart(Guid partId)
    {
        EnsureModifiable();
        var part = _parts.FirstOrDefault(p => p.Id == partId)
            ?? throw new DomainException($"Part with id '{partId}' not found in this service order.");

        EnsurePaidTotalWillFit(CalculateGrandTotal(LaborTotal, PartsTotal - part.TotalPrice, DiscountTotal));

        _parts.Remove(part);
        RecalculateTotals();
    }

    // --- Payments ---

    public void AddPayment(decimal amount, PaymentMethod method, DateTimeOffset paymentDate)
    {
        EnsureModifiable();
        if (amount <= 0) throw new DomainException("Payment amount must be greater than zero.");
        if (PaidTotal + amount > GrandTotal)
        {
            throw new DomainException("Bu odeme eklenemez. Alinan odeme toplam tutari asiyor.");
        }

        _payments.Add(new ServicePayment(amount, method, paymentDate));
        RecalculateTotals();
    }

    // --- Discount ---

    public void SetDiscount(decimal discount)
    {
        EnsureModifiable();
        if (discount < 0) throw new DomainException("Discount cannot be negative.");
        if (discount > (LaborTotal + PartsTotal)) throw new DomainException("Discount cannot exceed labor and parts total.");

        EnsurePaidTotalWillFit(CalculateGrandTotal(LaborTotal, PartsTotal, discount));
        
        DiscountTotal = discount;
        RecalculateTotals();
    }

    // --- Status Transitions ---

    /// <summary>Open → InProgress</summary>
    public void StartProgress()
    {
        if (Status != ServiceOrderStatus.Open)
            throw new DomainException($"Cannot start progress from {Status} state. Order must be in Open state.");
        
        Status = ServiceOrderStatus.InProgress;
    }

    /// <summary>InProgress → WaitingForParts</summary>
    public void WaitForParts()
    {
        if (Status != ServiceOrderStatus.InProgress)
            throw new DomainException($"Cannot set waiting for parts from {Status} state. Order must be in InProgress state.");
        
        Status = ServiceOrderStatus.WaitingForParts;
    }

    /// <summary>WaitingForParts → InProgress</summary>
    public void ResumeProgress()
    {
        if (Status != ServiceOrderStatus.WaitingForParts)
            throw new DomainException($"Cannot resume progress from {Status} state. Order must be in WaitingForParts state.");
        
        Status = ServiceOrderStatus.InProgress;
    }

    /// <summary>InProgress | WaitingForParts → Completed</summary>
    public void MarkAsCompleted()
    {
        if (Status != ServiceOrderStatus.InProgress && Status != ServiceOrderStatus.WaitingForParts)
            throw new DomainException($"Cannot complete order from {Status} state. Order must be in InProgress or WaitingForParts state.");
        
        Status = ServiceOrderStatus.Completed;
        ClosedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Completed → Delivered</summary>
    public void MarkAsDelivered()
    {
        if (Status != ServiceOrderStatus.Completed)
            throw new DomainException($"Cannot mark as delivered from {Status} state. Order must be in Completed state.");
        
        Status = ServiceOrderStatus.Delivered;
    }

    /// <summary>Open | InProgress | WaitingForParts → Cancelled</summary>
    public void Cancel()
    {
        if (Status != ServiceOrderStatus.Open &&
            Status != ServiceOrderStatus.InProgress &&
            Status != ServiceOrderStatus.WaitingForParts)
        {
            throw new DomainException($"Cannot cancel order from {Status} state. Only Open, InProgress, or WaitingForParts orders can be cancelled.");
        }

        Status = ServiceOrderStatus.Cancelled;
        ClosedAt = DateTimeOffset.UtcNow;
    }

    // --- Work Description ---

    public void UpdateWorkDescription(string? workDescription)
    {
        EnsureModifiable();
        WorkDescription = workDescription;
    }

    public void UpdateInternalNote(string? internalNote)
    {
        EnsureModifiable();
        InternalNote = internalNote;
    }

    // --- Private Helpers ---

    private void RecalculateTotals()
    {
        LaborTotal = _operations.Sum(o => o.Price);
        PartsTotal = _parts.Sum(p => p.TotalPrice);
        GrandTotal = CalculateGrandTotal(LaborTotal, PartsTotal, DiscountTotal);
        PaidTotal = _payments.Sum(p => p.Amount);
    }

    private void EnsurePaidTotalWillFit(decimal projectedGrandTotal)
    {
        if (PaidTotal > projectedGrandTotal)
        {
            throw new DomainException("Bu islem silinemez. Alinan odeme toplam tutardan fazla kaliyor. Once odemeyi duzenleyin veya iade islemi olusturun.");
        }
    }

    private static decimal CalculateGrandTotal(decimal laborTotal, decimal partsTotal, decimal discountTotal)
    {
        var total = (laborTotal + partsTotal) - discountTotal;
        return total < 0 ? 0 : total;
    }

    private void EnsureModifiable()
    {
        if (Status == ServiceOrderStatus.Completed ||
            Status == ServiceOrderStatus.Cancelled ||
            Status == ServiceOrderStatus.Delivered)
        {
            throw new DomainException($"Cannot modify Service Order in {Status} state.");
        }
    }
}
