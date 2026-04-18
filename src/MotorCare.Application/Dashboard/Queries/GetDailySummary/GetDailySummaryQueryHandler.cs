using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Dashboard.Queries.GetDailySummary;

public class GetDailySummaryQueryHandler : IRequestHandler<GetDailySummaryQuery, DailySummaryDto>
{
    private readonly IServiceOrderRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetDailySummaryQueryHandler(IServiceOrderRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<DailySummaryDto> Handle(GetDailySummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var summary = await _repository.GetDailySummaryAsync(tenantId, cancellationToken);

        return new DailySummaryDto(
            summary.TotalServiceOrdersToday,
            summary.OpenServiceOrders,
            summary.CompletedServiceOrdersToday,
            summary.TotalPaymentsToday,
            summary.TotalRevenueToday,
            summary.PendingAmount);
    }
}
