using MotorCare.Domain.Inventory;

namespace MotorCare.Domain.Repositories;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetBySkuAsync(string tenantId, string sku, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByBarcodeAsync(string tenantId, string barcode, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByNameAsync(string tenantId, string name, CancellationToken cancellationToken = default);
    Task<(List<InventoryItem> Items, int TotalCount)> GetPagedAsync(
        string tenantId,
        string? searchText,
        string? category,
        bool? isActive,
        bool lowStockOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> GetActiveAsync(string tenantId, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default);
    void Update(InventoryItem item);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
