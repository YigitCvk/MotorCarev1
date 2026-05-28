using MediatR;

namespace MotorCare.Application.ServiceOrders.Queries.GetPaymentSummary;

public sealed record GetPaymentSummaryQuery(
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<PaymentSummaryDto>;

public sealed record PaymentSummaryDto(
    decimal TotalCollected,
    decimal CashTotal,
    decimal CreditCardTotal,
    decimal BankTransferTotal,
    decimal OpenBalance,
    int TotalOrdersInPeriod,
    int PaidOrdersCount,
    int PartiallyPaidOrdersCount,
    int UnpaidOrdersCount,
    IReadOnlyList<DailyPaymentSummary> DailyBreakdown);

public sealed record DailyPaymentSummary(
    DateOnly Date,
    decimal Total,
    decimal Cash,
    decimal CreditCard,
    decimal BankTransfer,
    int PaymentCount);
