using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Tenants;

namespace MotorCare.Application.Tenants.Commands.CreateTenant;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _repository;

    public CreateTenantCommandHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdentifierAsync(request.Identifier, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("A tenant with this identifier already exists.");
        }

        var tenant = new Tenant(request.Identifier, request.Name);

        await _repository.AddAsync(tenant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}
