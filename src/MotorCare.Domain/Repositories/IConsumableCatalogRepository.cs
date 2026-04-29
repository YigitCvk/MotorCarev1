using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Domain.Repositories;

public interface IConsumableCatalogRepository
{
    Task<IReadOnlyList<ConsumableCatalogItem>> SearchAsync(
        string? query,
        string? category,
        int maxResults,
        CancellationToken cancellationToken = default);

    Task<ConsumableCatalogItem?> GetExactMatchAsync(
        string category,
        string brand,
        string productName,
        CancellationToken cancellationToken = default);

    Task AddAsync(ConsumableCatalogItem item, CancellationToken cancellationToken = default);

    void Update(ConsumableCatalogItem item);
}
