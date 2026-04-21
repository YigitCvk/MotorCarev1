using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Inspections;
using MotorCare.Domain.Repositories;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public sealed class MotorcycleInspectionRepository : IMotorcycleInspectionRepository
{
    private readonly ApplicationDbContext _context;

    public MotorcycleInspectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<MotorcycleInspection?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        return _context.Set<MotorcycleInspection>()
            .AsSplitQuery()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
    }

    public Task<MotorcycleInspection?> GetByInspectionNoAsync(string tenantId, string inspectionNo, CancellationToken cancellationToken = default)
    {
        return _context.Set<MotorcycleInspection>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.InspectionNo == inspectionNo, cancellationToken);
    }

    public Task<int> GetTodayCountAsync(string tenantId, DateTimeOffset dayStartUtc, DateTimeOffset dayEndUtc, CancellationToken cancellationToken = default)
    {
        return _context.Set<MotorcycleInspection>()
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId &&
                     x.CreatedAt >= dayStartUtc &&
                     x.CreatedAt < dayEndUtc,
                cancellationToken);
    }

    public async Task<(List<MotorcycleInspection> Items, int TotalCount)> GetPagedAsync(
        string tenantId,
        string? searchText,
        MotorcycleInspectionPackageType? packageType,
        MotorcycleInspectionStatus? status,
        Guid? customerId,
        Guid? vehicleId,
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<MotorcycleInspection>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var pattern = $"%{searchText.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.CustomerName, pattern) ||
                EF.Functions.ILike(x.Phone, pattern) ||
                EF.Functions.ILike(x.Plate, pattern) ||
                EF.Functions.ILike(x.InspectionNo, pattern));
        }

        if (packageType.HasValue)
        {
            query = query.Where(x => x.PackageType == packageType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        if (vehicleId.HasValue)
        {
            query = query.Where(x => x.VehicleId == vehicleId.Value);
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= createdTo.Value);
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task AddAsync(MotorcycleInspection inspection, CancellationToken cancellationToken = default)
        => _context.Set<MotorcycleInspection>().AddAsync(inspection, cancellationToken).AsTask();

    public void Update(MotorcycleInspection inspection)
        => _context.Set<MotorcycleInspection>().Update(inspection);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
