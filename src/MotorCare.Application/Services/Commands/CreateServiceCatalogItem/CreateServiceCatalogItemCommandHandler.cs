using MediatR;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Services;

namespace MotorCare.Application.Services.Commands.CreateServiceCatalogItem;

public sealed class CreateServiceCatalogItemCommandHandler : IRequestHandler<CreateServiceCatalogItemCommand, Guid>
{
    private readonly IServiceCatalogRepository _repository;
    private readonly ITenantProvider _tenantProvider;

    public CreateServiceCatalogItemCommandHandler(IServiceCatalogRepository repository, ITenantProvider tenantProvider)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateServiceCatalogItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var existing = await _repository.GetByNameAsync(tenantId, request.Name, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("Bu isimle bir hizmet zaten kayıtlı.");
        }

        var item = new ServiceCatalogItem(
            tenantId,
            request.Name,
            request.Category,
            request.Description,
            request.DefaultDurationMinutes,
            request.DefaultPrice,
            request.IsActive);

        await _repository.AddAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
