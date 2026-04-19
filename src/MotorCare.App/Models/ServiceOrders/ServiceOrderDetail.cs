namespace MotorCare.App.Models.ServiceOrders;

public sealed class ServiceOrderOperation
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public sealed class ServiceOrderPart
{
    public Guid Id { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public sealed class ServiceOrderPayment
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTimeOffset PaymentDate { get; set; }
}

public sealed class ServiceOrderDetail : ServiceOrderListItem
{
    public IReadOnlyList<ServiceOrderOperation> Operations { get; set; } = [];
    public IReadOnlyList<ServiceOrderPart> Parts { get; set; } = [];
    public IReadOnlyList<ServiceOrderPayment> Payments { get; set; } = [];
}
