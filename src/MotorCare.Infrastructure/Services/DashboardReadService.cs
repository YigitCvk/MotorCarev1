using Microsoft.EntityFrameworkCore;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Infrastructure.Persistence;

namespace MotorCare.Infrastructure.Services;

public sealed class DashboardReadService : IDashboardReadService
{
    private readonly ApplicationDbContext _context;

    public DashboardReadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardOverviewReadModel> GetOverviewAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var (todayStart, todayEnd, monthStart, monthEnd) = GetIstanbulRanges();

        var totalCustomerCount = await _context.Customers
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId, cancellationToken);

        var totalVehicleCount = await _context.Vehicles
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId, cancellationToken);

        var openServiceOrderCount = await _context.ServiceOrders
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     (x.Status == ServiceOrderStatus.Open ||
                      x.Status == ServiceOrderStatus.InProgress ||
                      x.Status == ServiceOrderStatus.WaitingForParts),
                cancellationToken);

        var todayAppointmentCount = await _context.Appointments
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.StartAt >= todayStart &&
                     x.StartAt < todayEnd,
                cancellationToken);

        var completedServiceCountThisMonth = await _context.ServiceOrders
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     (x.Status == ServiceOrderStatus.Completed || x.Status == ServiceOrderStatus.Delivered) &&
                     x.ClosedAt.HasValue &&
                     x.ClosedAt.Value >= monthStart &&
                     x.ClosedAt.Value < monthEnd,
                cancellationToken);

        var totalPaymentsThisMonth = await _context.ServiceOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .SelectMany(x => x.Payments)
            .Where(x => x.PaymentDate >= monthStart && x.PaymentDate < monthEnd)
            .Select(x => (decimal?)x.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var criticalInspectionCount = await _context.MotorcycleInspections
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.Items.Any(item =>
                         item.Result == MotorcycleInspectionResult.Damaged ||
                         item.Result == MotorcycleInspectionResult.Changed),
                cancellationToken);

        var totalServiceOrdersToday = await _context.ServiceOrders
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.OpenedAt >= todayStart &&
                     x.OpenedAt < todayEnd,
                cancellationToken);

        var completedServiceOrdersToday = await _context.ServiceOrders
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     (x.Status == ServiceOrderStatus.Completed || x.Status == ServiceOrderStatus.Delivered) &&
                     x.ClosedAt.HasValue &&
                     x.ClosedAt.Value >= todayStart &&
                     x.ClosedAt.Value < todayEnd,
                cancellationToken);

        var inProgressServiceOrdersCount = await _context.ServiceOrders
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.Status == ServiceOrderStatus.InProgress,
                cancellationToken);

        var totalPaymentsToday = await _context.ServiceOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .SelectMany(x => x.Payments)
            .Where(x => x.PaymentDate >= todayStart && x.PaymentDate < todayEnd)
            .Select(x => (decimal?)x.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var recentServiceOrders = await _context.ServiceOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.OpenedAt)
            .Take(5)
            .Select(x => new DashboardRecentServiceOrderReadModel(
                x.Id,
                x.OrderNo,
                x.Status.ToString(),
                x.OpenedAt,
                x.GrandTotal))
            .ToListAsync(cancellationToken);

        return new DashboardOverviewReadModel(
            totalCustomerCount,
            totalVehicleCount,
            openServiceOrderCount,
            todayAppointmentCount,
            completedServiceCountThisMonth,
            totalPaymentsThisMonth,
            criticalInspectionCount,
            totalServiceOrdersToday,
            completedServiceOrdersToday,
            inProgressServiceOrdersCount,
            totalPaymentsToday,
            openServiceOrderCount,
            recentServiceOrders);
    }

    private static (DateTimeOffset DayStart, DateTimeOffset DayEnd, DateTimeOffset MonthStart, DateTimeOffset MonthEnd) GetIstanbulRanges()
    {
        var zone = ResolveIstanbulTimeZone();
        var nowInZone = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        var localDate = nowInZone.Date;
        var dayStartLocal = new DateTime(localDate.Year, localDate.Month, localDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var dayEndLocal = dayStartLocal.AddDays(1);
        var monthStartLocal = new DateTime(localDate.Year, localDate.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var monthEndLocal = monthStartLocal.AddMonths(1);

        return (
            new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, zone)),
            new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, zone)),
            new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(monthStartLocal, zone)),
            new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(monthEndLocal, zone)));
    }

    private static TimeZoneInfo ResolveIstanbulTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
    }
}
