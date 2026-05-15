using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Dashboard.Queries.GetDailySummary;

public sealed class GetDailySummaryQueryHandler : IRequestHandler<GetDailySummaryQuery, DailySummaryDto>
{
    private readonly IDashboardReadService _dashboardReadService;
    private readonly ITenantProvider _tenantProvider;

    public GetDailySummaryQueryHandler(
        IDashboardReadService dashboardReadService,
        ITenantProvider tenantProvider)
    {
        _dashboardReadService = dashboardReadService;
        _tenantProvider = tenantProvider;
    }

    public async Task<DailySummaryDto> Handle(GetDailySummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var summary = await _dashboardReadService.GetOverviewAsync(tenantId, cancellationToken);

        return new DailySummaryDto(
            summary.TotalCustomerCount,
            summary.TotalVehicleCount,
            summary.OpenServiceOrderCount,
            summary.TodayAppointmentCount,
            summary.CompletedServiceCountThisMonth,
            summary.TotalPaymentsThisMonth,
            summary.CriticalInspectionCount,
            summary.TotalServiceOrdersToday,
            summary.CompletedServiceOrdersToday,
            summary.InProgressServiceOrdersCount,
            summary.TotalPaymentsToday,
            summary.ActiveServiceOrdersCount,
            summary.RecentServiceOrders
                .Select(x => new RecentServiceOrderDto(
                    x.Id,
                    x.OrderNo,
                    x.Status,
                    x.OpenedAt,
                    x.GrandTotal))
                .ToList());
    }
}
