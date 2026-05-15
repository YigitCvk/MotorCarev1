namespace MotorCare.App.Models.PublicRecords;

public sealed class PublicServiceRecordOperation
{
    public string Description { get; set; } = string.Empty;
}

public sealed class PublicServiceRecordPart
{
    public string PartName { get; set; } = string.Empty;
    public string? PartNumber { get; set; }
    public int Quantity { get; set; }
}

public sealed class PublicServiceRecordConsumable
{
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? Specification { get; set; }
    public string? Notes { get; set; }
}

public sealed class PublicServiceRecordPayment
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTimeOffset PaymentDate { get; set; }
}

public sealed class PublicServiceRecordTotals
{
    public decimal LaborTotal { get; set; }
    public decimal PartsTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidTotal { get; set; }
    public decimal RemainingTotal { get; set; }
}

public sealed class PublicServiceRecordPaymentSummary
{
    public int PaymentCount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal CashTotal { get; set; }
    public decimal CreditCardTotal { get; set; }
    public decimal BankTransferTotal { get; set; }
}

public sealed class PublicServiceRecord
{
    public string OrderNo { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string? MaskedCustomerDisplayName { get; set; }
    public string? VehiclePlate { get; set; }
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
    public int VehicleKm { get; set; }
    public string? WorkDescription { get; set; }
    public string ServiceSummary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ClosedAt { get; set; }
    public List<PublicServiceRecordOperation> Operations { get; set; } = [];
    public List<PublicServiceRecordPart> Parts { get; set; } = [];
    public List<PublicServiceRecordConsumable> Consumables { get; set; } = [];
    public PublicServiceRecordTotals Totals { get; set; } = new();
    public PublicServiceRecordPaymentSummary PaymentSummary { get; set; } = new();
    public List<PublicServiceRecordPayment> Payments { get; set; } = [];
    public string? BusinessName { get; set; }
    public string VerificationText { get; set; } = string.Empty;
}
