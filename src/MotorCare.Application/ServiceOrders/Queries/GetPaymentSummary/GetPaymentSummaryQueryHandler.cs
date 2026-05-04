using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.ServiceOrders.Queries.GetPaymentSummary;

public class GetPaymentSummaryQueryHandler : IRequestHandler<GetPaymentSummaryQuery, PaymentSummaryDto>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetPaymentSummaryQueryHandler(
        IServiceOrderRepository repository,
        ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaymentSummaryDto> Handle(GetPaymentSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var orders = await _repository.GetFilteredAsync(
            tenantId,
            customerId: null,
            status: null,
            searchText: null,
            openedFrom: request.From,
            openedTo: request.To,
            cancellationToken);

        var totalCollected = 0m;
        var cashTotal = 0m;
        var creditCardTotal = 0m;
        var bankTransferTotal = 0m;
        var openBalance = 0m;
        var paidOrdersCount = 0;
        var partiallyPaidOrdersCount = 0;
        var unpaidOrdersCount = 0;

        var dailyMap = new Dictionary<DateOnly, (decimal Total, decimal Cash, decimal CreditCard, decimal BankTransfer, int Count)>();

        foreach (var order in orders)
        {
            // Open balance: orders not cancelled, not delivered
            if (order.Status != ServiceOrderStatus.Cancelled && order.Status != ServiceOrderStatus.Delivered)
            {
                openBalance += order.RemainingTotal;
            }

            // Payment-level aggregation: only payments within the requested period
            var periodPayments = order.Payments
                .Where(p => p.PaymentDate >= request.From && p.PaymentDate <= request.To)
                .ToList();

            foreach (var payment in periodPayments)
            {
                totalCollected += payment.Amount;

                switch (payment.Method)
                {
                    case PaymentMethod.Cash:
                        cashTotal += payment.Amount;
                        break;
                    case PaymentMethod.CreditCard:
                        creditCardTotal += payment.Amount;
                        break;
                    case PaymentMethod.BankTransfer:
                        bankTransferTotal += payment.Amount;
                        break;
                }

                var day = DateOnly.FromDateTime(payment.PaymentDate.ToLocalTime().Date);
                if (!dailyMap.TryGetValue(day, out var existing))
                    existing = (0m, 0m, 0m, 0m, 0);

                var cash = payment.Method == PaymentMethod.Cash ? payment.Amount : 0m;
                var cc = payment.Method == PaymentMethod.CreditCard ? payment.Amount : 0m;
                var bt = payment.Method == PaymentMethod.BankTransfer ? payment.Amount : 0m;

                dailyMap[day] = (
                    existing.Total + payment.Amount,
                    existing.Cash + cash,
                    existing.CreditCard + cc,
                    existing.BankTransfer + bt,
                    existing.Count + 1);
            }

            // Order-level payment status classification
            if (order.RemainingTotal == 0)
                paidOrdersCount++;
            else if (order.PaidTotal > 0)
                partiallyPaidOrdersCount++;
            else
                unpaidOrdersCount++;
        }

        var dailyBreakdown = dailyMap
            .OrderBy(kv => kv.Key)
            .Select(kv => new DailyPaymentSummary(
                kv.Key,
                kv.Value.Total,
                kv.Value.Cash,
                kv.Value.CreditCard,
                kv.Value.BankTransfer,
                kv.Value.Count))
            .ToList();

        return new PaymentSummaryDto(
            totalCollected,
            cashTotal,
            creditCardTotal,
            bankTransferTotal,
            openBalance,
            orders.Count,
            paidOrdersCount,
            partiallyPaidOrdersCount,
            unpaidOrdersCount,
            dailyBreakdown);
    }
}
