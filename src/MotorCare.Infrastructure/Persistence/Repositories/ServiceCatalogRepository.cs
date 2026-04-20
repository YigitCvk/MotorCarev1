using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Services;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public sealed class ServiceCatalogRepository : IServiceCatalogRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceCatalogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceCatalogItem?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ServiceCatalogItem>()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<ServiceCatalogItem?> GetByNameAsync(string tenantId, string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        return await _context.Set<ServiceCatalogItem>()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Name.ToLower() == normalizedName,
                cancellationToken);
    }

    public async Task<(List<ServiceCatalogItem> Items, int TotalCount)> GetPagedAsync(
        string tenantId,
        string? searchText,
        ServiceCategory? category,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<ServiceCatalogItem>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var pattern = $"%{searchText.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, pattern) ||
                (x.Description != null && EF.Functions.ILike(x.Description, pattern)));
        }

        if (category.HasValue)
        {
            query = query.Where(x => x.Category == category.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        query = query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ServiceCatalogItem item, CancellationToken cancellationToken = default)
    {
        await _context.Set<ServiceCatalogItem>().AddAsync(item, cancellationToken);
    }

    public void Update(ServiceCatalogItem item)
    {
        _context.Set<ServiceCatalogItem>().Update(item);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
