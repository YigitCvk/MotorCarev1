using MotorCare.Domain.Tenants;

namespace MotorCare.Domain.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    void Update(Tenant tenant);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
