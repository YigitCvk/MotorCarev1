using MediatR;
using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Application.Dashboard.Queries.GetMonthlyRevenue;

public sealed class GetMonthlyRevenueQueryHandler : IRequestHandler<GetMonthlyRevenueQuery, List<MonthlyRevenueStat>>
{
    private readonly IDashboardReadService _dashboardReadService;
    private readonly ITenantProvider _tenantProvider;

    public GetMonthlyRevenueQueryHandler(
        IDashboardReadService dashboardReadService,
        ITenantProvider tenantProvider)
    {
        _dashboardReadService = dashboardReadService;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<MonthlyRevenueStat>> Handle(GetMonthlyRevenueQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        return await _dashboardReadService.GetMonthlyRevenueAsync(tenantId, cancellationToken);
    }
}
