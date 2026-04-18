using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Customers;
using MotorCare.Domain.Repositories;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);
    }

    public async Task<Customer?> GetByPhoneAsync(string tenantId, string normalizedPhone, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Phone != null && c.Phone.Value == normalizedPhone, cancellationToken);
    }

    public async Task<List<Customer>> SearchAsync(string tenantId, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers
            .Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(term) ||
                (c.Email != null && c.Email.ToLower().Contains(term)) ||
                (c.Phone != null && c.Phone.Value.Contains(term)));
        }

        return await query.OrderBy(c => c.FullName).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
