using MotorCare.Domain.Customers;

namespace MotorCare.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByPhoneAsync(string tenantId, string normalizedPhone, CancellationToken cancellationToken = default);
    Task<List<Customer>> SearchAsync(string tenantId, string? searchTerm, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    void Update(Customer customer);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
