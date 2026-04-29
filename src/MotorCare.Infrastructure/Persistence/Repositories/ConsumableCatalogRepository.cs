using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public sealed class ConsumableCatalogRepository : IConsumableCatalogRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ConsumableCatalogRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ConsumableCatalogItem>> SearchAsync(
        string? query,
        string? category,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        var q = _dbContext.Set<ConsumableCatalogItem>()
            .Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim();
            q = q.Where(c => c.Category == cat);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var search = $"%{query.Trim().ToLower()}%";
            q = q.Where(c => 
                EF.Functions.ILike(c.ProductName, search) ||
                EF.Functions.ILike(c.Brand, search) ||
                EF.Functions.ILike(c.Category, search) ||
                (c.SubCategory != null && EF.Functions.ILike(c.SubCategory, search)));
        }

        return await q
            .OrderByDescending(c => c.UsageCount)
            .ThenBy(c => c.Brand)
            .ThenBy(c => c.ProductName)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public Task<ConsumableCatalogItem?> GetExactMatchAsync(
        string category,
        string brand,
        string productName,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<ConsumableCatalogItem>()
            .FirstOrDefaultAsync(c =>
                c.Category == category.Trim() &&
                c.Brand == brand.Trim() &&
                c.ProductName == productName.Trim(),
                cancellationToken);
    }

    public Task AddAsync(ConsumableCatalogItem item, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<ConsumableCatalogItem>().AddAsync(item, cancellationToken).AsTask();
    }

    public void Update(ConsumableCatalogItem item)
    {
        _dbContext.Set<ConsumableCatalogItem>().Update(item);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
