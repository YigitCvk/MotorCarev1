using MediatR;
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

    public CreateCustomerCommandHandler(ICustomerRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
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

        return customer.Id;
    }
}
