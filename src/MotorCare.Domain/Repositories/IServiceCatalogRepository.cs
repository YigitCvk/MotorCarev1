using MotorCare.Domain.Enums;
using MotorCare.Domain.Services;

namespace MotorCare.Domain.Repositories;

public interface IServiceCatalogRepository
{
    Task<ServiceCatalogItem?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<ServiceCatalogItem?> GetByNameAsync(string tenantId, string name, CancellationToken cancellationToken = default);
    Task<(List<ServiceCatalogItem> Items, int TotalCount)> GetPagedAsync(
        string tenantId,
        string? searchText,
        ServiceCategory? category,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(ServiceCatalogItem item, CancellationToken cancellationToken = default);
    void Update(ServiceCatalogItem item);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
