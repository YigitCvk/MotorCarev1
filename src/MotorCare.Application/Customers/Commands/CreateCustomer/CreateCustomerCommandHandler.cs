using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Customers;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;

namespace MotorCare.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(
        ICustomerRepository repository,
        ITenantProvider tenantProvider,
        ILogger<CreateCustomerCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var phone = PhoneNumber.Create(request.Phone);

        var existing = await _repository.GetByPhoneAsync(tenantId, phone.Value, cancellationToken);
        if (existing != null)
            throw new ConflictException($"A customer with phone '{phone.Value}' already exists.");

        PhoneNumber? whatsapp = null;
        if (!string.IsNullOrWhiteSpace(request.Whatsapp))
            whatsapp = PhoneNumber.Create(request.Whatsapp);

        var customer = new Customer(tenantId, request.FullName, phone, request.Email, whatsapp, request.Notes);

        await _repository.AddAsync(customer, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Customer.CustomerCreated,
            "Customer created. CustomerId={CustomerId} FullName={FullName}",
            customer.Id,
            request.FullName);

        return customer.Id;
    }
}
