using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Services.Commands.UpdateServiceCatalogItem;

public sealed class UpdateServiceCatalogItemCommandHandler : IRequestHandler<UpdateServiceCatalogItemCommand>
{
    private readonly IServiceCatalogRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateServiceCatalogItemCommandHandler> _logger;

    public UpdateServiceCatalogItemCommandHandler(
        IServiceCatalogRepository repository,
        ITenantProvider tenantProvider,
        ILogger<UpdateServiceCatalogItemCommandHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(UpdateServiceCatalogItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var item = await _repository.GetByIdAsync(request.Id, tenantId, cancellationToken)
            ?? throw new NotFoundException("ServiceCatalogItem", request.Id);

        var existing = await _repository.GetByNameAsync(tenantId, request.Name, cancellationToken);
        if (existing is not null && existing.Id != request.Id)
        {
            throw new ConflictException("Bu isimle bir hizmet zaten kayıtlı.");
        }

        item.Update(
            request.Name,
            request.Category,
            request.Description,
            request.DefaultDurationMinutes,
            request.Price,
            request.Currency,
            request.IsActive);

        _repository.Update(item);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            EventIdStore.ServiceCatalog.ServiceCatalogItemUpdated,
            "Service catalog item {ServiceCatalogItemId} updated for tenant {TenantId}. Category={Category} IsActive={IsActive}",
            item.Id,
            tenantId,
            item.Category,
            item.IsActive);
    }
}
