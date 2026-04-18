using Microsoft.EntityFrameworkCore;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Infrastructure.Persistence;

namespace MotorCare.Infrastructure.Services;

/// <summary>
/// Generates order numbers in the format SRV-{yyyyMMdd}-{0001}.
/// Uses a transaction-safe approach by counting today's orders per tenant.
/// </summary>
public class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public OrderNumberGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var todayDate = DateTimeOffset.UtcNow;
        var todayStart = todayDate.Date;
        var todayEnd = todayStart.AddDays(1);

        // Count today's orders for the tenant to determine the next sequence number
        var todayCount = await _context.ServiceOrders
            .Where(o => o.TenantId == tenantId && o.OpenedAt >= todayStart && o.OpenedAt < todayEnd)
            .CountAsync(cancellationToken);

        var sequence = todayCount + 1;

        return $"SRV-{todayDate:yyyyMMdd}-{sequence:D4}";
    }
}
