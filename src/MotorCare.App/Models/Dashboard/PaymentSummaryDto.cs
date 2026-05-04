namespace MotorCare.App.Models.Dashboard;

public sealed class PaymentSummaryDto
{
    public decimal TotalCollected { get; set; }
    public decimal CashTotal { get; set; }
    public decimal CreditCardTotal { get; set; }
    public decimal BankTransferTotal { get; set; }
    public decimal OpenBalance { get; set; }
    public int TotalOrdersInPeriod { get; set; }
    public int PaidOrdersCount { get; set; }
    public int PartiallyPaidOrdersCount { get; set; }
    public int UnpaidOrdersCount { get; set; }
    public List<DailyPaymentSummary> DailyBreakdown { get; set; } = [];
}

public sealed class DailyPaymentSummary
{
    public DateOnly Date { get; set; }
    public decimal Total { get; set; }
    public decimal Cash { get; set; }
    public decimal CreditCard { get; set; }
    public decimal BankTransfer { get; set; }
    public int PaymentCount { get; set; }
}
