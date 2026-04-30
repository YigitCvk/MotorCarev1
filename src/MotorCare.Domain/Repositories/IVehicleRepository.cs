using MotorCare.Domain.Vehicles;

namespace MotorCare.Domain.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByPlateAsync(string tenantId, string normalizedPlate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetByCustomerIdAsync(string tenantId, Guid customerId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, Vehicle>> GetByIdsAsync(IEnumerable<Guid> ids, string tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    void Update(Vehicle vehicle);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IMotorcycleModelCatalogRepository
{
    Task<IReadOnlyList<string>> SearchBrandsAsync(string? search, int maxResults, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotorcycleModelCatalogItem>> SearchModelsAsync(string brand, string? search, int maxResults, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotorcycleModelCatalogItem>> SearchAsync(string? query, int maxResults, CancellationToken cancellationToken = default);
}
