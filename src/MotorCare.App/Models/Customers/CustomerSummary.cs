namespace MotorCare.App.Models.Customers;

public class CustomerSummary
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Whatsapp { get; set; }
    public string? Notes { get; set; }
    public List<CustomerVehicleItem> Vehicles { get; set; } = [];
    public List<CustomerAppointmentHistoryItem> Appointments { get; set; } = [];
    public List<CustomerServiceOrderHistoryItem> ServiceOrders { get; set; } = [];
}

public class CustomerVehicleItem
{
    public Guid Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string VehicleDisplay { get; set; } = string.Empty;
    public int? CurrentKm { get; set; }
    public DateTimeOffset? LastServiceDate { get; set; }
}

public class CustomerAppointmentHistoryItem
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public string? Plate { get; set; }
    public Guid? ServiceOrderId { get; set; }
}

public class CustomerServiceOrderHistoryItem
{
    public Guid Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? VehiclePlate { get; set; }
    public string? Complaint { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidTotal { get; set; }
    public List<CustomerOperationHistoryItem> Operations { get; set; } = [];
    public List<CustomerPartHistoryItem> Parts { get; set; } = [];
    public List<CustomerPaymentHistoryItem> Payments { get; set; } = [];
}

public class CustomerOperationHistoryItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CustomerPartHistoryItem
{
    public string PartName { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CustomerPaymentHistoryItem
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string MethodText { get; set; } = string.Empty;
    public DateTimeOffset PaymentDate { get; set; }
}
