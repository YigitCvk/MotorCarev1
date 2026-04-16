using MotorCare.Domain.Customers;

namespace MotorCare.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    void Update(Customer customer);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
