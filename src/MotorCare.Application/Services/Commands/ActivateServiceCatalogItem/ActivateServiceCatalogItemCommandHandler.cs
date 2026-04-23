using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Services.Commands.ActivateServiceCatalogItem;

public sealed class ActivateServiceCatalogItemCommandHandler : IRequestHandler<ActivateServiceCatalogItemCommand>
{
    private readonly IServiceCatalogRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<ActivateServiceCatalogItemCommandHandler> _logger;

    public ActivateServiceCatalogItemCommandHandler(
        IServiceCatalogRepository repository,
        ITenantProvider tenantProvider,
        ILogger<ActivateServiceCatalogItemCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(ActivateServiceCatalogItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var item = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("ServiceCatalogItem", request.Id);

        item.Activate();
        _repository.Update(item);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceCatalog.ServiceCatalogItemActivated,
            "Service catalog item activated. ItemId={ItemId}",
            item.Id);
    }
}
