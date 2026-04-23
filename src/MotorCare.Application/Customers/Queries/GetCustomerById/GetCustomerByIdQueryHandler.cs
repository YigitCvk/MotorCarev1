using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;

    public GetCustomerByIdQueryHandler(
        ICustomerRepository repository,
        ITenantProvider tenantProvider,
        ILogger<GetCustomerByIdQueryHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
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

        _logger.LogInformation(
            EventIdStore.Customer.CustomerFetched,
            "Customer {CustomerId} fetched for tenant {TenantId}.",
            customer.Id,
            tenantId);

        return new CustomerDto(
            customer.Id,
            customer.FullName,
            customer.Phone?.Value,
            customer.Email,
            customer.Whatsapp?.Value,
            customer.Notes);
    }
}
