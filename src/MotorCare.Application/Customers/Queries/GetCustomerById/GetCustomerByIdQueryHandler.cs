using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public GetCustomerByIdQueryHandler(ICustomerRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var customer = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        return new CustomerDto(
            customer.Id,
            customer.FullName,
            customer.Phone?.Value,
            customer.Email,
            customer.Whatsapp?.Value,
            customer.Notes);
    }
}
