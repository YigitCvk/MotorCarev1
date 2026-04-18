namespace MotorCare.App.Models.ServiceOrders;

public sealed class ServiceOperationItemResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public sealed class ServicePartItemResponse
{
    public Guid Id { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public sealed class ServicePaymentResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTimeOffset PaymentDate { get; set; }
}

public sealed class ServiceOrderResponse
{
    public Guid Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public int VehicleKm { get; set; }
    public string? Complaint { get; set; }
    public string? WorkDescription { get; set; }
    public string? InternalNote { get; set; }
    public decimal LaborTotal { get; set; }
    public decimal PartsTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidTotal { get; set; }
    public decimal RemainingTotal { get; set; }
    public IReadOnlyList<ServiceOperationItemResponse> Operations { get; set; } = [];
    public IReadOnlyList<ServicePartItemResponse> Parts { get; set; } = [];
    public IReadOnlyList<ServicePaymentResponse> Payments { get; set; } = [];
}
