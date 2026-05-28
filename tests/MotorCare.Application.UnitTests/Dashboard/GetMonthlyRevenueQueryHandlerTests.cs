using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Dashboard.Queries.GetMonthlyRevenue;

namespace MotorCare.Application.UnitTests.Dashboard;

public class GetMonthlyRevenueQueryHandlerTests
{
    private readonly IDashboardReadService _readService = Substitute.For<IDashboardReadService>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();

    private readonly GetMonthlyRevenueQueryHandler _handler;

    private const string TenantId = "test-tenant";

    public GetMonthlyRevenueQueryHandlerTests()
    {
        _tenantProvider.GetTenantId().Returns(TenantId);
        _handler = new GetMonthlyRevenueQueryHandler(_readService, _tenantProvider);
    }

    [Fact]
    public async Task Handle_ReturnsStats_FromReadService()
    {
        var expected = new List<MonthlyRevenueStat>
        {
            new("2025-01", 12450.00m, 8),
            new("2025-02", 9800.00m, 6),
        };
        _readService.GetMonthlyRevenueAsync(TenantId, default).Returns(expected);

        var result = await _handler.Handle(new GetMonthlyRevenueQuery(), default);

        result.Should().BeEquivalentTo(expected);
        await _readService.Received(1).GetMonthlyRevenueAsync(TenantId, default);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoOrders()
    {
        _readService.GetMonthlyRevenueAsync(TenantId, default).Returns(new List<MonthlyRevenueStat>());

        var result = await _handler.Handle(new GetMonthlyRevenueQuery(), default);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenTenantMissing()
    {
        _tenantProvider.GetTenantId().Returns((string?)null);

        var act = async () => await _handler.Handle(new GetMonthlyRevenueQuery(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
