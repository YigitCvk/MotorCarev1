using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Inventory;
using MotorCare.Domain.Repositories;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<InventoryItem?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        return _context.Set<InventoryItem>()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
    }

    public Task<InventoryItem?> GetBySkuAsync(string tenantId, string sku, CancellationToken cancellationToken = default)
    {
        var normalized = sku.Trim().ToLower();
        return _context.Set<InventoryItem>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Sku != null && x.Sku.ToLower() == normalized, cancellationToken);
    }

    public Task<InventoryItem?> GetByBarcodeAsync(string tenantId, string barcode, CancellationToken cancellationToken = default)
    {
        var normalized = barcode.Trim().ToLower();
        return _context.Set<InventoryItem>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Barcode != null && x.Barcode.ToLower() == normalized, cancellationToken);
    }

    public Task<InventoryItem?> GetByNameAsync(string tenantId, string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();
        return _context.Set<InventoryItem>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task<(List<InventoryItem> Items, int TotalCount)> GetPagedAsync(
        string tenantId,
        string? searchText,
        string? category,
        bool? isActive,
        bool lowStockOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<InventoryItem>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var pattern = $"%{searchText.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, pattern) ||
                (x.Sku != null && EF.Functions.ILike(x.Sku, pattern)) ||
                (x.Barcode != null && EF.Functions.ILike(x.Barcode, pattern)) ||
                (x.Category != null && EF.Functions.ILike(x.Category, pattern)) ||
                (x.Brand != null && EF.Functions.ILike(x.Brand, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim().ToLower();
            query = query.Where(x => x.Category != null && x.Category.ToLower() == normalizedCategory);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (lowStockOnly)
        {
            query = query.Where(x => x.StockQuantity <= x.MinimumStockLevel);
        }

        query = query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetActiveAsync(string tenantId, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Set<InventoryItem>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderBy(x => x.Name)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
        => _context.Set<InventoryItem>().AddAsync(item, cancellationToken).AsTask();

    public void Update(InventoryItem item)
        => _context.Set<InventoryItem>().Update(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
