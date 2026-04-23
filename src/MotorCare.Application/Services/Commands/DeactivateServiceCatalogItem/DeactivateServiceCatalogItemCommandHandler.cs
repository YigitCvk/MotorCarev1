using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Services.Commands.DeactivateServiceCatalogItem;

public sealed class DeactivateServiceCatalogItemCommandHandler : IRequestHandler<DeactivateServiceCatalogItemCommand>
{
    private readonly IServiceCatalogRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<DeactivateServiceCatalogItemCommandHandler> _logger;

    public DeactivateServiceCatalogItemCommandHandler(
        IServiceCatalogRepository repository,
        ITenantProvider tenantProvider,
        ILogger<DeactivateServiceCatalogItemCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
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

        _logger.LogInformation(
            EventIdStore.ServiceCatalog.ServiceCatalogItemDeactivated,
            "Service catalog item deactivated. ItemId={ItemId}",
            item.Id);
    }
}
