using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Customers;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;

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
        var phone = PhoneNumber.Create(normalizedPhone);

        return await _context.Customers
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Phone == phone, cancellationToken);
    }

    public async Task<List<Customer>> SearchAsync(string tenantId, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers
            .Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            var termPattern = $"%{term}%";
            var normalizedPhoneTerm = new string(term.Where(char.IsDigit).ToArray());
            PhoneNumber? phoneTerm = null;

            if (normalizedPhoneTerm.Length >= 10)
            {
                phoneTerm = PhoneNumber.Create(normalizedPhoneTerm);
            }

            query = query.Where(c =>
                EF.Functions.ILike(c.FullName, termPattern) ||
                (c.Email != null && EF.Functions.ILike(c.Email, termPattern)) ||
                (phoneTerm != null && c.Phone == phoneTerm));
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
