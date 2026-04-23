using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ValueObjects;

namespace MotorCare.Application.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Unit>
{
    private readonly ICustomerRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(
        ICustomerRepository repository,
        ITenantProvider tenantProvider,
        ILogger<UpdateCustomerCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var customer = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Customers.Customer), request.Id);

        var phone = PhoneNumber.Create(request.Phone);

        var existing = await _repository.GetByPhoneAsync(tenantId, phone.Value, cancellationToken);
        if (existing != null && existing.Id != request.Id)
            throw new ConflictException($"A customer with phone '{phone.Value}' already exists.");

        PhoneNumber? whatsapp = null;
        if (!string.IsNullOrWhiteSpace(request.Whatsapp))
            whatsapp = PhoneNumber.Create(request.Whatsapp);

        customer.UpdateContactInfo(request.FullName, phone, request.Email, whatsapp);
        customer.UpdateNotes(request.Notes);

        _repository.Update(customer);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.Customer.CustomerUpdated,
            "Customer updated. CustomerId={CustomerId}",
            request.Id);

        return Unit.Value;
    }
}
