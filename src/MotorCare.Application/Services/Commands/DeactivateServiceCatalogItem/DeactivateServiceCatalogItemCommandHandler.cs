using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Services.Commands.DeactivateServiceCatalogItem;

public sealed class DeactivateServiceCatalogItemCommandHandler : IRequestHandler<DeactivateServiceCatalogItemCommand>
{
    private readonly IServiceCatalogRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public DeactivateServiceCatalogItemCommandHandler(IServiceCatalogRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(DeactivateServiceCatalogItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var item = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("ServiceCatalogItem", request.Id);

        item.Deactivate();
        _repository.Update(item);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
