using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Enums;
using MotorCare.Domain.ServiceOrders;
using MotorCare.Domain.Repositories;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceOrders
            .AsSplitQuery()
            .Include(o => o.Operations)
            .Include(o => o.Parts)
            .Include(o => o.Consumables)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId, cancellationToken);
    }

    public async Task<ServiceOrder?> GetByOrderNoAsync(string tenantId, string orderNo, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceOrders
            .AsSplitQuery()
            .Include(o => o.Operations)
            .Include(o => o.Parts)
            .Include(o => o.Consumables)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.OrderNo == orderNo, cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetAllAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceOrders
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceOrderDailySummary> GetDailySummaryAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var (todayStart, todayEnd) = GetIstanbulDayRange();

        var totalServiceOrdersToday = await _context.ServiceOrders
            .Where(o => o.TenantId == tenantId && o.OpenedAt >= todayStart && o.OpenedAt < todayEnd)
            .CountAsync(cancellationToken);

        var activeServiceOrders = await _context.ServiceOrders
            .Where(o =>
                o.TenantId == tenantId &&
                (o.Status == ServiceOrderStatus.Open ||
                 o.Status == ServiceOrderStatus.InProgress ||
                 o.Status == ServiceOrderStatus.WaitingForParts))
            .CountAsync(cancellationToken);

        var completedServiceOrdersToday = await _context.ServiceOrders
            .Where(o =>
                o.TenantId == tenantId &&
                (o.Status == ServiceOrderStatus.Completed || o.Status == ServiceOrderStatus.Delivered) &&
                o.ClosedAt.HasValue &&
                o.ClosedAt.Value >= todayStart &&
                o.ClosedAt.Value < todayEnd)
            .CountAsync(cancellationToken);

        var deliveryWaitingCount = await _context.ServiceOrders
            .Where(o => o.TenantId == tenantId && o.Status == ServiceOrderStatus.Completed)
            .CountAsync(cancellationToken);

        var totalPaymentsToday = await _context.ServiceOrders
            .Where(o => o.TenantId == tenantId)
            .SelectMany(o => o.Payments)
            .Where(p => p.PaymentDate >= todayStart && p.PaymentDate < todayEnd)
            .Select(p => (decimal?)p.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var totalRevenueToday = await _context.ServiceOrders
            .Where(o => o.TenantId == tenantId && o.OpenedAt >= todayStart && o.OpenedAt < todayEnd)
            .Select(o => (decimal?)o.GrandTotal)
            .SumAsync(cancellationToken) ?? 0m;

        var pendingAmount = await _context.ServiceOrders
            .Where(o => o.TenantId == tenantId && o.Status != ServiceOrderStatus.Cancelled && (o.GrandTotal - o.PaidTotal) > 0)
            .Select(o => (decimal?)(o.GrandTotal - o.PaidTotal))
            .SumAsync(cancellationToken) ?? 0m;

        return new ServiceOrderDailySummary(
            totalServiceOrdersToday,
            activeServiceOrders,
            completedServiceOrdersToday,
            deliveryWaitingCount,
            totalPaymentsToday,
            totalRevenueToday,
            pendingAmount);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetIstanbulDayRange()
    {
        var zone = ResolveIstanbulTimeZone();
        var nowInZone = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        var localDate = nowInZone.Date;
        var startLocal = new DateTime(localDate.Year, localDate.Month, localDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var endLocal = startLocal.AddDays(1);

        var startUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(startLocal, zone));
        var endUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(endLocal, zone));
        return (startUtc, endUtc);
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

    public async Task<List<ServiceOrder>> GetFilteredAsync(
        string tenantId,
        Guid? customerId,
        ServiceOrderStatus? status,
        string? searchText,
        DateTimeOffset? openedFrom,
        DateTimeOffset? openedTo,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ServiceOrders
            .AsNoTracking()
            .Where(o => o.TenantId == tenantId);

        if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            var normalizedPlate = new string(searchText.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            var matchingCustomerIds = _context.Customers
                .Where(c => c.TenantId == tenantId && EF.Functions.ILike(c.FullName, $"%{searchText.Trim()}%"))
                .Select(c => c.Id);
            var matchingVehicleIds = _context.Vehicles
                .Where(v => v.TenantId == tenantId && v.Plate.NormalizedValue.Contains(normalizedPlate))
                .Select(v => v.Id);

            query = query.Where(o =>
                o.OrderNo.ToLower().Contains(term) ||
                (o.Complaint != null && o.Complaint.ToLower().Contains(term)) ||
                matchingCustomerIds.Contains(o.CustomerId) ||
                matchingVehicleIds.Contains(o.VehicleId));
        }

        if (openedFrom.HasValue)
        {
            query = query.Where(o => o.OpenedAt >= openedFrom.Value);
        }

        if (openedTo.HasValue)
        {
            query = query.Where(o => o.OpenedAt <= openedTo.Value);
        }

        return await query
            .AsSplitQuery()
            .Include(o => o.Operations)
            .Include(o => o.Parts)
            .Include(o => o.Consumables)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ServiceOrder> Items, int TotalCount)> GetFilteredPagedAsync(
        string tenantId,
        Guid? customerId,
        ServiceOrderStatus? status,
        string? searchText,
        DateTimeOffset? openedFrom,
        DateTimeOffset? openedTo,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ServiceOrders
            .AsNoTracking()
            .Where(o => o.TenantId == tenantId);

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            var normalizedPlate = new string(searchText.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            var matchingCustomerIds = _context.Customers
                .Where(c => c.TenantId == tenantId && EF.Functions.ILike(c.FullName, $"%{searchText.Trim()}%"))
                .Select(c => c.Id);
            var matchingVehicleIds = _context.Vehicles
                .Where(v => v.TenantId == tenantId && v.Plate.NormalizedValue.Contains(normalizedPlate))
                .Select(v => v.Id);

            query = query.Where(o =>
                o.OrderNo.ToLower().Contains(term) ||
                (o.Complaint != null && o.Complaint.ToLower().Contains(term)) ||
                matchingCustomerIds.Contains(o.CustomerId) ||
                matchingVehicleIds.Contains(o.VehicleId));
        }

        if (openedFrom.HasValue)
            query = query.Where(o => o.OpenedAt >= openedFrom.Value);

        if (openedTo.HasValue)
            query = query.Where(o => o.OpenedAt <= openedTo.Value);

        query = query.OrderByDescending(o => o.OpenedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<ServiceOrder> Items, int TotalCount)> GetByVehicleIdAsync(
        Guid vehicleId, string tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ServiceOrders
            .Where(o => o.VehicleId == vehicleId && o.TenantId == tenantId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.OpenedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetTodayOrderCountAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var todayStart = DateTimeOffset.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        return await _context.ServiceOrders
            .Where(o => o.TenantId == tenantId && o.OpenedAt >= todayStart && o.OpenedAt < todayEnd)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(ServiceOrder order, CancellationToken cancellationToken = default)
    {
        await _context.ServiceOrders.AddAsync(order, cancellationToken);
    }

    public void Update(ServiceOrder order)
    {
        var entry = _context.Entry(order);
        if (entry.State == EntityState.Detached)
        {
            _context.ServiceOrders.Update(order);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
