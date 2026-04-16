using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Domain.Repositories;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceOrder?> GetByOrderNoAsync(string tenantId, string orderNo, CancellationToken cancellationToken = default);
    Task AddAsync(ServiceOrder order, CancellationToken cancellationToken = default);
    void Update(ServiceOrder order);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
