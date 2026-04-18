using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Customers.Queries.GetCustomerById;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Customers.Queries.SearchCustomers;

public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public SearchCustomersQueryHandler(ICustomerRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<CustomerDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var customers = await _repository.SearchAsync(tenantId, request.SearchTerm, cancellationToken);

        return customers
            .Select(customer => new CustomerDto(
                customer.Id,
                customer.FullName,
                customer.Phone?.Value,
                customer.Email,
                customer.Whatsapp?.Value,
                customer.Notes))
            .ToList();
    }
}
